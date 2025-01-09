using BusinessObject.Models;
using BusinessObject.ViewModels.Contracts;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IContractService
    {
        Task<ApiResponse<Contract>> CreateContract(CreateContract contract);
        Task<ApiResponse<Contract>> CreatePrepareContract(CreatePrepareContract contract);
        Task<ApiResponse<Contract>> UpdateContract(Guid id, UpdateContract contract);
        Task<ApiResponse<Contract>> ChangeStatusContract(Guid id, ChangeStatusContract contract);
        Task<ApiResponse<Contract>> AddContractAttachment(Guid id, AddContractAttachment attachment);
        Task<ApiResponse<Contract>> AddOrUpdateBooksInContract(Guid contractId, AddBookToContract book, Guid publisherId);
        Task<ApiResponse<Contract>> RenewContractIfNeeded(Guid contractId);
        Task<ApiResponse<bool>> DeleteContract(Guid contractId);
        Task<ApiResponse<bool>> SoftDeleteContract(Guid contractId);
        Task<ApiResponse<Contract>> GetContractById(Guid contractId);
        Task<ApiResponse<List<Contract>>> GetContractByPublisherId(Guid publisherId);
        Task<ApiResponse<List<Contract>>> GetAllContracts();
        Task AutoUpdateContractsAsync();
    }
}
