using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IAppealRepository : IBaseRepository<Appeal>
    {
        Task<List<Appeal>> GetAppealsByStaffAsync(Guid staffId);
    }
}
