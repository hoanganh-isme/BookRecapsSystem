using BusinessObject.Data;
using BusinessObject.Enums;
using BusinessObject.Models;
using Google;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class RecapVersionRepository : BaseRepository<RecapVersion>, IRecapVersionRepository
    {
        public RecapVersionRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<RecapVersion> GetLatestVersionAsync()
        {
            return await context.Set<RecapVersion>()
                .OrderByDescending(cv => cv.VersionNumber)
                .FirstOrDefaultAsync();
        }
        public async Task<List<RecapVersion>> GetVersionApprovedbyRecapId(Guid recapId)
        {
            return await context.RecapVersions
                .Where(version => version.RecapId == recapId 
                && version.Status == RecapStatus.Approved
                && version.IsDeleted == false)
                .ToListAsync();
        }
        public async Task<List<RecapVersion>> GetAllRecapVersionNotDraft()
        {
            return await context.RecapVersions
                .Include(x => x.Recap)
                .Include(x => x.Recap.Book)
                .Include(x => x.Recap.Contributor)
                .Include(x => x.Review)
                .Where(version =>  version.Status != RecapStatus.Draft
                && version.IsDeleted == false)
                .OrderByDescending(version => version.CreatedAt)
                .ToListAsync();
        }
    }
}
