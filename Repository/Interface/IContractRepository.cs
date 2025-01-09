using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IContractRepository : IBaseRepository<Contract>
    {
        Task<Contract?> GetContractByIdWithBook(Guid contractId);
        Task<Contract?> GetContractByIdWithAttachment(Guid contractId);
        Task<Contract?> GetValidContractByBookIdAsync(Guid bookId);
        Task<List<Contract>> GetValidContractsByBookIdsAsync(List<Guid> bookIds);
    }
}
