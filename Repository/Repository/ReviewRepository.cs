using BusinessObject.Data;
using BusinessObject.Enums;
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
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<Review>> GetReviewsByStaffAsync(Guid staffId)
        {
            return await context.Reviews
                .Where(a => a.StaffId == staffId)
                .Include(a => a.Staff)
                .Include(a => a.ReviewNotes)
                .Include(a => a.RecapVersion)
                .ToListAsync();
        }
        public async Task<List<Review>> GetReviewsByRecapVersionIds(List<Guid> recapVersionIds)
        {
            return await context.Reviews
                .Include(r => r.Staff)
                .Where(r => recapVersionIds.Contains(r.RecapVersionId))
                .ToListAsync();
        }


    }
}
