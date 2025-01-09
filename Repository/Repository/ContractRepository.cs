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
    public class ContractRepository : BaseRepository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Contract?> GetContractByIdWithBook(Guid contractId)
        {
            return await context.Contracts
               .Include(b => b.Books).Where(b => b.Id == contractId).FirstOrDefaultAsync();
        }
        public async Task<Contract?> GetContractByIdWithAttachment(Guid contractId)
        {
            return await context.Contracts
               .Include(b => b.ContractAttachments).Where(b => b.Id == contractId).FirstOrDefaultAsync();
        }
        public async Task<Contract?> GetValidContractByBookIdAsync(Guid bookId)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return await context.Contracts
                .Include(c => c.Books)
                .Where(c => c.Books.Any(b => b.Id == bookId)
                    && c.StartDate <= currentDate
                    && (c.EndDate == null || c.EndDate >= currentDate))
                .FirstOrDefaultAsync();
        }
        public async Task<List<Contract>> GetValidContractsByBookIdsAsync(List<Guid> bookIds)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Truy vấn các hợp đồng mà có ít nhất một cuốn sách thuộc danh sách bookIds và hợp lệ theo ngày
            return await context.Contracts
                .Include(c => c.Books)
                .Where(c => c.Books.Any(b => bookIds.Contains(b.Id)) // Kiểm tra xem sách có nằm trong danh sách bookIds không
                    && c.StartDate <= currentDate
                    && (c.EndDate == null || c.EndDate >= currentDate))
                .ToListAsync();
        }

    }
}
