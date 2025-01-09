using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPlayListRepository : IBaseRepository<PlayList>
    {
        Task<List<PlayListItem>> GetPlayListItemsByRecapIdAndUserId(Guid recapId, Guid userId);
        Task<List<PlayList>> GetPlayListsWithDetailsByUserId(Guid userId);
    }
}
