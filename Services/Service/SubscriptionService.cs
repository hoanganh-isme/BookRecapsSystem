using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Subscription;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Subscription>> CreateSubscription(Guid userId, Guid transactionId, Guid subscriptionPackageId)
        {
            // Lấy gói đăng ký từ repository
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                return new ApiResponse<Subscription>
                {
                    Succeeded = false,
                    Message = "Subscription package not found."
                };
            }

            // Lấy subscription hiện tại của user (nếu có)
            var existingSubscription = await _unitOfWork.SubscriptionRepository.GetByUserIdAsync(userId);
            DateOnly startDate;
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now).AddDays((int)subscriptionPackage.Duration);

            Subscription newSubscription;

            if (existingSubscription == null)
            {
                // Nếu không có subscription trước đó, sử dụng ngày hiện tại làm StartDate
                startDate = DateOnly.FromDateTime(DateTime.Now);
                var subscription = new CreateSubscription
                {
                    Price = subscriptionPackage.Price,
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId,
                    SubscriptionPackageId = subscriptionPackageId,
                    TransactionId = transactionId,
                    ExpectedViewsCount = subscriptionPackage.ExpectedViewsCount,
                    ActualViewsCount = 0,
                    Durations = subscriptionPackage.Duration,
                    Status = SubStatus.Active, // Trạng thái là "Active"
                };
                newSubscription = _mapper.Map<Subscription>(subscription);
                await _unitOfWork.SubscriptionRepository.AddAsync(newSubscription);
            }
            else
            {
                // Nếu có subscription hiện tại, lấy EndDate của subscription hiện tại làm StartDate cho subscription mới
                startDate = existingSubscription.EndDate.Value;
                var subscription = new CreateSubscription
                {
                    Price = subscriptionPackage.Price,
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId,
                    SubscriptionPackageId = subscriptionPackageId,
                    TransactionId = transactionId,
                    ExpectedViewsCount = subscriptionPackage.ExpectedViewsCount,
                    ActualViewsCount = 0,
                    Durations = subscriptionPackage.Duration,
                    Status = SubStatus.NotStarted, // Trạng thái là "NotStarted" cho subscription mới
                };
                newSubscription = _mapper.Map<Subscription>(subscription);
                await _unitOfWork.SubscriptionRepository.AddAsync(newSubscription);
            }

            // Lưu lại thay đổi vào database
            await _unitOfWork.SaveChangesAsync();

            // Trả về ApiResponse với thông tin subscription mới
            return new ApiResponse<Subscription>
            {
                Succeeded = true,
                Message = "Subscription created successfully.",
                Data = newSubscription
            };
        }
        public async Task<ApiResponse<SubscriptionHistory>> GetSubscriptionByUserId(Guid userId)
        {
            // Lấy Current Subscription
            var currentSubscription = await _unitOfWork.SubscriptionRepository.GetRemainViewByUserIdAsync(userId);

            var currentSubscriptionDto = currentSubscription != null
                ? new CurrentSubscriptionDto
                {
                    SubscriptionId = currentSubscription.Id,
                    PackageName = currentSubscription.SubscriptionPackage.Name,
                    Price = currentSubscription.Price,
                    StartDate = currentSubscription.StartDate,
                    EndDate = currentSubscription.EndDate,
                    ExpectedViewsCount = currentSubscription.ExpectedViewsCount,
                    ActualViewsCount = currentSubscription.ActualViewsCount,
                    CreateAt = currentSubscription.CreatedAt
                }
                : null;

            // Lấy History Subscriptions
            var historySubscriptions = await _unitOfWork.SubscriptionRepository.GetHistorySubscriptionsByUserIdAsync(userId);

            var historySubscriptionDtos = historySubscriptions.Select(s => new HistorySubscriptionDto
            {
                SubscriptionId = s.Id,
                PackageName = s.SubscriptionPackage.Name,
                Price = s.Price,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status,
                ExpectedViewsCount = s.ExpectedViewsCount,
                ActualViewsCount = s.ActualViewsCount,
                CreateAt = s.CreatedAt
                
            }).ToList();

            // Kiểm tra nếu không có dữ liệu
            if (currentSubscriptionDto == null && !historySubscriptionDtos.Any())
            {
                return new ApiResponse<SubscriptionHistory>
                {
                    Succeeded = false,
                    Message = "Người dùng chưa mua gói nghe premium nào."
                };
            }

            // Trả về dữ liệu
            return new ApiResponse<SubscriptionHistory>
            {
                Succeeded = true,
                Message = "Lấy thông tin subscription thành công.",
                Data = new SubscriptionHistory
                {
                    CurrentSubscription = currentSubscriptionDto,
                    HistorySubscriptions = historySubscriptionDtos
                }
            };
        }


        // Hàm kích hoạt subscription
        public async Task ActivateSubscription(Guid subscriptionId)
        {
            var subscription = await _unitOfWork.SubscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null) throw new Exception("Subscription not found");

            subscription.Status = SubStatus.Active;
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateSubscriptionStatusesAsync()
        {
            // Lấy danh sách các gói cần được cập nhật
            var subscriptions = await _unitOfWork.SubscriptionRepository.GetAllAsync();

            foreach (var subscription in subscriptions)
            {
                if (subscription.Status == SubStatus.NotStarted && subscription.StartDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    // Kích hoạt gói đăng ký khi đến ngày bắt đầu
                    subscription.Status = SubStatus.Active;
                }
                else if (subscription.Status == SubStatus.Active && subscription.EndDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    // Chuyển gói đăng ký sang trạng thái hết hạn khi ngày kết thúc đã qua
                    subscription.Status = SubStatus.Expired;
                }

                _unitOfWork.SubscriptionRepository.Update(subscription);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
