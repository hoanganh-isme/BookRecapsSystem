using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;

namespace BusinessObject.ViewModels.ContributorPayouts
{
    public class ContributorPayoutDTO
    {
        public class ContributorPayoutDto
        {
            public Guid PayoutId { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public List<RecapEarningDto> RecapEarnings { get; set; }
        }

        public class ContributorEarningsDto
        {
            public string ContributorName { get; set; }
            public Guid ContributorId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string BankAccount { get; set; }
            public decimal TotalEarnings { get; set; }
            public DateTime Fromdate { get; set; }
            public DateTime Todate { get; set; }
            public List<RecapEarningsDto> RecapDetails { get; set; }
        }
        public class ContributorDto
        {
            public Guid payoutId { get; set; }
            public Guid contributorId { get; set; }
            public string ContributorName { get; set; }
            public string Description { get; set; }
            public decimal? TotalEarnings { get; set; }
            public DateTime? Fromdate { get; set; }
            public DateTime? Todate { get; set; }
            public string Status { get; set; }
            public DateTime CreateAt { get; set; }
        }

        public class RecapEarningsDto
        {
            public Guid RecapId { get; set; }
            public string RecapName { get; set; }
            public int? ViewCount { get; set; }
            public decimal RecapEarnings { get; set; }
        }

        public class RecapEarningDto
        {
            public Guid RecapId { get; set; }
            public string RecapName { get; set; }
            public int? ViewCount { get; set; }
            public decimal TotalEarnings { get; set; }
        }
    }

}
