using BusinessObject.ViewModels.Dashboards;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.Dashboards.AdminChartDTO;

namespace Services.Interface
{
    public interface IDashboardService
    {
        Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync(DateTime fromDate, DateTime toDate);
        Task<ApiResponse<AdminChartDto>> GetAdminChartAsync(DateTime fromDate, DateTime toDate);
        Task<List<PackageSalesDto>> GetPackageSalesAsync(DateTime fromDate, DateTime toDate);
        Task<ApiResponse<PublisherDashboardDto>> GetPublisherDashboardAsync(Guid publisherId);
        Task<ApiResponse<ChartDTO>> GetPublisherChartDashboardAsync(Guid publisherId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<ContributorDashboardDto>> GetContributorDashboardAsync(Guid contributorId);
        Task<ApiResponse<ChartDTO>> GetContributorChartDashboardAsync(Guid contributorId, DateTime fromDate, DateTime toDate);
    }
}
