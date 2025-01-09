using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRecapVersionRepository : IBaseRepository<RecapVersion>
    {
        Task<RecapVersion> GetLatestVersionAsync();
        Task<List<RecapVersion>> GetVersionApprovedbyRecapId(Guid recapId);
        Task<List<RecapVersion>> GetAllRecapVersionNotDraft();
    }
}
