using BusinessObject.Data;
using BusinessObject.Models;
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
    public class LikeRepository : BaseRepository<Like>, ILikeRepository
    {
        public LikeRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Like?> FindLikeByUserAndRecapAsync(Guid userId, Guid recapId)
        {
            return await context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecapId == recapId);
        }
        public async Task<bool> IsRecapLikedByUser(Guid userId, Guid recapId)
        {
            return await context.Likes
                .AnyAsync(like => like.UserId == userId && like.RecapId == recapId);
        }
    }
}
