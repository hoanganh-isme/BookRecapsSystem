using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class SupportTicket : BaseEntity
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
        public SupportStatus? Status { get; set; } // open, pending, closed
        public string? Response { get; set; }
        public Guid RecapId {  get; set; }
        public Recap Recaps { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
