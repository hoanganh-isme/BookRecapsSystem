using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.SupportTicket
{
    public class UpdateSupportTicketRequest
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
    }
}
