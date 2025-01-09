using BusinessObject.Data;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class KeyIdeaRepository : BaseRepository<KeyIdea>, IKeyIdeaRepository
    {
        public KeyIdeaRepository(AppDbContext context) : base(context)
        {
        }
        public IQueryable<KeyIdea> GetKeyIdeasAsNoTracking(Guid recapVersionId)
        {
            return context.Set<KeyIdea>()
                .AsNoTracking()
                .Where(ki => ki.RecapVersionId == recapVersionId);
        }
        public void DetachEntity<T>(T entity) where T : class
        {
            var entry = context.Entry(entity);
            if (entry != null && entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
