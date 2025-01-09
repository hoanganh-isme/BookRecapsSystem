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
    public class ContractAttachmentRepository : BaseRepository<ContractAttachment>, IContractAttachmentRepository
    {
        public ContractAttachmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ContractAttachment>> GetByIdsAsync(ICollection<Guid> attachmentIds)
        {
            return await context.ContractAttachments
        .Where(b => attachmentIds.Contains(b.Id))
        .ToListAsync();
        }
    }
}
