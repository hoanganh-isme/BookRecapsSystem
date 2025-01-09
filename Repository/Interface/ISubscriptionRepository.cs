using BusinessObject.Models;
using BusinessObject.ViewModels.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ISubscriptionRepository : IBaseRepository<Subscription>
    {
        Task<Subscription?> GetByUserIdAsync(Guid userId);
        Task<List<Subscription>> GetValidSubscriptionsByUserIdAsync(Guid userId);
        Task<Subscription?> GetRemainViewByUserIdAsync(Guid userId);
        Task<List<Subscription>> GetHistorySubscriptionsByUserIdAsync(Guid userId);
        Task<Subscription?> GetNextSubscriptionByUserIdAsync(Guid userId);
        Task<decimal> GetRevenueFromPackages(DateTime fromDate, DateTime toDate);
        Task<List<PackageSalesDto>> GetPackageSales(DateTime fromDate, DateTime toDate);
        Task<List<PackageSalesDto>> GetPackageSalesFromSubscriptions(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalRevenue();
    }
}
