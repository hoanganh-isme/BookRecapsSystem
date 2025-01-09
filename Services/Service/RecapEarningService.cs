using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.ContributorPayouts;
using BusinessObject.ViewModels.Dashboards;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;
using static BusinessObject.ViewModels.ContributorPayouts.RecapDetailsDTO;

namespace Services.Service
{
    public class RecapEarningService : IRecapEarningService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RecapEarningService(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<ContributorEarningsDto>> GetContributorEarningsAsync(Guid contributorId, DateTime toDate)
        {
            if (toDate > DateTime.UtcNow)
            {
                return new ApiResponse<ContributorEarningsDto>
                {
                    Succeeded = false,
                    Message = "Ngày quyết toán không lớn hơn ngày hiện tại.",
                    Data = null
                };
            }
            // Lấy danh sách các recap và user liên quan
            var recaps = await _unitOfWork.RecapRepository.GetRecapsByContributorIdAsync(contributorId);

            if (recaps == null || !recaps.Any())
            {
                return new ApiResponse<ContributorEarningsDto>
                {
                    Succeeded = false,
                    Message = "No recaps found for this contributor.",
                    Data = null
                };
            }
            // Lấy thông tin User từ Recap đầu tiên
            var user = recaps.First().Contributor;

            // Lấy ContributorPayout cuối cùng có trạng thái Done để lấy FromDate
            var lastPayout = await _unitOfWork.ContributorPayoutRepository
                .GetLastPayoutByContributorIdAsync(contributorId);

            // Nếu không tìm thấy payout, FromDate sẽ là DateTime.MinValue (hoặc một giá trị khác tùy bạn)
            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;

            // Tính tổng earnings trong khoảng thời gian từ FromDate đến ToDate
            var totalEarnings = recaps.Sum(r => r.ViewTrackings
                .Where(vt => vt.ContributorValueShare.HasValue && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .Sum(vt => vt.ContributorValueShare.Value));

            // Tạo DTO để trả về
            var result = new ContributorEarningsDto
            {
                ContributorName = user.FullName,
                ContributorId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                BankAccount = user.BankAccount,
                TotalEarnings = totalEarnings,
                Fromdate = fromDate,
                Todate = toDate,
                RecapDetails = recaps.Select(r => new RecapEarningsDto
                {
                    RecapId = r.Id,
                    RecapName = r.Name,
                    ViewCount = r.ViewsCount,
                    RecapEarnings = r.ViewTrackings
                        .Where(vt => vt.ContributorValueShare.HasValue && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                        .Sum(vt => vt.ContributorValueShare.Value)
                }).ToList()
            };

            return new ApiResponse<ContributorEarningsDto>
            {
                Succeeded = true,
                Message = "Successfully retrieved contributor earnings.",
                Data = result
            };
        }
        public async Task<ApiResponse<RecapAdminDetailDto>> GetRecapDetailForAdminAsync(Guid recapId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<RecapAdminDetailDto>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }

            // Lấy thông tin Recap
            var recap = await _unitOfWork.RecapRepository.QueryWithIncludes(x => x.CurrentVersion)
                .FirstOrDefaultAsync(x => x.Id == recapId);
            if (recap == null)
            {
                return new ApiResponse<RecapAdminDetailDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Recap.",
                    Data = null
                };
            }

            // Lấy danh sách ViewTracking theo recapId và khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetByRecapIdAndDateRangeAsync(recapId, fromDate, toDate);

            // Tạo danh sách tất cả các ngày trong khoảng thời gian
            var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset).Date)
                .ToList();

            // Kết hợp dữ liệu với danh sách các ngày
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
            // Lấy quyết toán gần nhất của Contributor
            var latestPayout = await _unitOfWork.RecapEarningRepository.GetRecapEarningByRecapId(recapId);

            // Nếu không có latestPayout, sử dụng DateTime.MinValue làm giá trị mặc định
            var unpaidfromDate = latestPayout?.ToDate ?? DateTime.MinValue;

            // Lấy danh sách unpaid view trackings từ fromDate đến hiện tại
            var unpaidViewTrackings = await _unitOfWork.ViewTrackingRepository.GetByRecapIdAndDateRangeAsync(recapId, unpaidfromDate, DateTime.Now);
            // Tổng hợp dữ liệu chi tiết Recap
            var recapDetail = new RecapAdminDetailDto
            {
                RecapId = recap.Id,
                RecapName = recap.Name,
                isPublished = recap.isPublished,
                isPremium = recap.isPremium,
                CurrentVersionId = recap.CurrentVersion?.Id, // Kiểm tra null
                CurrentVersionName = recap.CurrentVersion?.VersionName, // Kiểm tra null
                TotalViews = dailyViewValueStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyViewValueStats.Sum(stat => stat.WatchTime),
                DailyViewValueStats = dailyViewValueStats,
                DailyPlatformStats = dailyPlatformStats

            };

            return new ApiResponse<RecapAdminDetailDto>
            {
                Succeeded = true,
                Message = "Lấy chi tiết Recap thành công.",
                Data = recapDetail
            };
        }

        public async Task<ApiResponse<RecapDetailDto>> GetRecapDetailAsync(Guid recapId, DateTime fromDate, DateTime toDate)
        {
            // Kiểm tra ngày đầu vào
            if (fromDate > toDate)
            {
                return new ApiResponse<RecapDetailDto>
                {
                    Succeeded = false,
                    Message = "Ngày không hợp lệ.",
                    Data = null
                };
            }

            // Lấy thông tin Recap
            var recap = await _unitOfWork.RecapRepository.QueryWithIncludes(x => x.CurrentVersion)
                .FirstOrDefaultAsync(x => x.Id == recapId);
            if (recap == null)
            {
                return new ApiResponse<RecapDetailDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Recap.",
                    Data = null
                };
            }

            // Lấy danh sách ViewTracking theo recapId và khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetByRecapIdAndDateRangeAsync(recapId, fromDate, toDate);

            // Tạo danh sách tất cả các ngày trong khoảng thời gian
            var allDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset).Date)
                .ToList();

            // Kết hợp dữ liệu với danh sách các ngày
            var dailyStats = allDates
                .GroupJoin(
                    viewTrackings,
                    date => date,
                    vt => vt.CreatedAt.Date,
                    (date, matchingViewTrackings) => new DailyRecapStatDto
                    {
                        Date = date,
                        Views = matchingViewTrackings.Count(),
                        WatchTime = matchingViewTrackings.Sum(vt => vt.Durations ?? 0),
                        Earning = matchingViewTrackings.Sum(vt => vt.ContributorValueShare ?? 0)
                    }
                )
                .OrderBy(stat => stat.Date)
                .ToList();

            // Lấy quyết toán gần nhất của Contributor
            var latestPayout = await _unitOfWork.RecapEarningRepository.GetRecapEarningByRecapId(recapId);

            // Nếu không có latestPayout, sử dụng DateTime.MinValue làm giá trị mặc định
            var unpaidfromDate = latestPayout?.ToDate ?? DateTime.MinValue;

            // Lấy danh sách unpaid view trackings từ fromDate đến hiện tại
            var unpaidViewTrackings = await _unitOfWork.ViewTrackingRepository.GetByRecapIdAndDateRangeAsync(recapId, unpaidfromDate, DateTime.Now);
            // Tổng hợp dữ liệu chi tiết Recap
            var recapDetail = new RecapDetailDto
            {
                RecapId = recap.Id,
                RecapName = recap.Name,
                isPublished = recap.isPublished,
                isPremium = recap.isPremium,
                CurrentVersionId = recap.CurrentVersion?.Id, // Kiểm tra null
                CurrentVersionName = recap.CurrentVersion?.VersionName, // Kiểm tra null
                UnpaidEarning = unpaidViewTrackings.Sum(vt => vt.ContributorValueShare ?? 0),
                TotalViews = dailyStats.Sum(stat => stat.Views),
                TotalWatchTime = dailyStats.Sum(stat => stat.WatchTime),
                TotalEarnings = dailyStats.Sum(stat => stat.Earning),
                DailyStats = dailyStats,
                LastPayout = latestPayout != null
                    ? new LatestPayoutDto
                    {
                        FromDate = latestPayout.FromDate,
                        ToDate = latestPayout.ToDate,
                        Amount = latestPayout.EarningAmount
                    }
                    : null
            };

            return new ApiResponse<RecapDetailDto>
            {
                Succeeded = true,
                Message = "Lấy chi tiết Recap thành công.",
                Data = recapDetail
            };
        }    

    }
}
