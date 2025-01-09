using BusinessObject.Data;
using BusinessObject.Enums;
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
    public class RecapRepository : BaseRepository<Recap>, IRecapRepository
    {
        public RecapRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<Recap>> GetRecapsByContributorIdAsync(Guid contributorId)
        {
            // Lấy tất cả các Recap của contributorId
            var recaps = await context.Recaps
                .Where(r => r.UserId == contributorId)
                .Include(r => r.Contributor)
                .Include(r => r.ViewTrackings)// Bao gồm thông tin Contributor
                .ToListAsync();

            //// Lấy tất cả các ViewTracking có RecapId trong danh sách Recap
            //var viewTrackings = await context.ViewTrackings
            //    .Where(vt => vt.Recap.UserId == contributorId && vt.UserId != contributorId) // Lọc theo Recap và UserId khác contributorId
            //    .ToListAsync();

            //// Áp dụng các ViewTracking vào Recap tương ứng
            //foreach (var recap in recaps)
            //{
            //    recap.ViewTrackings = viewTrackings.Where(vt => vt.RecapId == recap.Id).ToList();
            //}

            return recaps;
        }

        public async Task<int> GetNewRecaps(DateTime fromDate, DateTime toDate)
        {
            return await context.Recaps
                .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .CountAsync();
        }
        public async Task<int> CountRecapsCreatedBetweenDatesAsync(List<Guid> bookIds, DateTime fromDate, DateTime toDate)
        {
            return await context.Recaps
                .Where(r => bookIds.Contains(r.BookId) && r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .CountAsync();
        }
        public async Task<List<Recap>> GetRecapsByBookIdAsync(Guid bookId)
        {
            return await context.Set<Recap>()
                .Where(recap => recap.BookId == bookId)
                .ToListAsync();
        }
        public async Task<int> CountRecapsCreatedBetweenDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            return await context.Recaps
                .Where(r => r.UserId == contributorId && r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .CountAsync();
        }
        public async Task<List<Recap>> GetAllRecapsByContributorIdAsync(Guid contributorId)
        {
            return await context.Recaps
                .Include(r => r.Book)
                .Where(r => r.UserId == contributorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<Recap>> GetMostViewedRecapsByContributorIdAsync(Guid contributorId, int topCount)
        {
            return await context.Recaps
                .Where(r => r.UserId == contributorId && r.isPublished) // Chỉ lấy recap đã xuất bản
                .OrderByDescending(r => r.ViewsCount)
                .Take(topCount)
                .ToListAsync();
        }

    }
}
