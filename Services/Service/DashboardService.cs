using BusinessObject.Models;
using BusinessObject.ViewModels.Dashboards;
using BusinessObject.ViewModels.PublisherPayouts;
using Microsoft.AspNetCore.Identity;
using Repository;
using Repository.Interface;
using Repository.Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.Dashboards.AdminChartDTO;

namespace Services.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Get data from repositories
                var revenueFromPackages = await _unitOfWork.SubscriptionRepository.GetRevenueFromPackages(fromDate, toDate);
                var packageSales = await _unitOfWork.SubscriptionRepository.GetPackageSales(fromDate, toDate);
                var newRecaps = await _unitOfWork.RecapRepository.GetNewRecaps(fromDate, toDate);
                var viewTrackingSummary = await _unitOfWork.ViewTrackingRepository.GetViewTrackingSummary(fromDate, toDate);
                var totalRevenue = await _unitOfWork.SubscriptionRepository.GetTotalRevenue();
                var totalExpenses = await _unitOfWork.ContributorPayoutRepository.GetTotalExpenses();

                // Create the AdminDashboardDto object
                var dashboardData = new AdminDashboardDto
                {
                    RevenueFromPackages = revenueFromPackages,
                    PackageSales = packageSales,
                    NewRecaps = newRecaps,
                    TotalViews = viewTrackingSummary.TotalViews,
                    RevenueFromViews = viewTrackingSummary.RevenueFromViews,
                    PlatformProfit = viewTrackingSummary.PlatformProfit,
                    PublisherExpense = viewTrackingSummary.PublisherExpense,
                    ContributorExpense = viewTrackingSummary.ContributorExpense,
                    TotalRevenue = totalRevenue,
                    TotalExpenses = totalExpenses,
                    CurrentBalance = totalRevenue - totalExpenses
                };

                return new ApiResponse<AdminDashboardDto>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thống kê cho Admin thành công.",
                    Data = dashboardData
                };
            }
            catch (Exception ex)
            {
                // Handle any errors and return an ApiResponse with failure status
                return new ApiResponse<AdminDashboardDto>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving the dashboard data.",
                    Errors = new[] { ex.Message },

                };
            }
        }
        public async Task<ApiResponse<AdminChartDto>> GetAdminChartAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var packages = await _unitOfWork.SubscriptionRepository.GetPackageSalesFromSubscriptions(fromDate, toDate);
                // Lấy tất cả ViewTracking trong khoảng thời gian từ Repository
                var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetViewTrackingsByDateRangeAsync(fromDate, toDate);
                var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                                     .Select(offset => fromDate.AddDays(offset).Date)
                                     .ToList();
                // Nhóm và tính toán doanh thu, lợi nhuận và số lượt xem theo ngày
                var dailyViewStats = allDates
                .GroupJoin(
                    viewTrackings,
                    date => date,
                    vt => vt.CreatedAt.Date,
                    (date, matchingViewTrackings) => new DailyViewChartDTO
                    {
                        Date = date,
                        RevenueEarning = matchingViewTrackings.Sum(vt => vt.ViewValue ?? 0), // Tổng doanh thu từ view
                        ProfitEarning = matchingViewTrackings.Sum(vt => vt.PlatformValueShare ?? 0), // Tổng lợi nhuận
                        ViewCount = matchingViewTrackings.Count() // Số lượt xem
                    })
                    .OrderBy(stat => stat.Date)
                    .ToList();
                var dailyPackageStats = allDates
                .Select(date => new DailyPackageChartDTO
                {
                    Date = date,
                    PackageName = string.Join(", ", packages
                        .Where(p => p.CreatedAt.Date == date)
                        .Select(p => p.PackageName)
                        .Distinct()),
                    Count = packages
                        .Where(p => p.CreatedAt.Date == date)
                        .Sum(p => p.Count),
                    Earning = packages
                        .Where(p => p.CreatedAt.Date == date)
                        .Sum(p => p.Price) // Cần đảm bảo logic tính `Earning` là chính xác
                })
                .OrderBy(stat => stat.Date)
                .ToList();
                // Đóng gói dữ liệu trả về
                var dashboardData = new AdminChartDto
                {
                    DailyPackageStats = dailyPackageStats,
                    DailyViewStats = dailyViewStats
                };

                return new ApiResponse<AdminChartDto>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thống kê thành công.",
                    Data = dashboardData
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return new ApiResponse<AdminChartDto>
                {
                    Succeeded = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Data = null
                };
            }
        }




        public async Task<List<PackageSalesDto>> GetPackageSalesAsync(DateTime fromDate, DateTime toDate)
        {
            return await _unitOfWork.SubscriptionRepository.GetPackageSales(fromDate, toDate);
        }
        public async Task<ApiResponse<PublisherDashboardDto>> GetPublisherDashboardAsync(Guid publisherId)
        {
            // Lấy quyết toán gần nhất của publisher
            var lastPayout = await _unitOfWork.PublisherPayoutRepository
                .GetLastPayoutByPublisherIdAsync(publisherId);

            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;
            var toDate = DateTime.UtcNow;

            // 1. Thu nhập từ ViewTracking mới (từ quyết toán cũ đến hiện tại)
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByPublisherIdAndDateRangeAsync(publisherId, fromDate, toDate);

            var incomeFromViewTracking = viewTrackings
                .Where(vt => vt.PublisherValueShare.HasValue)
                .Sum(vt => vt.PublisherValueShare ?? 0);

            // 2. Số tiền của lần quyết toán gần nhất
            var lastPayoutAmount = lastPayout?.Amount ?? 0;

            // 3. Số recap mới
            var books = await _unitOfWork.BookRepository.GetBooksByPublisherIdAsync(publisherId);
            var bookIds = books.Select(b => b.Id).ToList();

            var newRecaps = await _unitOfWork.RecapRepository
                .CountRecapsCreatedBetweenDatesAsync(bookIds, fromDate, toDate);

            // 4. Số recap cũ (tính trong khoảng thời gian của quyết toán gần nhất)
            var oldRecaps = await _unitOfWork.RecapRepository
                .CountRecapsCreatedBetweenDatesAsync(bookIds, lastPayout?.FromDate ?? DateTime.MinValue, lastPayout?.ToDate ?? DateTime.MinValue);

            // 5. Số lượt xem mới
            var newViewTrackings = await _unitOfWork.ViewTrackingRepository
                .CountViewTrackingsBetweenDatesAsync(bookIds, fromDate, toDate);

            // 6. Số lượt xem cũ (tính trong khoảng thời gian của quyết toán gần nhất)
            var oldViewTrackings = await _unitOfWork.ViewTrackingRepository
                .CountViewTrackingsBetweenDatesAsync(bookIds, lastPayout?.FromDate ?? DateTime.MinValue, lastPayout?.ToDate ?? DateTime.MinValue);

            // 7. Thu nhập tổng từ các book (All-time earnings)
            decimal totalEarningsFromBooks = 0;

            var bookDashboardDtos = new List<BookDashboardDto>();

            // Lặp qua các Book để tính tổng thu nhập từ tất cả các Recap của Book và lấy thông tin chi tiết sách
            foreach (var bookId in bookIds)
            {
                var recaps = await _unitOfWork.RecapRepository.GetRecapsByBookIdAsync(bookId);
                decimal bookTotalEarnings = 0;
                decimal paidEarning = 0;
                decimal unpaidEarning = 0;

                foreach (var recap in recaps)
                {
                    // Tính thu nhập từ ViewTracking có PublisherValueShare cho mỗi Recap
                    var viewTrackingsForRecap = await _unitOfWork.ViewTrackingRepository
                        .GetViewTrackingsByRecapIdsAsync(new List<Guid> { recap.Id });

                    // Tính tổng thu nhập từ ViewTracking chưa thanh toán và đã thanh toán
                    bookTotalEarnings += viewTrackingsForRecap
                        .Where(vt => vt.PublisherValueShare.HasValue)
                        .Sum(vt => vt.PublisherValueShare ?? 0);

                    // Tính thu nhập từ các ViewTracking đã thanh toán từ lần quyết toán gần nhất
                    var paidViewTrackings = viewTrackingsForRecap
                        .Where(vt => vt.PublisherValueShare.HasValue  && vt.CreatedAt >= lastPayout?.FromDate && vt.CreatedAt <= lastPayout?.ToDate);

                    paidEarning += paidViewTrackings
                        .Sum(vt => vt.PublisherValueShare ?? 0);

                    unpaidEarning = bookTotalEarnings - paidEarning;
                }

                var book = books.FirstOrDefault(b => b.Id == bookId); // Lấy thông tin sách từ danh sách books

                var bookDashboardDto = new BookDashboardDto
                {
                    BookId = book?.Id ?? Guid.Empty,
                    Title = book?.Title ?? "Unknown Title",
                    PublicationYear = book?.PublicationYear ?? 0,
                    CoverImage = book?.CoverImage ?? string.Empty,
                    RecapCount = recaps.Count(),
                    ISBN_10 = book.ISBN_10 ?? "null",
                    ISBN_13 = book.ISBN_13 ?? "null",
                    PaidEarnings = paidEarning,
                    UnPaidEarnings = unpaidEarning

                };

                // Thêm BookDashboardDto vào danh sách
                bookDashboardDtos.Add(bookDashboardDto);

                totalEarningsFromBooks += bookTotalEarnings;
            }

            // Tạo đối tượng PublisherDashboardDto
            var dashboardData = new PublisherDashboardDto
            {
                TotalIncomeFromViewTracking = incomeFromViewTracking,
                LastPayoutAmount = lastPayoutAmount,
                NewRecapsCount = newRecaps,
                OldRecapsCount = oldRecaps,
                NewViewCount = newViewTrackings,
                OldViewCount = oldViewTrackings,
                Books = bookDashboardDtos
            };

            return new ApiResponse<PublisherDashboardDto>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = dashboardData
            };
        }
        public async Task<ApiResponse<ChartDTO>> GetPublisherChartDashboardAsync(Guid publisherId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<ChartDTO>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }
            var books = await _unitOfWork.BookRepository.GetBooksByPublisherIdAsync(publisherId);
            var bookIds = books.Select(b => b.Id).ToList();
            decimal totalEarningsFromBooks = 0;
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByPublisherIdAndDateRangeAsync(publisherId, fromDate, toDate);
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
            // Tạo đối tượng PublisherDashboardDto
            var dashboardData = new ChartDTO
            {
                TotalViews = dailyStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyStats.Sum(stat => stat.WatchTime),
                TotalEarnings = dailyStats.Sum(stat => stat.Earning),
                DailyStats = dailyStats,
            };

            return new ApiResponse<ChartDTO>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = dashboardData
            };
        }



        public async Task<ApiResponse<ContributorDashboardDto>> GetContributorDashboardAsync(Guid contributorId)
        {
            // Lấy quyết toán gần nhất của contributor
            var lastPayout = await _unitOfWork.ContributorPayoutRepository
                .GetLastPayoutByContributorIdAsync(contributorId);

            var user = await _userManager.FindByIdAsync(contributorId.ToString());
            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;
            var toDate = DateTime.UtcNow;

            // 1. Thu nhập từ ViewTracking mới (từ quyết toán cũ đến hiện tại)
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByContributorIdAndDateRangeAsync(contributorId, fromDate, toDate);

            var incomeFromViewTracking = viewTrackings
                .Where(vt => vt.ContributorValueShare.HasValue)
                .Sum(vt => vt.ContributorValueShare ?? 0);

            // 2. Số tiền của lần quyết toán gần nhất
            var lastPayoutAmount = lastPayout?.Amount ?? 0;

            // 3. Số recap mới
            var newRecaps = await _unitOfWork.RecapRepository
                .CountRecapsCreatedBetweenDatesAsync(contributorId, fromDate, toDate);
            var oldRecaps = await _unitOfWork.RecapRepository
                .CountRecapsCreatedBetweenDatesAsync(contributorId, lastPayout?.FromDate ?? DateTime.MinValue, lastPayout?.ToDate ?? DateTime.MinValue);

            // 4. Số lượt xem mới
            var newViewTrackings = await _unitOfWork.ViewTrackingRepository
                .CountViewTrackingsBetweenByContributorIdDatesAsync(contributorId, fromDate, toDate);
            var oldViewTrackings = await _unitOfWork.ViewTrackingRepository
               .CountViewTrackingsBetweenByContributorIdDatesAsync(contributorId, lastPayout?.FromDate ?? DateTime.MinValue, lastPayout?.ToDate ?? DateTime.MinValue);

            // 5. Thu nhập tổng từ tất cả các recap (All-time earnings)
            var allTimeEarnings = (await _unitOfWork.ViewTrackingRepository
    .GetTotalEarningsByContributorIdAsync(contributorId)) != default ?
    await _unitOfWork.ViewTrackingRepository.GetTotalEarningsByContributorIdAsync(contributorId) : 0m;



            // 6. Recaps có lượt xem cao nhất (sắp xếp theo lượt xem)
            var mostViewedRecaps = await _unitOfWork.RecapRepository
                .GetMostViewedRecapsByContributorIdAsync(contributorId, 5) ?? new List<Recap>();

            // 7. Lấy tất cả recap của Contributor
            var allRecapsPublished = await _unitOfWork.RecapRepository
                .GetAllRecapsByContributorIdAsync(contributorId) ?? new List<Recap>();

            var currentEarning = user?.Earning ?? 0;

            // Tạo đối tượng ContributorDashboardDto
            var dashboardData = new ContributorDashboardDto
            {
                TotalIncome = incomeFromViewTracking,
                LastPayoutAmount = lastPayoutAmount,
                NewRecapsCount = newRecaps,
                OldRecapsCount = oldRecaps,
                NewViewCount = newViewTrackings,
                OldViewCount = oldViewTrackings,
                TotalEarnings = allTimeEarnings,
                CurrentEarnings = currentEarning,
                Recaps = allRecapsPublished.Select(r => new RecapDashboardDto
                {
                    RecapId = r.Id,
                    RecapName = r.Name ?? string.Empty,
                    BookImage = r.Book.CoverImage ?? string.Empty,
                    BookName = r.Book.Title ?? string.Empty,
                    isPublished = r.isPublished,
                    ViewsCount = r.ViewsCount ?? 0,
                    LikesCount = r.LikesCount ?? 0
                }).ToList(),
                MostViewedRecaps = mostViewedRecaps.Select(r => new MostRecapDashboardDto
                {
                    RecapId = r.Id,
                    RecapName = r.Name ?? string.Empty,
                    BookImage = r.Book.CoverImage ?? string.Empty,
                    BookName = r.Book.Title ?? string.Empty,
                    isPublished = r.isPublished,
                    ViewsCount = r.ViewsCount ?? 0,
                    LikesCount = r.LikesCount ?? 0
                }).ToList()
            };

            return new ApiResponse<ContributorDashboardDto>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = dashboardData
            };
        }
        public async Task<ApiResponse<ChartDTO>> GetContributorChartDashboardAsync(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<ChartDTO>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }
            var allRecapsPublished = await _unitOfWork.RecapRepository
                .GetAllRecapsByContributorIdAsync(contributorId) ?? new List<Recap>();
            var recapIds = allRecapsPublished.Select(b => b.Id).ToList();
            decimal totalEarningsFromBooks = 0;
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByContributorIdAndDateRangeAsync(contributorId, fromDate, toDate);
            var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset).Date)
                .ToList();
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
                        Earning = matchingViewTrackings.Sum(vt => vt.ContributorValueShare ?? 0)
                    }
                )
                .OrderBy(stat => stat.Date)
                .ToList();
            var dashboardData = new ChartDTO
            {
                TotalViews = dailyStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyStats.Sum(stat => stat.WatchTime),
                TotalEarnings = dailyStats.Sum(stat => stat.Earning),
                DailyStats = dailyStats,
            };

            return new ApiResponse<ChartDTO>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = dashboardData
            };
        }

    }
}
