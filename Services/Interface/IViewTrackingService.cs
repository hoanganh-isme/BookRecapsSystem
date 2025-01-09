using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Viewtrackings;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IViewTrackingService
    {
        public Task<ApiResponse<ViewTracking>> CreateViewTrackingAsync(Guid recapId, Guid? userId, DeviceType deviceType);
        public Task<ApiResponse<ViewTracking>> GetViewTrackingById(Guid trackingId);
        public Task<ApiResponse<List<ViewTracking>>> GetAllViewTrackings();
        Task<ApiResponse<PaginationResponse<ViewTrackingDTO>>> GetViewTrackingByUserId(Guid userId, int pageNumber, int pageSize);
        Task<ApiResponse<ViewTracking>> UpdateDurationViewtracking(Guid trackingId, int duration);
    }
}
