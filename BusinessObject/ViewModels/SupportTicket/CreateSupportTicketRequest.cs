using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.SupportTicket
{
    public class CreateSupportTicketRequest
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
        public Guid RecapId { get; set; }
        public Guid UserId { get; set; }
    }
}
