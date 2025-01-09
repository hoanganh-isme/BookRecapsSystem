using BusinessObject.Data;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class BaseRepository<TModel> : IBaseRepository<TModel> where TModel : BaseEntity
    {

        protected readonly AppDbContext context;
        public BaseRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(TModel entity)
        {
            await context.Set<TModel>().AddAsync(entity);
        }

        public void Delete(TModel entity)
        {
            context.Set<TModel>().Remove(entity);
        }

        public void SoftDelete(TModel entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            context.Set<TModel>().Update(entity);
        }
        public async Task<TModel> GetByIdAsync(Guid id)
        {
            return await context.Set<TModel>().FindAsync(id);
        }

        public void Update(TModel entity)
        {
            entity.UpdatedAt = DateTime.Now;
            context.Set<TModel>().Update(entity);
        }
        public async Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate)
        {
            return await context.Set<TModel>().AnyAsync(predicate);
        }
        public async Task<TModel> FirstOrDefaultAsync(
    Expression<Func<TModel, bool>> predicate,
    Func<IQueryable<TModel>, IQueryable<TModel>> include = null)
        {
            IQueryable<TModel> query = context.Set<TModel>();

            // Nếu có bao gồm include, áp dụng nó vào query
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<TModel>> GetAllAsync()
        {
            return await context.Set<TModel>().AsNoTracking()
                .OrderByDescending(x=> x.CreatedAt)
                .Where(x => x.IsDeleted == false).ToListAsync();
        }

        public async Task<List<TModel>> GetAllAsync(Expression<Func<TModel, bool>> predicate)
        {
            return await context.Set<TModel>().AsNoTracking().Where(predicate).Where(x => x.IsDeleted == false).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }
        public IQueryable<TModel> QueryWithIncludes(params Expression<Func<TModel, object>>[] includeProperties)
        {
            var query = context.Set<TModel>().AsQueryable();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

    }
}
