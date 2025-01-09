using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.ContributorPayouts
{
    public class RecapDetailsDTO
    {
        public class RecapDetailDto
        {
            public Guid RecapId { get; set; }
            public string RecapName { get; set; }
            public bool isPublished { get; set; }
            public bool isPremium { get; set; }
            public Guid? CurrentVersionId { get; set; }
            public string CurrentVersionName { get; set; }
            public decimal UnpaidEarning { get; set; }
            public int TotalViews { get; set; }
            public int TotalWatchTime { get; set; }
            public decimal TotalEarnings { get; set; }
            public LatestPayoutDto LastPayout { get; set; }
            public List<DailyRecapStatDto> DailyStats { get; set; }
        }
        public class DailyRecapStatDto
        {
            public DateTime Date { get; set; }
            public int Views { get; set; }
            public int WatchTime { get; set; }
            public decimal Earning { get; set; }
        }
        public class LatestPayoutDto
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public decimal Amount { get; set; }
        }

    }
}
