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
    public class PlayListItemRepository : BaseRepository<PlayListItem>, IPlayListItemRepository
    {
        public PlayListItemRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<PlayListItem>> GetAllPlaylistItemByPlaylistId (Guid playlistId)
        {
            return await context.PlayListItems.Where(pl => pl.PlayListId == playlistId).ToListAsync();
        }
    }
}
