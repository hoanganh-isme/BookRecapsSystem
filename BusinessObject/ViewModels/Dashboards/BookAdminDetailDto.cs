using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class BookAdminDetailDto
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Description { get; set; }
        public int PublicationYear { get; set; }
        public string CoverImage { get; set; }
        public int TotalViews { get; set; }
        public int TotalWatchTime { get; set; }
        public List<DailyViewValueStatDto> DailyViewValueStats { get; set; }
        public List<DailyPlatformStatDto> DailyPlatformStats { get; set; }
    }
    public class DailyPlatformStatDto
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int WatchTime { get; set; }
        public decimal Earning { get; set; }
    }
    public class DailyViewValueStatDto
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int WatchTime { get; set; }
        public decimal Earning { get; set; }
    }
}
