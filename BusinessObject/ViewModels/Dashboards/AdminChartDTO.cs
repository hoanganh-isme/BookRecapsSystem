using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class AdminChartDTO
    {
        public class AdminChartDto
        {
            public List<DailyPackageChartDTO> DailyPackageStats { get; set; }
            public List<DailyViewChartDTO> DailyViewStats { get; set; }
        }
        public class DailyPackageChartDTO
        {
            public DateTime Date { get; set; }
            public string PackageName { get; set; }
            public decimal? Earning { get; set; }
            public int Count { get; set; }
        }

        public class DailyViewChartDTO
        {
            public DateTime Date { get; set; }
            public decimal RevenueEarning { get; set; }
            public decimal ProfitEarning { get; set; }
            public int ViewCount { get; set; }
        }
    }
}
