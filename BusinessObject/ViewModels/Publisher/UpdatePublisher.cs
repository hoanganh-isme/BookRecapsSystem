using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Publisher
{
    public class UpdatePublisher
    {
        public string? PublisherName { get; set; }
        public string? ContactInfo { get; set; }
        public string? BankAccount { get; set; }
        public decimal RevenueSharePercentage { get; set; }
    }
}
