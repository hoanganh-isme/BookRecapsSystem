using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Subscription
{
    public class CreateSubscription
    {
        public decimal? Price { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? Durations { get; set; }
        public SubStatus? Status { get; set; }
        public int? ExpectedViewsCount { get; set; }
        public int? ActualViewsCount { get; set; }
        public Guid UserId { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid SubscriptionPackageId { get; set; }
    }
}
