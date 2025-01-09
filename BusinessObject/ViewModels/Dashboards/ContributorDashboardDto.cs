using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class ContributorDashboardDto
    {
        public decimal TotalIncome { get; set; }
        public decimal LastPayoutAmount { get; set; }
        public int NewRecapsCount { get; set; }
        public int OldRecapsCount { get; set; }
        public int NewViewCount { get; set; }
        public int OldViewCount { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal? CurrentEarnings { get; set; }
        public List<RecapDashboardDto> Recaps { get; set; }
        public List<MostRecapDashboardDto> MostViewedRecaps { get; set; }
    }
    public class RecapDashboardDto
    {
        public Guid RecapId { get; set; }
        public string RecapName { get; set; }
        public string BookImage {  get; set; }
        public string BookName {  get; set; }
        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }
        public bool isPublished {  get; set; }
    }
    public class MostRecapDashboardDto
    {
        public Guid RecapId { get; set; }
        public string RecapName { get; set; }
        public string BookImage { get; set; }
        public string BookName { get; set; }
        public int ViewsCount { get; set; }
        public int LikesCount {  get; set; }
        public bool isPublished { get; set; }
    }

}
