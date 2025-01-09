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
    public class PlayListRepository : BaseRepository<PlayList>, IPlayListRepository
    {
        public PlayListRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<PlayListItem>> GetPlayListItemsByRecapIdAndUserId(Guid recapId, Guid userId)
        {
            return await context.PlayLists
                .Where(p => p.UserId == userId)
                .SelectMany(p => p.PlayListItems)
                .Where(item => item.RecapId == recapId)
                .ToListAsync();
        }
        public async Task<List<PlayList>> GetPlayListsWithDetailsByUserId(Guid userId)
        {
            return await context.PlayLists
                .Include(pl => pl.PlayListItems)
                    .ThenInclude(pli => pli.Recap)
                        .ThenInclude(r => r.Book)
                            .ThenInclude(b => b.Authors) // Bao gồm Authors tại đây
                .Include(pl => pl.PlayListItems)
                    .ThenInclude(pli => pli.Recap.Contributor)
                .Where(pl => pl.UserId == userId)
                .ToListAsync();
        }


    }
}
