using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.SubscriptionPackage;
using BusinessObject.ViewModels.SystemSetting;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Service
{
    public class SubscriptionPackageService : ISubscriptionPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionPackageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SubscriptionPackage>> CreateSubscriptionPackage(CreateSubscriptionPackage subscriptionPackage)
        {
            var newSubscriptionPackage = _mapper.Map<SubscriptionPackage>(subscriptionPackage);
            await _unitOfWork.SubscriptionPackageRepository.AddAsync(newSubscriptionPackage);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<SubscriptionPackage>
            {
                Succeeded = true,
                Message = "Tạo gói mua thành công.",
                Data = newSubscriptionPackage
            };
        }

        public async Task<ApiResponse<SubscriptionPackage>> UpdateSubscriptionPackage(Guid subscriptionPackageId, UpdateSubscriptionPackage request)
        {
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                return new ApiResponse<SubscriptionPackage>
                {
                    Succeeded = false,
                    Message = "Subscription package not found.",
                    Errors = new[] { "The specified subscription package does not exist." }
                };
            }

            _mapper.Map(request, subscriptionPackage);
            _unitOfWork.SubscriptionPackageRepository.Update(subscriptionPackage);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<SubscriptionPackage>
            {
                Succeeded = true,
                Message = "Cập nhật gói mua thành công.",
                Data = subscriptionPackage
            };
        }

        public async Task<ApiResponse<bool>> DeleteSubscriptionPackage(Guid subscriptionPackageId)
        {
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Subscription package not found.",
                    Errors = new[] { "The specified subscription package does not exist." }
                };
            }

            _unitOfWork.SubscriptionPackageRepository.Delete(subscriptionPackage);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa gói mua thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> SoftDeleteSubscriptionPackage(Guid subscriptionPackageId)
        {
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Subscription package not found.",
                    Errors = new[] { "The specified subscription package does not exist." }
                };
            }

            _unitOfWork.SubscriptionPackageRepository.SoftDelete(subscriptionPackage);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa gói mua thành công",
                Data = true
            };
        }

        public async Task<ApiResponse<SubscriptionPackage>> GetSubscriptionPackageById(Guid subscriptionPackageId)
        {
            var subscriptionPackage = await _unitOfWork.SubscriptionPackageRepository.GetByIdAsync(subscriptionPackageId);
            if (subscriptionPackage == null)
            {
                return new ApiResponse<SubscriptionPackage>
                {
                    Succeeded = false,
                    Message = "Subscription package not found.",
                    Errors = new[] { "The specified subscription package does not exist." }
                };
            }

            return new ApiResponse<SubscriptionPackage>
            {
                Succeeded = true,
                Message = "Subscription package retrieved successfully.",
                Data = subscriptionPackage
            };
        }

        public async Task<ApiResponse<List<SubscriptionPackage>>> GetAllSubscriptionPackages()
        {
            var subscriptionPackages = await _unitOfWork.SubscriptionPackageRepository.GetAllAsync();
            if (!subscriptionPackages.Any())
            {
                return new ApiResponse<List<SubscriptionPackage>>
                {
                    Succeeded = false,
                    Message = "No subscription packages found.",
                    Errors = new[] { "No data available." }
                };
            }

            return new ApiResponse<List<SubscriptionPackage>>
            {
                Succeeded = true,
                Message = "Subscription packages retrieved successfully.",
                Data = subscriptionPackages
            };
        }
    }
}
