using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Contracts
{
    public class UpdateContract
    {
        public Guid PublisherId { get; set; }
        public bool SeparateRevenueShare { get; set; }
        public decimal? RevenueSharePercentage { get; set; }
        public bool AutoRenew { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ContractStatus Status { get; set; }
    }
}
