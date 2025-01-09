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
    public class BookEarningRepository : BaseRepository<BookEarning>, IBookEarningRepository
    {
        public BookEarningRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<BookEarning> GetBookEarningByBookId(Guid bookId)
        {
            return await context.BookEarnings.Where(p => p.BookId == bookId)
                .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
        }
    }
}
