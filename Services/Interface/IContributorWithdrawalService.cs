using BusinessObject.Models;
using BusinessObject.ViewModels.Withdrawal;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IContributorWithdrawalService
    {
        Task<ApiResponse<HistoryWithDrawal>> GetDrawalByContributorId(Guid contributorId);
        Task<RecapResponse<ContributorWithdrawal, object>> GetWithdrawalById(Guid withdrawalId);
        Task<ApiResponse<ContributorWithdrawal>> AcceptWithdrawal(Guid contributorId, Stream ImageStream, string ImageContentType, ProcessWithdrawal processWithdrawal);
        Task<ApiResponse<ContributorWithdrawal>> CreateDrawal(Guid contributorId, decimal amount);
        Task<List<ContributorWithdrawalDTO>> GetAllContributorWithDrawalsAsync();
        Task<List<ContributorWithdrawalDTO>> GetAllDrawalsAsync();
    }
}
