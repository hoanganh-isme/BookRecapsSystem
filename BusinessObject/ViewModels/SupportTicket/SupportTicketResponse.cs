using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.SupportTicket
{
    public class SupportTicketResponse
    {
        public Guid SupportTicketId { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public SupportStatus? Status { get; set; }
        public string? Response { get; set; }
        public Guid UserId { get; set; }
        public Guid RecapId { get; set; }
        public string? RecapName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public RecapVersionResponse? CurrentVersion { get; set; }
    }

    public class RecapVersionResponse
    {
        public Guid VersionId { get; set; }
        public string? VersionName { get; set; }
        public RecapStatus Status { get; set; }
        public ReviewResponse? Review { get; set; }
    }

    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public string? Comments { get; set; }
        public Guid? StaffId { get; set; }
    }

}
