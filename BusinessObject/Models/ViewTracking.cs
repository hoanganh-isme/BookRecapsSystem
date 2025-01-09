using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ViewTracking : BaseEntity
    {
        public Guid RecapId { get; set; }
        public Guid UserId { get; set; }
        public decimal? ViewValue { get; set; }
        public decimal? PublisherValueShare { get; set; }
        public decimal? ContributorValueShare { get; set; }
        public decimal? PlatformValueShare {  get; set; }
        public bool isPremium { get; set; }
        public int? Durations { get; set; }
        public DeviceType DeviceType { get; set; }
        public Guid? SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }
        public User User { get; set; }
        public Recap Recap { get; set; }
    }
}
