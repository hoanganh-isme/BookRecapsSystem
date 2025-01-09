using BusinessObject.Models;
using BusinessObject.ViewModels.ContractAttachments;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IContractAttachmentService
    {
        Task<ApiResponse<ContractAttachment>> CreateContractAttachment(CreateContractAttachment contractAttachment, Stream fileStream, string fileExtension);
        Task<ApiResponse<ContractAttachment>> UpdateContractAttachment(Guid contractAttachmentId, UpdateContractAttachment request, Stream newFileStream, string fileExtension);
        Task<ApiResponse<bool>> DeleteContractAttachment(Guid contractAttachmentId);
        Task<ApiResponse<bool>> SoftDeleteContractAttachment(Guid contractAttachmentId);
        Task<ApiResponse<ContractAttachment>> GetContractAttachmentById(Guid contractAttachmentId);
        Task<ApiResponse<List<ContractAttachment>>> GetAllContractAttachmentsByContractId(Guid contractId);
        Task<ApiResponse<List<ContractAttachment>>> GetAllContractAttachments();
    }
}
