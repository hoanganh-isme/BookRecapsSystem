using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBaseRepository<TModel>
    {
        Task<TModel> GetByIdAsync(Guid id);
        Task AddAsync(TModel entity);
        void Update(TModel entity);
        void Delete(TModel entity);
        void SoftDelete(TModel entity);
        //Task<Pagination<TModel>> GetPageAsync(int pageIndex, int pageSize);
        Task<List<TModel>> GetAllAsync();
        Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate);
        public Task<TModel> FirstOrDefaultAsync(
    Expression<Func<TModel, bool>> predicate,
    Func<IQueryable<TModel>, IQueryable<TModel>> include = null);
        Task<List<TModel>> GetAllAsync(Expression<Func<TModel, bool>> predicate);
        IQueryable<TModel> QueryWithIncludes(params Expression<Func<TModel, object>>[] includeProperties);    }
}
