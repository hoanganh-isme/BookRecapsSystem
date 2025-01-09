using BusinessObject.Models;
using BusinessObject.ViewModels.Dashboards;
using BusinessObject.ViewModels.Viewtrackings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IViewTrackingRepository : IBaseRepository<ViewTracking>
    {
        Task<ViewTracking?> GetByUserIdRecapIdAndSubscriptionIdAsync(Guid userId, Guid recapId, Guid subscriptionId);
        Task<List<ViewTracking>> GetViewTrackingsByRecapIdsAndDateRangeAsync(List<Guid> recapIds, DateTime fromDate, DateTime toDate);
        Task<List<ViewTracking>> GetViewTrackingsByRecapIdsAsync(List<Guid> recapIds);
        Task<List<ViewTracking>> GetPagedViewTrackingsWithDetailsAsync(Guid userId, int skip, int take);
        Task<int> GetCountByUserIdAsync(Guid userId);
        Task<List<Guid>> GetRecapIdsByUserIdAsync(Guid userId);
        Task<List<ViewTracking>> GetByRecapIdAndDateRangeAsync(Guid recapId, DateTime fromDate, DateTime toDate);
        Task<List<ViewTracking>> GetByListRecapIdsAndDateRangeAsync(List<Guid> recapIds, DateTime fromDate, DateTime toDate);
        Task<ViewTrackingSummaryDto> GetViewTrackingSummary(DateTime fromDate, DateTime toDate);
        Task<int> CountViewTrackingsBetweenDatesAsync(List<Guid> bookIds, DateTime fromDate, DateTime toDate);
        Task<List<ViewTracking>> GetViewTrackingsByPublisherIdAndDateRangeAsync(Guid publisherId, DateTime fromDate, DateTime toDate);
        Task<List<ViewTracking>> GetViewTrackingsByBookIdAndDateRangeAsync(Guid bookId, DateTime fromDate, DateTime toDate);
        Task<decimal> GetAllTimeEarningsFromBooks(Guid publisherId);
        Task<List<ViewTracking>> GetViewTrackingsByContributorIdAndDateRangeAsync(Guid contributorId, DateTime fromDate, DateTime toDate);
        Task<int> CountRecapsCreatedBetweenDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate);
        Task<int> CountViewTrackingsBetweenByContributorIdDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalEarningsByContributorIdAsync(Guid contributorId);
        Task<List<ViewTracking>> GetViewTrackingsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
