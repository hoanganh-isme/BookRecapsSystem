using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.ContractAttachments;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using Services.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ContractAttachmentService : IContractAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GoogleCloudService _googleCloudService;

        public ContractAttachmentService(IUnitOfWork unitOfWork, 
            IMapper mapper, 
            GoogleCloudService googleCloudService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleCloudService = googleCloudService;
        }

        public async Task<ApiResponse<ContractAttachment>> CreateContractAttachment(CreateContractAttachment contractAttachment, Stream fileStream, string fileExtension)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractAttachment.ContractId);
            if ( contract == null)
            {
                return new ApiResponse<ContractAttachment>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "Invalid contractId." }
                };
            }
            if ( contract.Status == ContractStatus.Draft)
            {
                try
                {
                    // Tạo tên file động dựa trên thông tin hợp đồng
                    string folderName = "contract_attachments/";
                    string newFileName = $"{folderName}attachment_{Guid.NewGuid()}{fileExtension}";

                    // Upload file lên cloud và lấy URL public của file
                    string publicFileUrl = await _googleCloudService.UploadImageAsync(newFileName, fileStream, fileExtension);

                    // Gán URL của file upload vào AttachmentURL của contractAttachment
                    var newContractAttachment = _mapper.Map<ContractAttachment>(contractAttachment);
                    newContractAttachment.AttachmentURL = publicFileUrl;

                    // Thêm contract attachment vào repository
                    await _unitOfWork.ContractAttachmentRepository.AddAsync(newContractAttachment);
                    await _unitOfWork.SaveChangesAsync();

                    return new ApiResponse<ContractAttachment>
                    {
                        Succeeded = true,
                        Message = "Tạo thông tin hợp đồng thành công.",
                        Data = newContractAttachment
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating contract attachment: {ex.Message}");
                    return new ApiResponse<ContractAttachment>
                    {
                        Succeeded = false,
                        Message = "Tạo thông tin hợp đồng thất bại.",
                        Errors = new[] { ex.Message }
                    };
                }
            }
            else
            {
                return new ApiResponse<ContractAttachment>
                {
                    Succeeded = false,
                    Message = "Trạng thái hợp đồng phải là bản nháp."
                };
            }
            
        }

        public async Task<ApiResponse<ContractAttachment>> UpdateContractAttachment(Guid contractAttachmentId, UpdateContractAttachment request, Stream newFileStream, string fileExtension)
        {
            // Tìm ContractAttachment hiện có theo ID
            var contractAttachment = await _unitOfWork.ContractAttachmentRepository.GetByIdAsync(contractAttachmentId);
            if (contractAttachment == null)
            {
                return new ApiResponse<ContractAttachment>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "The specified contract attachment does not exist." }
                };
            }

            // Xóa file cũ trên cloud nếu có URL
            if (!string.IsNullOrEmpty(contractAttachment.AttachmentURL))
            {
                Uri oldUri = new Uri(contractAttachment.AttachmentURL);
                string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                await _googleCloudService.DeleteFileAsync(oldObjectName); // Xóa file cũ
            }

            // Upload file mới lên cloud và lấy URL mới
            string folderName = "contract_attachments/";
            string newObjectName = $"{folderName}attachment_{Guid.NewGuid()}{fileExtension}";
            string newAttachmentUrl = await _googleCloudService.UploadImageAsync(newObjectName, newFileStream, fileExtension);

            // Cập nhật thông tin ContractAttachment với URL mới
            contractAttachment.AttachmentURL = newAttachmentUrl;
            _mapper.Map(request, contractAttachment);

            // Cập nhật dữ liệu trong cơ sở dữ liệu
            _unitOfWork.ContractAttachmentRepository.Update(contractAttachment);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<ContractAttachment>
            {
                Succeeded = true,
                Message = "Cập nhật thông tin hợp đồng thành công",
                Data = contractAttachment
            };
        }


        public async Task<ApiResponse<bool>> DeleteContractAttachment(Guid contractAttachmentId)
        {
            var contractAttachment = await _unitOfWork.ContractAttachmentRepository.GetByIdAsync(contractAttachmentId);
            if (contractAttachment == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng",
                    Errors = new[] { "The specified contract attachment does not exist." }
                };
            }

            _unitOfWork.ContractAttachmentRepository.Delete(contractAttachment);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa thông tin hợp đồng thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> SoftDeleteContractAttachment(Guid contractAttachmentId)
        {
            var contractAttachment = await _unitOfWork.ContractAttachmentRepository.GetByIdAsync(contractAttachmentId);
            if (contractAttachment == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "The specified contract attachment does not exist." }
                };
            }

            _unitOfWork.ContractAttachmentRepository.SoftDelete(contractAttachment);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa thông tin hợp đồng thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<ContractAttachment>> GetContractAttachmentById(Guid contractAttachmentId)
        {
            var contractAttachment = await _unitOfWork.ContractAttachmentRepository
                .QueryWithIncludes(c => c.Contract).FirstOrDefaultAsync(c => c.Id == contractAttachmentId);
            if (contractAttachment == null)
            {
                return new ApiResponse<ContractAttachment>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "The specified contract attachment does not exist." }
                };
            }

            return new ApiResponse<ContractAttachment>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = contractAttachment
            };
        }

        public async Task<ApiResponse<List<ContractAttachment>>> GetAllContractAttachmentsByContractId(Guid contractId)
        {
            var contractAttachments = await _unitOfWork.ContractAttachmentRepository
                .QueryWithIncludes(c => c.Contract)
                .Where(c => c.ContractId == contractId)
                .ToListAsync();
            if (!contractAttachments.Any())
            {
                return new ApiResponse<List<ContractAttachment>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "No data available." }
                };
            }

            return new ApiResponse<List<ContractAttachment>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = contractAttachments
            };
        }
        public async Task<ApiResponse<List<ContractAttachment>>> GetAllContractAttachments()
        {
            var contractAttachments = await _unitOfWork.ContractAttachmentRepository
                .QueryWithIncludes(c => c.Contract).ToListAsync();
            if (!contractAttachments.Any())
            {
                return new ApiResponse<List<ContractAttachment>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng.",
                    Errors = new[] { "No data available." }
                };
            }

            return new ApiResponse<List<ContractAttachment>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = contractAttachments
            };
        }
    }
}
