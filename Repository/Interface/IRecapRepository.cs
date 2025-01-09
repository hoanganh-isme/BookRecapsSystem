using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRecapRepository : IBaseRepository<Recap>
    {
        Task<List<Recap>> GetRecapsByContributorIdAsync(Guid contributorId);
        Task<int> GetNewRecaps(DateTime fromDate, DateTime toDate);
        Task<int> CountRecapsCreatedBetweenDatesAsync(List<Guid> bookIds, DateTime fromDate, DateTime toDate);
        Task<List<Recap>> GetRecapsByBookIdAsync(Guid bookId);
        Task<int> CountRecapsCreatedBetweenDatesAsync(Guid contributorId, DateTime fromDate, DateTime toDate);
        Task<List<Recap>> GetMostViewedRecapsByContributorIdAsync(Guid contributorId, int topCount);
        Task<List<Recap>> GetAllRecapsByContributorIdAsync(Guid contributorId);
    }
}
