using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.PublisherPayouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repository;
using Services.Interface;
using Services.Responses;
using Services.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;
using static BusinessObject.ViewModels.PublisherPayouts.PublisherPayoutDTO;

namespace Services.Service
{
    public class PublisherPayoutService : IPublisherPayoutService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;
        private readonly GoogleCloudService _googleCloudService;
        public PublisherPayoutService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
             IMapper mapper,
            IOptions<GoogleSettings> googleSettings,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _googleCloudService = new GoogleCloudService(configuration);
        }
        public async Task<List<PublisherDto>> GetAllPublishersWithPayoutsAsync()
        {
            var publishers = await _unitOfWork.PublisherRepository.GetAllAsync();

            // Lấy thông tin payout mới nhất của mỗi Publisher
            var publisherPayouts = await _unitOfWork.PublisherPayoutRepository.GetLatestPayoutsForAllPublishersAsync();

            // Map danh sách Publishers từ users
            var usersDto = _mapper.Map<List<PublisherDto>>(publishers);

            // Kết hợp thông tin payouts
            foreach (var userDto in usersDto)
            {
                var payoutInfo = publisherPayouts.FirstOrDefault(p => p.PublisherId == userDto.Id);

                if (payoutInfo != null)
                {
                    // Gán thông tin payout mới nhất
                    userDto.payoutId = payoutInfo.Id;
                    userDto.Id= payoutInfo.PublisherId;
                    userDto.Fromdate = payoutInfo.FromDate;
                    userDto.Todate = payoutInfo.ToDate;
                    userDto.TotalEarnings = payoutInfo.Amount;
                    userDto.Status = payoutInfo.Status.ToString();
                }
                else
                {
                    // Gán giá trị mặc định nếu không có payout
                    userDto.Fromdate = null;
                    userDto.Todate = null;
                    userDto.TotalEarnings = 0;
                    userDto.Status = "Không có dữ liệu";
                }
            }

            return usersDto;
        }

        public async Task<ApiResponse<PublisherPayoutDto>> CreatePublisherPayoutAsync(Guid publisherId, Stream ImageStream, string ImageContentType, string description, DateTime toDate)
        {
            if (toDate > DateTime.UtcNow)
            {
                return new ApiResponse<PublisherPayoutDto>
                {
                    Succeeded = false,
                    Message = "Ngày quyết toán không lớn hơn ngày hiện tại.",
                    Data = null
                };
            }
            // Lấy danh sách sách của Publisher
            var books = await _unitOfWork.BookRepository.GetBooksByPublisherIdAsync(publisherId);
            if (books == null || !books.Any())
            {
                return new ApiResponse<PublisherPayoutDto>
                {
                    Succeeded = false,
                    Message = "No books found for this publisher.",
                    Data = null
                };
            }

            var recapIds = books.SelectMany(b => b.Recaps).Select(r => r.Id).ToList();

            // Xác định FromDate và ToDate
            var lastPayout = await _unitOfWork.PublisherPayoutRepository
                .GetLastPayoutByPublisherIdAsync(publisherId);
            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;

            // Lấy ViewTracking trong khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByRecapIdsAndDateRangeAsync(recapIds, fromDate, toDate);

            // Tính earnings cho từng Book dựa trên các Recap của nó
            var bookEarnings = new List<BookEarning>();
            foreach (var book in books)
            {
                var bookRecapIds = book.Recaps.Select(r => r.Id).ToList();
                var bookViewTrackings = viewTrackings.Where(vt => bookRecapIds.Contains(vt.RecapId));

                var bookEarningAmount = bookViewTrackings
                    .Where(vt => vt.PublisherValueShare.HasValue)
                    .Sum(vt => vt.PublisherValueShare.Value);

                bookEarnings.Add(new BookEarning
                {
                    BookId = book.Id,
                    EarningAmount = bookEarningAmount,
                    FromDate = fromDate,
                    ToDate = toDate
                });
            }

            // Tính tổng earnings cho tất cả Books
            var totalEarnings = bookEarnings.Sum(be => be.EarningAmount);
            string imagepayout = "";
            if (ImageStream != null && !string.IsNullOrEmpty(ImageContentType))
            {
                // Tạo thư mục động cho ảnh bìa dựa theo BookId
                string publisherpayoutFolderName = $"publisher_payout/";
                string coverImageFileName = $"{publisherpayoutFolderName}publisherpayout_{Guid.NewGuid()}.jpg";

                var coverImageUrl = await _googleCloudService.UploadImageAsync(coverImageFileName, ImageStream, ImageContentType);
                imagepayout = coverImageUrl;
            }
            // Tạo PublisherPayout
            var payout = new PublisherPayout
            {
                ImageURL = imagepayout,
                Description = description,
                Amount = totalEarnings,
                Status = PayoutStatus.Done,
                FromDate = fromDate,
                ToDate = toDate,
                PublisherId = publisherId,
                BookEarnings = bookEarnings
            };

            // Lưu vào cơ sở dữ liệu
            await _unitOfWork.PublisherPayoutRepository.AddAsync(payout);
            await _unitOfWork.SaveChangesAsync();

            // Tạo kết quả trả về
            var result = new PublisherPayoutDto
            {
                PayoutId = payout.Id,
                TotalAmount = payout.Amount,
                FromDate = payout.FromDate,
                ToDate = payout.ToDate,
                BookEarnings = bookEarnings.Select(be => new BookEarningDto
                {
                    BookId = be.BookId,
                    BookTitle = books.FirstOrDefault(b => b.Id == be.BookId)?.Title,
                    TotalEarnings = be.EarningAmount
                }).ToList(),
                CreateAt = payout.CreatedAt
            };

            return new ApiResponse<PublisherPayoutDto>
            {
                Succeeded = true,
                Message = "Payout created successfully.",
                Data = result
            };
        }


        public async Task<ApiResponse<PublisherPayout>> GetPayoutById(Guid id)
        {
            // Gọi phương thức repository để lấy dữ liệu chi tiết
            var payout = await _unitOfWork.PublisherPayoutRepository.GetPayoutWithDetailsByIdAsync(id);

            if (payout == null)
            {
                return new ApiResponse<PublisherPayout>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy quyết toán.",
                    Errors = new[] { "Invalid PayoutId." }
                };
            }

            // Map dữ liệu sang DTO
            var bookDetails = payout.BookEarnings.Select(be => new BookEarningsDto
            {
                BookId = be.BookId,
                BookTitle = be.Book?.Title ?? "Unknown Book",
                BookEarnings = be.EarningAmount
            }).ToList();

            //var publisherEarningsDto = new PublisherEarningsDto
            //{
            //    PublisherName = payout.Publisher.PublisherName,
            //    PublisherId = payout.Id,
            //    ContactInfo = payout.Publisher.ContactInfo,
            //    BankAccount = payout.Publisher.BankAccount,
            //    TotalEarnings = payout.Amount,
            //    FromDate = payout.FromDate,
            //    ToDate = payout.ToDate,
            //    BookDetails = bookDetails
            //};

            return new ApiResponse<PublisherPayout>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu quyết toán thành công.",
                Data = payout
            };
        }

        public async Task<ApiResponse<List<PublisherHistoryDto>>> GetPayoutByPublisherId(Guid publisherId)
        {
            var payout = await _unitOfWork.PublisherPayoutRepository.GetListPayoutByPublisherId(publisherId);

            var publisherdtos = payout.OrderByDescending(a => a.CreatedAt).Select(a => new PublisherHistoryDto
            {
                payoutId = a.Id,
                publisherId = a.PublisherId,
                PublisherName = a.Publisher.PublisherName,
                Description = a.Description,
                FromDate = a.FromDate,
                ToDate = a.ToDate,
                TotalEarnings = a.Amount,
                Status = a.Status.ToString(),
                CreateAt = a.CreatedAt
            }).ToList();
            return new ApiResponse<List<PublisherHistoryDto>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu quyết toán thành công.",
                Data = publisherdtos
            };
        }
    }
}
