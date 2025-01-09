using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IContractAttachmentRepository : IBaseRepository<ContractAttachment>
    {
        Task<IEnumerable<ContractAttachment>> GetByIdsAsync(ICollection<Guid> attachmentIds);
    }
}
