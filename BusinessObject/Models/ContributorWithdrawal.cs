using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ContributorWithdrawal : BaseEntity
    {
        public Guid ContributorId { get; set; }
        public string? Description {  get; set; }
        public decimal? Amount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public User Contributor { get; set; }

    }
}
