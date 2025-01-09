using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ILikeRepository : IBaseRepository<Like>
    {
        Task<Like?> FindLikeByUserAndRecapAsync(Guid userId, Guid recapId);
        Task<bool> IsRecapLikedByUser(Guid userId, Guid recapId);
    }
}
