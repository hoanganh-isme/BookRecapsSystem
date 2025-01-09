using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Contracts;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Contract>> CreateContract(CreateContract contract)
        {
            contract.Status = ContractStatus.Draft;
            if (contract.StartDate >= contract.EndDate)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Ngày kết thúc phải lớn hơn ngày bắt đầu.",
                    Errors = new[] { "Invalid StartDate." }
                };
            }

            // Nếu EndDate không có, tự động tính bằng StartDate + 365 ngày (1 năm)
            if (!contract.EndDate.HasValue && contract.StartDate.HasValue)
            {
                contract.EndDate = contract.StartDate.Value.AddYears(1);
            }

            // Map to Contract entity and save to database
            var newContract = _mapper.Map<Contract>(contract);
            await _unitOfWork.ContractRepository.AddAsync(newContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Message = saveResult ? "Tạo hợp đồng thành công." : "Tạo hợp đồng thất bại.",
                Data = saveResult ? newContract : null
            };
        }
        public async Task<ApiResponse<Contract>> CreatePrepareContract(CreatePrepareContract contract)
        {

            contract.Status = ContractStatus.Draft;
            var newContract = _mapper.Map<Contract>(contract);
            await _unitOfWork.ContractRepository.AddAsync(newContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Message = saveResult ? "Tạo hợp đồng thành công." : "Tạo hợp đồng thất bại.",
                Data = saveResult ? newContract : null
            };
        }



        public async Task<ApiResponse<Contract>> UpdateContract(Guid id, UpdateContract contract)
        {
            var existingContract = await _unitOfWork.ContractRepository.GetByIdAsync(id);
            if (existingContract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Contract not found."
                };
            }
            if (contract.StartDate >= contract.EndDate)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Ngày kết thúc phải lớn hơn ngày bắt đầu.",
                    Errors = new[] { "Invalid StartDate." }
                };
            }
            // Nếu StartDate được cung cấp mà EndDate không được cung cấp, tính toán EndDate = StartDate + 1 năm
            if (contract.StartDate.HasValue && !contract.EndDate.HasValue)
            {
                contract.EndDate = contract.StartDate.Value.AddYears(1);
            }

            // Ánh xạ giá trị cập nhật từ `contract` vào `existingContract`
            _mapper.Map(contract, existingContract);

            // Cập nhật lại hợp đồng trong cơ sở dữ liệu
            _unitOfWork.ContractRepository.Update(existingContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Data = existingContract,
                Message = saveResult ? "Cập nhật hợp đồng thành công." : "Cập nhật hợp đồng thất bại."
            };
        }
        public async Task AutoUpdateContractsAsync()
        {
            var contracts = await _unitOfWork.ContractRepository.GetAllAsync();

            foreach (var contract in contracts)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                if (contract.Status == ContractStatus.NotStarted && contract.StartDate <= today)
                {
                    // Chuyển từ NotStarted sang Approved
                    contract.Status = ContractStatus.Approved;
                    _unitOfWork.ContractRepository.Update(contract);
                }
                else if (contract.Status == ContractStatus.Approved && contract.EndDate <= today)
                {
                    if (contract.AutoRenew)
                    {
                        // Gia hạn hợp đồng
                        contract.StartDate = contract.EndDate.Value.AddDays(1);
                        contract.EndDate = contract.StartDate.Value.AddYears(1);
                        contract.Status = ContractStatus.Approved; // Đảm bảo trạng thái là Approved
                        _unitOfWork.ContractRepository.Update(contract);
                    }
                    else
                    {
                        // Chuyển trạng thái sang Expired
                        contract.Status = ContractStatus.Expired;
                        _unitOfWork.ContractRepository.Update(contract);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<Contract>> ChangeStatusContract(Guid id, ChangeStatusContract contract)
        {
            var existingContract = await _unitOfWork.ContractRepository.GetByIdAsync(id);
            if (existingContract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng."
                };
            }

            if (contract.Status == ContractStatus.Approved)
            {
                 if (existingContract.StartDate > DateOnly.FromDateTime(DateTime.Now))
                    {
                    existingContract.Status = ContractStatus.NotStarted;
                    }
                else
                {
                    existingContract.Status = contract.Status;
                }           
            }
            else if (contract.Status == ContractStatus.Expired)
            {
                if (existingContract.AutoRenew && existingContract.EndDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    existingContract.StartDate = existingContract.EndDate.Value.AddDays(1);
                    existingContract.EndDate = existingContract.StartDate.Value.AddYears(1);
                    existingContract.Status = ContractStatus.Approved;
                }
                else
                {
                    existingContract.Status = contract.Status;
                }             
            }
            else
            {
                existingContract.Status = contract.Status;
            }

            _unitOfWork.ContractRepository.Update(existingContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Data = existingContract,
                Message = saveResult ? "Cập nhật trạng thái hợp đồng thành công." : "Cập nhật trạng thái hợp đồng thất bại."
            };
        }


        // Tách hàm tự động gia hạn nếu cần dùng riêng
        public async Task<ApiResponse<Contract>> RenewContractIfNeeded(Guid contractId)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "Invalid ContractId." }
                };
            }

            if (contract.AutoRenew && contract.EndDate <= DateOnly.FromDateTime(DateTime.Now))
            {
                contract.StartDate = contract.EndDate.Value.AddDays(1);
                contract.EndDate = contract.StartDate.Value.AddYears(1);

                _unitOfWork.ContractRepository.Update(contract);
                var saveResult = await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<Contract>
                {
                    Succeeded = saveResult,
                    Message = saveResult ? "Tự gia hạn hợp đồng thành công." : "Tự gia hạn hợp đồng thất bại.",
                    Data = saveResult ? contract : null
                };
            }

            return new ApiResponse<Contract>
            {
                Succeeded = true,
                Message = "No renewal needed.",
                Data = contract
            };
        }



        public async Task<ApiResponse<bool>> DeleteContract(Guid contractId)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "Invalid ContractId." },
                    Data = false
                };
            }
            _unitOfWork.ContractRepository.Delete(contract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = saveResult,
                Message = saveResult ? "Xóa hợp đồng thành công." : "Xóa hợp đồng thấy bại.",
                Data = saveResult
            };
        }

        public async Task<ApiResponse<bool>> SoftDeleteContract(Guid contractId)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "Invalid ContractId." },
                    Data = false
                };
            }

            _unitOfWork.ContractRepository.SoftDelete(contract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = saveResult,
                Message = saveResult ? "Xóa hợp đồng thành công." : "Xóa hợp đồng thấy bại.",
                Data = saveResult
            };
        }

        public async Task<ApiResponse<Contract>> GetContractById(Guid contractId)
        {
            var contract = await _unitOfWork.ContractRepository
                .QueryWithIncludes(c => c.ContractAttachments, c => c.Publisher, c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == contractId);
            if (contract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "Invalid ContractId." }
                };
            }

            return new ApiResponse<Contract>
            {
                Succeeded = true,
                Data = contract,
                Message = "Lấy dữ liệu hợp đồng thành công."
            };
        }

        public async Task<ApiResponse<List<Contract>>> GetContractByPublisherId(Guid publisherId)
        {
            var contracts = await _unitOfWork.ContractRepository
                .QueryWithIncludes(x => x.ContractAttachments,
                                        x => x.Books)
                .Where(c => c.PublisherId == publisherId)
                .ToListAsync();
            if (!contracts.Any())
            {
                return new ApiResponse<List<Contract>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "No contracts associated with this PublisherId." }
                };
            }

            return new ApiResponse<List<Contract>>
            {
                Succeeded = true,
                Data = contracts,
                Message = "Lấy dữ liệu hợp đồng thành công."
            };
        }

        public async Task<ApiResponse<List<Contract>>> GetAllContracts()
        {
            var contracts = await _unitOfWork.ContractRepository
                .QueryWithIncludes(c => c.ContractAttachments, c => c.Publisher, c => c.Books)
                .Include(c => c.Books)
                .ToListAsync();
            if (!contracts.Any())
            {
                return new ApiResponse<List<Contract>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "No contract data available." }
                };
            }

            return new ApiResponse<List<Contract>>
            {
                Succeeded = true,
                Data = contracts,
                Message = "Lấy dữ liệu hợp đồng thành công."
            };
        }
        public async Task<ApiResponse<Contract>> AddContractAttachment(Guid id, AddContractAttachment attachment)
        {
            // Lấy hợp đồng hiện tại từ cơ sở dữ liệu
            var existingContract = await _unitOfWork.ContractRepository.GetContractByIdWithAttachment(id);
            if (existingContract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng."
                };
            }

            // Kiểm tra xem có ID nào trong danh sách AttachmentIds không
            if (attachment.AttachmentIds == null || !attachment.AttachmentIds.Any())
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "No attachment IDs provided."
                };
            }

            // Lấy danh sách các ContractAttachment từ cơ sở dữ liệu dựa trên ID
            var attachmentsToAdd = await _unitOfWork.ContractAttachmentRepository.GetByIdsAsync(attachment.AttachmentIds);

            if (attachmentsToAdd == null || !attachmentsToAdd.Any())
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "No valid attachments found for the provided IDs."
                };
            }

            // Thêm các đính kèm vào hợp đồng
            foreach (var att in attachmentsToAdd)
            {
                existingContract.ContractAttachments.Add(att);
            }

            // Cập nhật hợp đồng
            _unitOfWork.ContractRepository.Update(existingContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Data = existingContract,
                Message = saveResult ? "Thêm thông tin hợp đồng thành công." : "Thêm thông tin hợp đồng thất bại."
            };
        }

        public async Task<ApiResponse<Contract>> AddOrUpdateBooksInContract(Guid contractId, AddBookToContract addBookToContract, Guid publisherId)
        {
            // Lấy contract hiện có kèm danh sách sách hiện tại
            var existingContract = await _unitOfWork.ContractRepository.GetContractByIdWithBook(contractId);
            if (existingContract == null)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng."
                };
            }
            if (existingContract.Status == ContractStatus.Approved)
            {
                return new ApiResponse<Contract>
                {
                    Succeeded = false,
                    Message = "Không thể thêm sách vào hợp đồng đã kích hoạt."
                };
            }
            var newBookIds = addBookToContract.BookIds ?? new List<Guid>();
            var existingBookIds = existingContract.Books.Select(b => b.Id).ToList();
            var booksToAddIds = newBookIds.Except(existingBookIds).ToList();
            var booksToAdd = await _unitOfWork.BookRepository.GetBooksByIdsAsync(booksToAddIds);
            var booksToRemove = existingContract.Books.Where(b => !newBookIds.Contains(b.Id)).ToList();
            foreach (var book in booksToAdd)
            {
                existingContract.Books.Add(book);
                book.PublisherId = publisherId;
            }
            foreach (var book in booksToRemove)
            {
                existingContract.Books.Remove(book);
                book.PublisherId = null;
            }
            _unitOfWork.ContractRepository.Update(existingContract);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Contract>
            {
                Succeeded = saveResult,
                Data = existingContract,
                Message = saveResult ? "Cập nhật sách vào hợp đồng thành công." : "Cập nhật sách vào hợp đồng thất bại."
            };
        }
    }
}
