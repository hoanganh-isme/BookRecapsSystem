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
    public class PublisherPayoutRepository : BaseRepository<PublisherPayout>, IPublisherPayoutRepository
    {
        public PublisherPayoutRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<PublisherPayout?> GetLastPayoutByPublisherIdAsync(Guid publisherId)
        {
            return await context.PublisherPayouts
                .Where(p => p.PublisherId == publisherId)
                .OrderByDescending(p => p.ToDate)  // Sắp xếp để lấy payout gần nhất
                .FirstOrDefaultAsync();
        }
        public async Task<List<PublisherPayout>> GetListPayoutByPublisherId(Guid publisherId)
        {
            return await context.PublisherPayouts
                .Include(p => p.Publisher)
                .Where(p => p.PublisherId == publisherId)
                .OrderByDescending(p => p.ToDate)
                .ToListAsync();
        }
        public async Task<List<PublisherPayout>> GetLatestPayoutsForAllPublishersAsync()
        {
            return await context.PublisherPayouts
                .AsNoTracking()
                .GroupBy(p => p.PublisherId) // Group payouts by UserId
                .Select(g => g.OrderByDescending(p => p.ToDate).FirstOrDefault()) // Lấy payout mới nhất
                .ToListAsync();
        }
        public async Task<PublisherPayout> GetPayoutWithDetailsByIdAsync(Guid payoutId)
        {
            return await context.PublisherPayouts
                .Include(p => p.Publisher) // Bao gồm thông tin Publisher
                .Include(p => p.BookEarnings) // Bao gồm BookEarnings
                .ThenInclude(be => be.Book) // Bao gồm thông tin Book qua BookEarnings
                .AsNoTracking() // Tăng hiệu năng khi chỉ đọc dữ liệu
                .FirstOrDefaultAsync(p => p.Id == payoutId);
        }

    }
}
