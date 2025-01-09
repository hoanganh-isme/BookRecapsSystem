using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class RecapAdminDetailDto
    {
        public Guid RecapId { get; set; }
        public string RecapName { get; set; }
        public bool isPublished { get; set; }
        public bool isPremium { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public string CurrentVersionName { get; set; }
        public int TotalViews { get; set; }
        public int TotalWatchTime { get; set; }
        public List<DailyViewValueStatDto> DailyViewValueStats { get; set; }
        public List<DailyPlatformStatDto> DailyPlatformStats { get; set; }
    }
}
