using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Withdrawal
{
    public class ContributorWithdrawalDTO
    {
        public Guid drawalId {  get; set; }
        public Guid contributorId { get; set; }
        public string ContributorName { get; set; }
        public string? Description {  get; set; }
        public decimal? TotalEarnings { get; set; }
        public string? BankAccount { get; set; }
        public string ImageURL { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Status { get; set; }
    }
}
