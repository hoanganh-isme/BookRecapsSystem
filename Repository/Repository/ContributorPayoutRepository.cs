using BusinessObject.Data;
using BusinessObject.Enums;
using BusinessObject.Models;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;

namespace Repository.Repository
{
    public class ContributorPayoutRepository : BaseRepository<ContributorPayout>, IContributorPayoutRepository
    {
        public ContributorPayoutRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<ContributorPayout> GetLastPayoutByContributorIdAsync(Guid contributorId)
        {
            return await context.ContributorPayouts
                .Where(p => p.UserId == contributorId && p.Status == PayoutStatus.Done) // Lọc theo trạng thái Done
                .OrderByDescending(p => p.ToDate) // Sắp xếp theo ToDate giảm dần
                .FirstOrDefaultAsync();
        }
        public async Task<List<ContributorPayout>> GetLatestPayoutsForAllContributorsAsync()
        {
            return await context.ContributorPayouts
                .AsNoTracking()
                .GroupBy(p => p.UserId) // Group payouts by UserId
                .Select(g => g.OrderByDescending(p => p.ToDate).FirstOrDefault()) // Lấy payout mới nhất
                .ToListAsync();
        }
        public async Task<List<ContributorPayout>> GetListPayoutByContributorId(Guid contributorId)
        {
            return await context.ContributorPayouts
                .Where(p => p.UserId == contributorId)
                .Include(p => p.Contributor)
                .OrderByDescending(p => p.ToDate)
                .ToListAsync();
        }
        public async Task<ContributorPayout> GetPayoutWithDetailsByIdAsync(Guid payoutId)
        {
            return await context.ContributorPayouts
                .Include(p => p.Contributor) // Bao gồm Contributor
                .Include(p => p.RecapEarnings) // Bao gồm RecapEarnings
                .ThenInclude(re => re.Recap) // Bao gồm Recap qua RecapEarnings
                .AsNoTracking() // Tăng hiệu năng nếu không cần thay đổi dữ liệu
                .FirstOrDefaultAsync(p => p.Id == payoutId);
        }
        public async Task<decimal> GetTotalExpenses()
        {
            var contributorExpenses = await context.ContributorPayouts.SumAsync(c => c.Amount);
            var publisherExpenses = await context.PublisherPayouts.SumAsync(p => p.Amount);

            return contributorExpenses + publisherExpenses;
        }

    }
}
