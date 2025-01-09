using BusinessObject.Data;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Dashboards;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Subscription?> GetByUserIdAsync(Guid userId)
        {
            return await context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.Status == SubStatus.Active)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }
        public async Task<Subscription?> GetRemainViewByUserIdAsync(Guid userId)
        {
            return await context.Subscriptions
                .AsNoTracking()
                .Include(s => s.SubscriptionPackage)
                .Where(s => s.UserId == userId && s.Status == SubStatus.Active)
                .OrderBy(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Subscription>> GetValidSubscriptionsByUserIdAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return await context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId
                            && s.StartDate <= today
                            && s.EndDate > today
                            && s.ActualViewsCount < s.ExpectedViewsCount)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }
        public async Task<List<Subscription>> GetHistorySubscriptionsByUserIdAsync(Guid userId)
        {
            return await context.Subscriptions
                .AsNoTracking()
                .Include(s => s.SubscriptionPackage)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
        public async Task<Subscription?> GetNextSubscriptionByUserIdAsync(Guid userId)
        {
            return await context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == SubStatus.NotStarted)
                .OrderBy(s => s.StartDate) // Sắp xếp theo ngày bắt đầu hoặc thứ tự ưu tiên
                .FirstOrDefaultAsync();
        }
        public async Task<decimal> GetRevenueFromPackages(DateTime fromDate, DateTime toDate)
        {
            return await context.Subscriptions
                .Where(s => s.CreatedAt >= fromDate && s.CreatedAt <= toDate)
                .SumAsync(s => s.Price ?? 0);
        }

        public async Task<List<PackageSalesDto>> GetPackageSales(DateTime fromDate, DateTime toDate)
        {
            var packageSales = await context.SubscriptionPackages
                .GroupJoin(
                    context.Subscriptions.Where(s => s.CreatedAt >= fromDate && s.CreatedAt <= toDate),
                    package => package.Id,
                    subscription => subscription.SubscriptionPackageId,
                    (package, subscriptions) => new PackageSalesDto
                    {
                        PackageId = package.Id,
                        PackageName = package.Name,
                        CreatedAt = package.CreatedAt,
                        Count = subscriptions.Count()
                    }
                )
                .ToListAsync();

            return packageSales;
        }
        public async Task<List<PackageSalesDto>> GetPackageSalesFromSubscriptions(DateTime fromDate, DateTime toDate)
        {
            var packageSales = await context.SubscriptionPackages
                .SelectMany(
                    package => context.Subscriptions
                        .Where(subscription => subscription.SubscriptionPackageId == package.Id &&
                                               subscription.CreatedAt >= fromDate && subscription.CreatedAt <= toDate)
                        .Select(subscription => new PackageSalesDto
                        {
                            PackageId = package.Id,
                            PackageName = package.Name,
                            CreatedAt = subscription.CreatedAt, // Lấy CreatedAt từ Subscription
                            Price = subscription.Price,
                            Count = 1 // Mỗi subscription được tính là 1
                        })
                )
                .ToListAsync();

            return packageSales;
        }

        public async Task<decimal> GetTotalRevenue()
        {
            return await context.Subscriptions.SumAsync(s => s.Price ?? 0);
        }

    }
}
