using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Contract : BaseEntity
    {
        public Guid? PublisherId { get; set; }
        public decimal? RevenueSharePercentage { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public bool AutoRenew { get; set; }
        public Publisher? Publisher { get; set; }
        public ICollection<ContractAttachment> ContractAttachments { get; set; }
        public ICollection<Book> Books { get; set; }

    }
}
