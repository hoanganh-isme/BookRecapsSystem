using BusinessObject.Data;
using BusinessObject.Models;
using BusinessObject.ViewModels.Dashboards;
using BusinessObject.ViewModels.Viewtrackings;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class ViewTrackingRepository : BaseRepository<ViewTracking>, IViewTrackingRepository
    {
        public ViewTrackingRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<ViewTracking?> GetByUserIdRecapIdAndSubscriptionIdAsync(Guid userId, Guid recapId, Guid subscriptionId)
        {
            return await context.ViewTrackings
                .FirstOrDefaultAsync(v => v.UserId == userId && v.RecapId == recapId && v.SubscriptionId == subscriptionId);
        }
        public async Task<List<ViewTracking>> GetViewTrackingsByRecapIdsAsync(List<Guid> recapIds)
        {
            return await context.ViewTrackings
                .Where(vt => recapIds.Contains(vt.RecapId))
                .ToListAsync();
        }

        // Lấy ViewTrackings theo danh sách RecapId và trong khoảng thời gian
        public async Task<List<ViewTracking>> GetViewTrackingsByRecapIdsAndDateRangeAsync(List<Guid> recapIds, DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => recapIds.Contains(vt.RecapId) &&
                             vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .ToListAsync();
        }
        public async Task<List<ViewTracking>> GetPagedViewTrackingsWithDetailsAsync(Guid userId, int skip, int take)
        {
            return await context.ViewTrackings
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(c => c.Recap.Contributor)
                .Include(v => v.Recap)
                    .ThenInclude(r => r.Book)
                    .ThenInclude(b => b.Authors)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await context.ViewTrackings
                .Where(v => v.UserId == userId)
                .CountAsync();
        }
        public async Task<List<Guid>> GetRecapIdsByUserIdAsync(Guid userId)
        {
            var recapIds = await context.ViewTrackings
                .Where(vt => vt.UserId == userId)
                .Select(vt => vt.RecapId)
                .Distinct()
                .ToListAsync();

            return recapIds;
        }


        public async Task<List<ViewTracking>> GetByRecapIdAndDateRangeAsync(Guid recapId, DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => vt.RecapId == recapId && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .ToListAsync();
        }
        public async Task<List<ViewTracking>> GetByListRecapIdsAndDateRangeAsync(List<Guid> recapIds, DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => recapIds.Contains(vt.RecapId)
                             && vt.CreatedAt >= fromDate
                             && vt.CreatedAt <= toDate)
                .ToListAsync();
        }
        public async Task<ViewTrackingSummaryDto> GetViewTrackingSummary(DateTime fromDate, DateTime toDate)
        {
            var result = await context.ViewTrackings
                .Where(v => v.CreatedAt >= fromDate && v.CreatedAt <= toDate)
                .GroupBy(v => true) // Group all rows together to calculate aggregates in one query
                .Select(g => new ViewTrackingSummaryDto
                {
                    TotalViews = g.Count(),
                    RevenueFromViews = g.Sum(v => v.ViewValue ?? 0),
                    PlatformProfit = g.Sum(v => v.PlatformValueShare ?? 0),
                    PublisherExpense = g.Sum(v => v.PublisherValueShare ?? 0),
                    ContributorExpense = g.Sum(v => v.ContributorValueShare ?? 0),
                })
                .FirstOrDefaultAsync();

            return result ?? new ViewTrackingSummaryDto(); // Return default values if no data
        }

        public async Task<int> CountViewTrackingsBetweenDatesAsync(List<Guid> bookIds, DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => bookIds.Contains(vt.Recap.BookId) && vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .CountAsync();
        }
        public async Task<List<ViewTracking>> GetViewTrackingsByPublisherIdAndDateRangeAsync(Guid publisherId, DateTime fromDate, DateTime toDate)
        {
            // Truy vấn từ Publisher -> Book -> Recap -> ViewTracking
            var viewTrackings = await context.Books
                .Where(b => b.PublisherId == publisherId) // Lọc theo PublisherId
                .SelectMany(b => b.Recaps) // Lấy tất cả các Recap của Book
                .SelectMany(r => r.ViewTrackings) // Lấy tất cả các ViewTracking của Recap
                .Where(vt => vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate) // Lọc theo khoảng thời gian
                .ToListAsync();

            return viewTrackings;
        }
        public async Task<List<ViewTracking>> GetViewTrackingsByBookIdAndDateRangeAsync(Guid bookId, DateTime fromDate, DateTime toDate)
        {
            // Truy vấn từ Publisher -> Book -> Recap -> ViewTracking
            var viewTrackings = await context.Books
                .Where(b => b.Id == bookId) // Lọc theo PublisherId
                .SelectMany(b => b.Recaps) // Lấy tất cả các Recap của Book
                .SelectMany(r => r.ViewTrackings) // Lấy tất cả các ViewTracking của Recap
                .Where(vt => vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate) // Lọc theo khoảng thời gian
                .ToListAsync();

            return viewTrackings;
        }
        public async Task<decimal> GetAllTimeEarningsFromBooks(Guid publisherId)
        {
            // Truy vấn từ Publisher -> Book -> Recap -> ViewTracking
            var totalEarningsFromBooks = await context.Books
                .Where(b => b.PublisherId == publisherId)
                .SelectMany(b => b.Recaps)
                .SelectMany(r => r.ViewTrackings) // Truyền qua ViewTrackings
                .Where(vt => vt.PublisherValueShare.HasValue) // Lọc nếu có PublisherValueShare
                .SumAsync(vt => vt.PublisherValueShare.Value); // Tính tổng thu nhập từ ViewTracking

            return totalEarningsFromBooks;
        }
        public async Task<List<ViewTracking>> GetViewTrackingsByContributorIdAndDateRangeAsync(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            return await context.Recaps
                .Where(vt => vt.UserId == contributorId).SelectMany(b => b.ViewTrackings)
                .Where(vt => vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .ToListAsync();
        }
        public async Task<int> CountRecapsCreatedBetweenDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            return await context.Recaps
                .Where(r => r.UserId == contributorId && r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .CountAsync();
        }
        public async Task<int> CountViewTrackingsBetweenByContributorIdDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => vt.Recap != null
                          && vt.Recap.UserId == contributorId
                          && vt.CreatedAt >= fromDate
                          && vt.CreatedAt <= toDate)
                .CountAsync();
        }

        public async Task<decimal> GetTotalEarningsByContributorIdAsync(Guid contributorId)
        {
            return await context.ViewTrackings
                .Where(vt => vt.Recap.UserId == contributorId && vt.ContributorValueShare.HasValue)
                .SumAsync(vt => vt.ContributorValueShare.Value);
        }
        // ViewTrackingRepository.cs
        public async Task<List<ViewTracking>> GetViewTrackingsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await context.ViewTrackings
                .Where(vt => vt.CreatedAt >= fromDate && vt.CreatedAt <= toDate)
                .ToListAsync();
        }


    }
}
