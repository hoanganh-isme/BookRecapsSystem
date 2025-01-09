using BusinessObject.Models;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;

namespace Services.Interface
{
    public interface IContributorPayoutService
    {
        Task<ApiResponse<ContributorPayoutDto>> CreateContributorPayoutAsync(Guid contributorId, string description, DateTime toDate);
        Task<ApiResponse<ContributorPayout>> GetPayoutById(Guid Id);
        Task<List<ContributorDto>> GetAllContributorWithPayoutsAsync();
        Task<ApiResponse<List<ContributorDto>>> GetPayoutByContributorId(Guid contributorId);
    }
}
