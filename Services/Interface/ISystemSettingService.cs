using BusinessObject.Models;
using BusinessObject.ViewModels.SystemSetting;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISystemSettingService
    {
        Task<ApiResponse<SystemSetting>> CreateSystemSetting(CreateSystemSetting systemSetting);
        Task<ApiResponse<SystemSetting>> UpdateSystemSetting(Guid systemSettingId, SystemSettingUpdateRequest systemSetting);
        Task<ApiResponse<bool>> DeleteSystemSetting(Guid systemSettingId);
        Task<ApiResponse<bool>> SoftDeleteSystemSetting(Guid systemSettingId);
        Task<ApiResponse<SystemSetting>> GetSystemSettingById(Guid systemSettingId);
        Task<ApiResponse<List<SystemSetting>>> GetAllSystemSettings();
    }
}
