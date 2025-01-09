using BusinessObject.Data;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class ContributorWithdrawalRepository : BaseRepository<ContributorWithdrawal>, IContributorWithdrawalRepository
    {
        public ContributorWithdrawalRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<ContributorWithdrawal>> GetLatestDrawalForAllContributorsAsync()
        {
            return await context.ContributorWithdrawals
                .AsNoTracking()
                .GroupBy(p => p.ContributorId) // Group payouts by UserId
                .Select(g => g.OrderByDescending(p => p.CreatedAt).FirstOrDefault()) // Lấy payout mới nhất
                .ToListAsync();
        }
        public async Task<List<ContributorWithdrawal>> GetListDrawalByContributorId(Guid contributorId)
        {
            return await context.ContributorWithdrawals
                .Where(p => p.ContributorId == contributorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<ContributorWithdrawal> GetDrawalByContributorIdAsync(Guid contributorId)
        {
            return await context.ContributorWithdrawals
                .Include(p => p.Contributor)
                .Where(p => p.ContributorId == contributorId)
                .OrderByDescending(p => p.CreatedAt)// Sắp xếp theo thời gian tạo mới nhất trước
                .FirstOrDefaultAsync();
        }

    }
}
