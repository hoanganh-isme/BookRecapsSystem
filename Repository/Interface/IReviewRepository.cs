using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<List<Review>> GetReviewsByStaffAsync(Guid staffId);
        Task<List<Review>> GetReviewsByRecapVersionIds(List<Guid> recapVersionIds);
    }
}
