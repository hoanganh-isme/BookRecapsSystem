using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ContributorPayout : BaseEntity
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public PayoutStatus Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid UserId { get; set; }
        public User Contributor { get; set; }
        public ICollection<RecapEarning> RecapEarnings { get; set; }
    }
}
