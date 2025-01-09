using AutoMapper;
using BusinessObject.ViewModels.ContributorPayouts;
using BusinessObject.ViewModels.Dashboards;
using BusinessObject.ViewModels.PublisherPayouts;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.RecapDetailsDTO;
using static BusinessObject.ViewModels.PublisherPayouts.PublisherPayoutDTO;

namespace Services.Service
{
    public class BookEarningService : IBookEarningService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BookEarningService(IUnitOfWork unitOfWork,
                                  IMapper mapper) 
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PublisherEarningsDto>> GetPublisherEarningsAsync(Guid publisherId, DateTime toDate)
        {
            if (toDate > DateTime.UtcNow)
            {
                return new ApiResponse<PublisherEarningsDto>
                {
                    Succeeded = false,
                    Message = "Ngày quyết toán không lớn hơn ngày hiện tại.",
                    Data = null
                };
            }
            var books = await _unitOfWork.BookRepository.GetBooksByPublisherIdAsync(publisherId);

            if (books == null || !books.Any())
            {
                return new ApiResponse<PublisherEarningsDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy sách của nào nhà xuất bản này.",
                    Data = null
                };
            }

            // Lấy thông tin Publisher từ Book đầu tiên
            var publisher = books.First().Publisher;

            // Lấy thông tin Payout cuối cùng của Publisher để xác định FromDate
            var lastPayout = await _unitOfWork.PublisherPayoutRepository.GetLastPayoutByPublisherIdAsync(publisherId);
            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;

            // Tính tổng earnings từ ViewTracking trong khoảng thời gian từ FromDate đến ToDate
            var totalEarnings = books.Sum(b => b.Recaps
                .SelectMany(r => r.ViewTrackings)
                .Where(vt => vt.PublisherValueShare.HasValue && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .Sum(vt => vt.PublisherValueShare.Value));

            // Tạo DTO để trả về
            var result = new PublisherEarningsDto
            {
                PublisherName = publisher.PublisherName,
                PublisherId = publisher.Id,
                BankAccount = publisher.BankAccount,
                ContactInfo = publisher.ContactInfo,
                TotalEarnings = totalEarnings,
                FromDate = fromDate,
                ToDate = toDate,
                BookDetails = books.Select(b => new BookEarningsDto
                {
                    BookId = b.Id,
                    BookTitle = b.Title,
                    BookEarnings = b.Recaps
                        .SelectMany(r => r.ViewTrackings)
                        .Where(vt => vt.PublisherValueShare.HasValue && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                        .Sum(vt => vt.PublisherValueShare.Value)
                }).ToList()
            };

            return new ApiResponse<PublisherEarningsDto>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = result
            };
        }
        public async Task<ApiResponse<BookAdminDetailDto>> GetBookDetailForAdminAsync(Guid bookId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<BookAdminDetailDto>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }

            // Lấy thông tin Book và danh sách RecapId
            var book = await _unitOfWork.BookRepository.QueryWithIncludes(b => b.Recaps)
                .Include(b => b.Recaps)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                return new ApiResponse<BookAdminDetailDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Book.",
                    Data = null
                };
            }

            // Lấy danh sách RecapId liên quan đến Book
            var recapIds = book.Recaps.Select(r => r.Id).ToList();

            if (!recapIds.Any())
            {
                return new ApiResponse<BookAdminDetailDto>
                {
                    Succeeded = false,
                    Message = "Book không có bất kỳ Recap nào.",
                    Data = null
                };
            }

            // Lấy danh sách ViewTracking dựa trên RecapId và khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetByListRecapIdsAndDateRangeAsync(recapIds, fromDate, toDate);

            // Tạo danh sách tất cả các ngày trong khoảng thời gian
            var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset).Date)
                .ToList();

            // Tính toán thống kê hàng ngày của Book
            var dailyViewValueStats = allDates
                .GroupJoin(
                    viewTrackings,
                    date => date,
                    vt => vt.CreatedAt.Date,
                    (date, matchingViewTrackings) => new DailyViewValueStatDto
                    {
                        Date = date,
                        Views = matchingViewTrackings.Count(),
                        WatchTime = matchingViewTrackings.Sum(vt => vt.Durations ?? 0),
                        Earning = matchingViewTrackings.Sum(vt => vt.ViewValue ?? 0)
                    }
                )
                .OrderBy(stat => stat.Date)
                .ToList();
            var dailyPlatformStats = allDates
                .GroupJoin(
                    viewTrackings,
                    date => date,
                    vt => vt.CreatedAt.Date,
                    (date, matchingViewTrackings) => new DailyPlatformStatDto
                    {
                        Date = date,
                        Views = matchingViewTrackings.Count(),
                        WatchTime = matchingViewTrackings.Sum(vt => vt.Durations ?? 0),
                        Earning = matchingViewTrackings.Sum(vt => vt.PlatformValueShare ?? 0)
                    }
                )
                .OrderBy(stat => stat.Date)
                .ToList();

            // Lấy quyết toán gần nhất của Book
            var latestPayout = await _unitOfWork.BookEarningRepository.GetBookEarningByBookId(bookId);
            var unpaidfromDate = latestPayout?.ToDate ?? DateTime.MinValue;
            var unpaidviewtracking = await _unitOfWork.ViewTrackingRepository.GetByListRecapIdsAndDateRangeAsync(recapIds, unpaidfromDate, DateTime.Now);

            // Tổng hợp dữ liệu Book
            var bookDetail = new BookAdminDetailDto
            {
                BookId = book.Id,
                Title = book.Title,
                OriginalTitle = book.OriginalTitle,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                CoverImage = book.CoverImage,
                DailyViewValueStats = dailyViewValueStats,
                DailyPlatformStats = dailyPlatformStats,
                TotalViews = dailyViewValueStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyViewValueStats.Sum(stat => stat.WatchTime),
            };

            return new ApiResponse<BookAdminDetailDto>
            {
                Succeeded = true,
                Message = "Lấy chi tiết Book thành công.",
                Data = bookDetail
            };
        }
        public async Task<ApiResponse<BookDetailDto>> GetBookDetailAsync(Guid bookId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<BookDetailDto>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }

            // Lấy thông tin Book và danh sách RecapId
            var book = await _unitOfWork.BookRepository.QueryWithIncludes(b => b.Recaps)
                .Include(b => b.Recaps)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                return new ApiResponse<BookDetailDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Book.",
                    Data = null
                };
            }

            // Lấy danh sách RecapId liên quan đến Book
            var recapIds = book.Recaps.Select(r => r.Id).ToList();

            if (!recapIds.Any())
            {
                return new ApiResponse<BookDetailDto>
                {
                    Succeeded = false,
                    Message = "Book không có bất kỳ Recap nào.",
                    Data = null
                };
            }

            // Lấy danh sách ViewTracking dựa trên RecapId và khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetByListRecapIdsAndDateRangeAsync(recapIds, fromDate, toDate);

            // Tạo danh sách tất cả các ngày trong khoảng thời gian
            var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset).Date)
                .ToList();

            // Tính toán thống kê hàng ngày của Book
            var dailyStats = allDates
                .GroupJoin(
                    viewTrackings,
                    date => date,
                    vt => vt.CreatedAt.Date,
                    (date, matchingViewTrackings) => new DailyBookStatDto
                    {
                        Date = date,
                        Views = matchingViewTrackings.Count(),
                        WatchTime = matchingViewTrackings.Sum(vt => vt.Durations ?? 0),
                        Earning = matchingViewTrackings.Sum(vt => vt.PublisherValueShare ?? 0)
                    }
                )
                .OrderBy(stat => stat.Date)
                .ToList();

            // Lấy quyết toán gần nhất của Book
            var latestPayout = await _unitOfWork.BookEarningRepository.GetBookEarningByBookId(bookId);
            var unpaidfromDate = latestPayout?.ToDate ?? DateTime.MinValue;
            var unpaidviewtracking = await _unitOfWork.ViewTrackingRepository.GetByListRecapIdsAndDateRangeAsync(recapIds, unpaidfromDate, DateTime.Now);

            // Tổng hợp dữ liệu Book
            var bookDetail = new BookDetailDto
            {
                BookId = book.Id,
                Title = book.Title,
                OriginalTitle = book.OriginalTitle,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                CoverImage = book.CoverImage,
                DailyStats = dailyStats,
                UnpaidEarning = unpaidviewtracking.Sum(vt => vt.PublisherValueShare ?? 0),
                TotalViews = dailyStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyStats.Sum(stat => stat.WatchTime),
                TotalEarnings = dailyStats.Sum(stat => stat.Earning),
                LastPayout = latestPayout != null
                    ? new LatestBookPayoutDto
                    {
                        FromDate = latestPayout.FromDate,
                        ToDate = latestPayout.ToDate,
                        Amount = latestPayout.EarningAmount
                    }
                    : null
            };

            return new ApiResponse<BookDetailDto>
            {
                Succeeded = true,
                Message = "Lấy chi tiết Book thành công.",
                Data = bookDetail
            };
        }



    }
}
