using BusinessObject.ViewModels.ContributorPayouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.RecapDetailsDTO;

namespace BusinessObject.ViewModels.PublisherPayouts
{
    public class BookDetailDto
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Description { get; set; }
        public int PublicationYear { get; set; }
        public string CoverImage { get; set; }
        public decimal UnpaidEarning { get; set; }
        public int TotalViews { get; set; }
        public int TotalWatchTime { get; set; }
        public decimal TotalEarnings {  get; set; }
        public LatestBookPayoutDto LastPayout { get; set; }
        public List<DailyBookStatDto> DailyStats { get; set; }
    }
    public class DailyBookStatDto
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int WatchTime { get; set; }
        public decimal Earning { get; set; }
    }
    public class LatestBookPayoutDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Amount { get; set; }
    }
}
