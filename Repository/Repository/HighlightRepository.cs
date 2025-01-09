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
    public class HighlightRepository : BaseRepository<Highlight>, IHighlightRepository
    {
        public HighlightRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<Highlight>> GetHighlightByUserId(Guid recapId, Guid userId)
        {
            return await context.Highlights
                .Where(x => x.RecapVersionId == recapId && x.UserId == userId)
                .ToListAsync();
        }
    }
}
