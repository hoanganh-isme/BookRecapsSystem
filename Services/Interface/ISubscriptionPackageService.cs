using BusinessObject.Models;
using BusinessObject.ViewModels.SubscriptionPackage;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISubscriptionPackageService
    {
        Task<ApiResponse<SubscriptionPackage>> CreateSubscriptionPackage(CreateSubscriptionPackage subscriptionPackage);
        Task<ApiResponse<SubscriptionPackage>> UpdateSubscriptionPackage(Guid SubscriptionPackageId, UpdateSubscriptionPackage subscriptionPackage);
        Task<ApiResponse<bool>> DeleteSubscriptionPackage(Guid SubscriptionPackageId);
        Task<ApiResponse<bool>> SoftDeleteSubscriptionPackage(Guid SubscriptionPackageId);
        Task<ApiResponse<SubscriptionPackage>> GetSubscriptionPackageById(Guid SubscriptionPackageId);
        Task<ApiResponse<List<SubscriptionPackage>>> GetAllSubscriptionPackages();
    }
}
