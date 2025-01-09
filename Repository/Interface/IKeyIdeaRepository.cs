using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IKeyIdeaRepository : IBaseRepository<KeyIdea>
    {
        IQueryable<KeyIdea> GetKeyIdeasAsNoTracking(Guid recapVersionId);
        void DetachEntity<T>(T entity) where T : class;
    }
}
