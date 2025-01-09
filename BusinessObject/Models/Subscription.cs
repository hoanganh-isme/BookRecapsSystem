using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Subscription : BaseEntity
    {
        public decimal? Price { get; set; }
        public DateOnly? StartDate {  get; set; }
        public DateOnly? EndDate { get;set; }
        public int? Durations {  get; set; }
        public SubStatus? Status { get; set; }
        public int? ExpectedViewsCount { get; set; }
        public int? ActualViewsCount { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid? TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public Guid SubscriptionPackageId { get; set; }
        public SubscriptionPackage SubscriptionPackage { get; set; }
        public ICollection<ViewTracking> ViewTrackings { get; set; }
    }
}
