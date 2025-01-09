using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IHighlightRepository : IBaseRepository<Highlight>
    {
        Task<List<Highlight>> GetHighlightByUserId(Guid userId, Guid recapId);
    }
}
