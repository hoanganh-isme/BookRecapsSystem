using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IContributorPayoutRepository : IBaseRepository<ContributorPayout>
    {
        Task<ContributorPayout> GetLastPayoutByContributorIdAsync(Guid contributorId);
        Task<List<ContributorPayout>> GetLatestPayoutsForAllContributorsAsync();
        Task<List<ContributorPayout>> GetListPayoutByContributorId(Guid contributorId);
        Task<ContributorPayout> GetPayoutWithDetailsByIdAsync(Guid payoutId);
        Task<decimal> GetTotalExpenses();
    }
}
