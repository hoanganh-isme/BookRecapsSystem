using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IContributorWithdrawalRepository : IBaseRepository<ContributorWithdrawal>
    {
        Task<List<ContributorWithdrawal>> GetLatestDrawalForAllContributorsAsync();
        Task<List<ContributorWithdrawal>> GetListDrawalByContributorId(Guid contributorId);
        Task<ContributorWithdrawal> GetDrawalByContributorIdAsync(Guid contributorId);
    }
}
