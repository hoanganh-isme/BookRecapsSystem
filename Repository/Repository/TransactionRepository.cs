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
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Transaction?> FindByOrderCode(int orderCode)
        {
            return await context.Transactions
                                 .FirstOrDefaultAsync(t => t.OrderCode == orderCode);
        }
    }
}
