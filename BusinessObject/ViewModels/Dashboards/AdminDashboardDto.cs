using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class AdminDashboardDto
    {
        public decimal RevenueFromPackages { get; set; }
        public List<PackageSalesDto> PackageSales { get; set; }
        public int NewRecaps { get; set; }
        public int TotalViews { get; set; }
        public decimal RevenueFromViews { get; set; }
        public decimal PlatformProfit { get; set; }
        public decimal PublisherExpense { get; set; }
        public decimal ContributorExpense { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
    }
    public class ViewTrackingSummaryDto
    {
        public int TotalViews { get; set; }
        public decimal RevenueFromViews { get; set; }
        public decimal PlatformProfit { get; set; }
        public decimal PublisherExpense { get; set; }
        public decimal ContributorExpense { get; set; }
    }
    public class PackageSalesDto
    {
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? Price { get; set; }
        public int Count { get; set; }
    }

}
