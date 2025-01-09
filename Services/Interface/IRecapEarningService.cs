using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.ViewModels.Dashboards;
using Services.Responses;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;
using static BusinessObject.ViewModels.ContributorPayouts.RecapDetailsDTO;

namespace Services.Interface
{
    public interface IRecapEarningService
    {
        Task<ApiResponse<ContributorEarningsDto>> GetContributorEarningsAsync(Guid contributorId, DateTime toDate);
        Task<ApiResponse<RecapDetailDto>> GetRecapDetailAsync(Guid recapId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<RecapAdminDetailDto>> GetRecapDetailForAdminAsync(Guid recapId, DateTime fromDate, DateTime toDate);
    }
}
