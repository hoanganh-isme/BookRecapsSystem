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
    public class RecapEarningRepository : BaseRepository<RecapEarning>, IRecapEarningRepository
    {
        public RecapEarningRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<RecapEarning> GetRecapEarningByRecapId(Guid recapId)
        {
            return await context.RecapEarnings.Where(p => p.RecapId == recapId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
