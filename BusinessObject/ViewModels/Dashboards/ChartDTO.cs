using BusinessObject.ViewModels.PublisherPayouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class ChartDTO
    {
        public int TotalViews { get; set; }
        public int TotalWatchTime { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<DailyBookStatDto> DailyStats { get; set; }
    }
}
