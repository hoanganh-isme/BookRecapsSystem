using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.SystemSetting;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SystemSettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResponse<SystemSetting>> CreateSystemSetting(CreateSystemSetting systemSetting)
        {

            var newSystemSetting = _mapper.Map<SystemSetting>(systemSetting);
            await _unitOfWork.SystemSettingRepository.AddAsync(newSystemSetting);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<SystemSetting>
            {
                Succeeded = true,
                Message = "Tạo cài đặt hệ thống thành công.",
                Data = newSystemSetting
            };
        }

        public async Task<ApiResponse<SystemSetting>> UpdateSystemSetting(Guid systemSettingId, SystemSettingUpdateRequest request)
        {
            var systemSetting = await _unitOfWork.SystemSettingRepository.GetByIdAsync(systemSettingId);
            if (systemSetting == null)
            {
                return new ApiResponse<SystemSetting>
                {
                    Succeeded = false,
                    Message = "System setting not found.",
                    Errors = new[] { "The specified system setting does not exist." }
                };
            }

            _mapper.Map(request, systemSetting);
            _unitOfWork.SystemSettingRepository.Update(systemSetting);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<SystemSetting>
            {
                Succeeded = true,
                Message = "Cập nhật cài đặt hệ thống thành công.",
                Data = systemSetting
            };
        }

        public async Task<ApiResponse<bool>> DeleteSystemSetting(Guid systemSettingId)
        {
            var systemSetting = await _unitOfWork.SystemSettingRepository.GetByIdAsync(systemSettingId);
            if (systemSetting == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "System setting not found.",
                    Errors = new[] { "The specified system setting does not exist." }
                };
            }

            _unitOfWork.SystemSettingRepository.Delete(systemSetting);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa cài đặt hệ thống thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> SoftDeleteSystemSetting(Guid systemSettingId)
        {
            var systemSetting = await _unitOfWork.SystemSettingRepository.GetByIdAsync(systemSettingId);
            if (systemSetting == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "System setting not found.",
                    Errors = new[] { "The specified system setting does not exist." }
                };
            }

            _unitOfWork.SystemSettingRepository.SoftDelete(systemSetting);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa cài đặt hệ thống thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<SystemSetting>> GetSystemSettingById(Guid systemSettingId)
        {
            var systemSetting = await _unitOfWork.SystemSettingRepository.GetByIdAsync(systemSettingId);
            if (systemSetting == null)
            {
                return new ApiResponse<SystemSetting>
                {
                    Succeeded = false,
                    Message = "System setting not found.",
                    Errors = new[] { "The specified system setting does not exist." }
                };
            }

            return new ApiResponse<SystemSetting>
            {
                Succeeded = true,
                Message = "System setting retrieved successfully.",
                Data = systemSetting
            };
        }

        public async Task<ApiResponse<List<SystemSetting>>> GetAllSystemSettings()
        {
            var systemSettings = await _unitOfWork.SystemSettingRepository.GetAllAsync();
            if (!systemSettings.Any())
            {
                return new ApiResponse<List<SystemSetting>>
                {
                    Succeeded = false,
                    Message = "No system settings found.",
                    Errors = new[] { "No data available." }
                };
            }

            return new ApiResponse<List<SystemSetting>>
            {
                Succeeded = true,
                Message = "System settings retrieved successfully.",
                Data = systemSettings
            };
        }
    }
}
