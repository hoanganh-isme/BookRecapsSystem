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
    public class PublisherRepository : BaseRepository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Publisher?> GetPublisherByUserIdAsync(Guid userId)
        {
            return await context.Publishers.FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
