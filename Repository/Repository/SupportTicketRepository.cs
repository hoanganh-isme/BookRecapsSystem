using BusinessObject.Data;
using BusinessObject.Models;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class SupportTicketRepository : BaseRepository<SupportTicket>, ISupportTicketRepository
    {
        public SupportTicketRepository(AppDbContext context) : base(context)
        {
        }
    }
}
