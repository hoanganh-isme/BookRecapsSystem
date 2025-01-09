using Microsoft.Extensions.Logging;
using Services.Interface;
using System;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using Repository;
using Services.Service.Helper;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Services.Service.Webhook;
using Net.payOS.Types;
using BusinessObject.ViewModels.Subscription;
using Services.Responses;

namespace Services.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PayOSService _payOSService;
        private readonly string _checksumKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionService> _logger; // Inject ILogger

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, PayOSService payOSService,
            IConfiguration configuration, ILogger<TransactionService> logger)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _payOSService = payOSService;
            _checksumKey = _configuration["PayOS:ChecksumKey"];
            _logger = logger; // Assign logger to field
        }

        public async Task<ApiResponse<string>> CreateTransactionAsync(Guid userId, Guid subscriptionPackageId)
        {
            _logger.LogInformation("Starting CreateTransactionAsync for user {UserId} with package {SubscriptionPackageId}", userId, subscriptionPackageId);

            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                _logger.LogWarning("Subscription package {SubscriptionPackageId} not found", subscriptionPackageId);
                return new ApiResponse<string> { Succeeded = false, Message = "Subscription package not found." };
            }

            var transaction = new BusinessObject.Models.Transaction
            {
                UserId = userId,
                SubscriptionPackageId = subscriptionPackageId,
                Price = subscriptionPackage.Price,
                Status = TransactionsStatus.Pending,
                OrderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")),
                PaymentMethod = "PayOS"
            };

            var payResponse = await _payOSService.CreateTransactionAsync(transaction, subscriptionPackage);

            if (!payResponse.IsSuccess)
            {
                _logger.LogError("Failed to create transaction with PayOS: {ErrorMessage}", payResponse.ErrorMessage);
                return new ApiResponse<string> { Succeeded = false, Message = "Failed to create transaction with PayOS.", Errors = new[] { payResponse.ErrorMessage } };
            }

            transaction.Status = TransactionsStatus.Processing;
            await _unitOfWork.TransactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transaction created successfully for user {UserId}, orderCode {OrderCode}", userId, transaction.OrderCode);

            return new ApiResponse<string>
            {
                Succeeded = true,
                Message = "Transaction created successfully.",
                Data = payResponse.CheckoutUrl
            };
        }

        public async Task<ApiResponse<bool>> CompleteTransactionAsync(long orderCode, WebhookType webhookRequest)
        {
            Console.WriteLine($"CompleteTransactionAsync: Start for OrderCode: {orderCode}");

            // Tìm giao dịch theo OrderCode
            var transaction = await _unitOfWork.TransactionRepository.FindByOrderCode((int)orderCode);
            if (transaction == null)
            {
                Console.WriteLine("Transaction not found.");
                return new ApiResponse<bool> { Succeeded = false, Message = "Transaction not found." };
            }

            // Kiểm tra kết quả thanh toán
            if (!webhookRequest.success)
            {
                Console.WriteLine("Payment failed: " + webhookRequest.desc);
                return new ApiResponse<bool> { Succeeded = false, Message = "Payment failed: " + webhookRequest.desc };
            }

            // Cập nhật trạng thái giao dịch thành "Paid"
            transaction.Status = TransactionsStatus.Paid;
            _unitOfWork.TransactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            Console.WriteLine($"Processing subscription for UserId: {transaction.UserId}");

            // Lấy thông tin subscription hiện tại của người dùng
            var existingSubscription = await _unitOfWork.SubscriptionRepository.GetByUserIdAsync(transaction.UserId);
            // Lấy thông tin gói đăng ký
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(transaction.SubscriptionPackageId);

            if (subscriptionPackage == null)
            {
                Console.WriteLine("Subscription package not found.");
                return new ApiResponse<bool> { Succeeded = false, Message = "Subscription package not found." };
            }

            DateOnly startDate;
            if (existingSubscription == null)
            {
                // Nếu không có subscription cũ, bắt đầu từ ngày hiện tại
                startDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                // Nếu có subscription cũ, sử dụng ngày kết thúc của subscription cũ làm ngày bắt đầu cho subscription mới
                startDate = existingSubscription.EndDate ?? DateOnly.FromDateTime(DateTime.Now);
            }

            DateOnly endDate = startDate.AddDays((int)subscriptionPackage.Duration);

            // Kiểm tra nếu không có subscription hiện tại (existingSubscription == null)
            if (existingSubscription == null)
            {
                // Nếu không có subscription trước đó, sử dụng ngày hiện tại làm StartDate
                var subscription = new CreateSubscription
                {
                    Price = transaction.Price,
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = transaction.UserId,
                    SubscriptionPackageId = transaction.SubscriptionPackageId,
                    TransactionId = transaction.Id,
                    ExpectedViewsCount = subscriptionPackage.ExpectedViewsCount,
                    ActualViewsCount = 0,
                    Durations = subscriptionPackage.Duration,
                    Status = SubStatus.Active, // Trạng thái là "Active" cho subscription mới
                };
                var newSubscription = _mapper.Map<Subscription>(subscription);
                await _unitOfWork.SubscriptionRepository.AddAsync(newSubscription);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Nếu có subscription hiện tại, lấy EndDate của subscription hiện tại làm StartDate cho subscription mới
                Console.WriteLine("Existing subscription found, creating new subscription with status NotStarted.");
                var subscription = new CreateSubscription
                {
                    Price = transaction.Price,
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = transaction.UserId,
                    SubscriptionPackageId = transaction.SubscriptionPackageId,
                    TransactionId = transaction.Id,
                    ExpectedViewsCount = subscriptionPackage.ExpectedViewsCount,
                    ActualViewsCount = 0,
                    Durations = subscriptionPackage.Duration,
                    Status = SubStatus.NotStarted,  // Trạng thái mới là "NotStarted"
                };
                var newSubscription = _mapper.Map<Subscription>(subscription);
                await _unitOfWork.SubscriptionRepository.AddAsync(newSubscription);
                await _unitOfWork.SaveChangesAsync();
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Transaction completed and subscription processed successfully.",
                Data = true
            };
        }




    }
}
