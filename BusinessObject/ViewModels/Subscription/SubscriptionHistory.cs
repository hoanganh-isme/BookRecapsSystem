using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Subscription
{
    public class SubscriptionHistory
    {
        public CurrentSubscriptionDto? CurrentSubscription { get; set; }
        public List<HistorySubscriptionDto> HistorySubscriptions { get; set; } = new List<HistorySubscriptionDto>();

    }
    public class CurrentSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public string PackageName { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? ExpectedViewsCount {  get; set; }
        public int? ActualViewsCount { get; set; }
        public DateTime CreateAt {  get; set; }
    }

    public class HistorySubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public string PackageName { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public SubStatus? Status { get; set; }
        public int? ExpectedViewsCount { get; set; }
        public int? ActualViewsCount { get; set; }
        public DateTime CreateAt { get; set; }
    }

}
