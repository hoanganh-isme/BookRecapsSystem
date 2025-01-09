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
    public class AppealRepository : BaseRepository<Appeal>, IAppealRepository
    {
        public AppealRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<Appeal>> GetAppealsByStaffAsync(Guid staffId)
        {
            return await context.Appeals
                .Where(a => a.StaffId == staffId && a.AppealStatus == AppealStatus.UnderReview)
                .Include(a => a.Staff)
                .Include(a => a.Review)
                .ToListAsync();
        }

    }
}
