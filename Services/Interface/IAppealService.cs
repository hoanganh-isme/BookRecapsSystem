using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Appeals;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAppealService
    {
        Task<ApiResponse<Appeal>> CreateAppeal(CreateAppealRequest appeal);
        Task<ApiResponse<Appeal>> UpdateAppeal(UpdateAppealContributor appeal, Guid userId);
        Task<ApiResponse<Appeal>> ResponseAppeal(UpdateAppealResponse appeal, Guid staffId, RecapStatus status);
        Task<ApiResponse<Appeal>> ChangeAppealStatus(UpdateAppealStatus appeal, Guid userId);
        Task<ApiResponse<Appeal>> GetAppealByReviewId(Guid reviewId);
        Task<ApiResponse<bool>> DeleteAppeal(Guid appealId, Guid userId);
        Task<ApiResponse<bool>> SoftDeleteAppeal(Guid appealId, Guid userId);
        Task<ApiResponse<Appeal>> GetAppealById(Guid appealId);
        Task<ApiResponse<List<Appeal>>> GetAppealByStaffId(Guid staffId);
        Task<ApiResponse<List<Appeal>>> GetAppealByUserId(Guid userId);
        Task<ApiResponse<List<Appeal>>> GetAllAppealsWithUnderReviewStatus();
        Task<ApiResponse<List<Appeal>>> GetAllAppeals();
    }
}
