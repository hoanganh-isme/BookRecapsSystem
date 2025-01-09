using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPublisherPayoutRepository : IBaseRepository<PublisherPayout>
    {
        
        Task<PublisherPayout?> GetLastPayoutByPublisherIdAsync(Guid publisherId);
        Task<List<PublisherPayout>> GetLatestPayoutsForAllPublishersAsync();
        Task<List<PublisherPayout>> GetListPayoutByPublisherId(Guid publisherId);
        Task<PublisherPayout> GetPayoutWithDetailsByIdAsync(Guid payoutId);
    }
}
