using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Highlight;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class HighlightService : IHighlightService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HighlightService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Tạo Highlight mới
        public async Task<ApiResponse<Highlight>> CreateHighlight(CreateHighlightRequest highlightRequest)
        {
            // Kiểm tra recapVersion
            var recapVersion = await _unitOfWork.RecapVersionRepository.GetByIdAsync(highlightRequest.RecapVersionId);
            if (recapVersion == null)
            {
                return new ApiResponse<Highlight>
                {
                    Succeeded = false,
                    Message = "Recap version not found.",
                    Errors = new[] { "Invalid RecapVersionId." }
                };
            }

            // Map CreateHighlightRequest thành Highlight entity
            var newHighlight = _mapper.Map<Highlight>(highlightRequest);
            newHighlight.UserId = highlightRequest.UserId;

            // Thêm highlight mới vào cơ sở dữ liệu
            await _unitOfWork.HighlightRepository.AddAsync(newHighlight);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<Highlight>
                {
                    Succeeded = false,
                    Message = "Failed to create highlight.",
                    Errors = new[] { "Database error while saving highlight." }
                };
            }

            return new ApiResponse<Highlight>
            {
                Succeeded = true,
                Message = "Highlight created successfully.",
                Data = newHighlight
            };
        }

        // Cập nhật Highlight
        public async Task<ApiResponse<Highlight>> UpdateHighlight(UpdateHighlightRequest highlightRequest, Guid userId)
        {
            // Lấy highlight từ database
            var existingHighlight = await _unitOfWork.HighlightRepository.GetByIdAsync(highlightRequest.Id);
            if (existingHighlight == null)
            {
                return new ApiResponse<Highlight>
                {
                    Succeeded = false,
                    Message = "Highlight not found.",
                    Errors = new[] { "The specified highlight does not exist." }
                };
            }

            // Kiểm tra xem user có phải là người tạo highlight không
            if (existingHighlight.UserId != userId)
            {
                return new ApiResponse<Highlight>
                {
                    Succeeded = false,
                    Message = "Unauthorized: Only the creator can update this highlight."
                };
            }

            // Cập nhật thông tin highlight
            _mapper.Map(highlightRequest, existingHighlight);
            _unitOfWork.HighlightRepository.Update(existingHighlight);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Highlight>
            {
                Succeeded = true,
                Message = "Highlight updated successfully.",
                Data = existingHighlight
            };
        }

        // Xóa Highlight
        public async Task<ApiResponse<bool>> DeleteHighlight(Guid highlightId, Guid userId)
        {
            // Lấy highlight từ database
            var existingHighlight = await _unitOfWork.HighlightRepository.GetByIdAsync(highlightId);
            if (existingHighlight == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Highlight not found.",
                    Errors = new[] { "The specified highlight does not exist." },
                    Data = false
                };
            }

            // Kiểm tra xem user có phải là người tạo highlight không
            if (existingHighlight.UserId != userId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Unauthorized: Only the creator can delete this highlight.",
                    Data = false
                };
            }

            // Xóa highlight
            _unitOfWork.HighlightRepository.Delete(existingHighlight);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Highlight deleted successfully.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteHighlight(Guid highlightId, Guid userId)
        {
            // Lấy highlight từ database
            var existingHighlight = await _unitOfWork.HighlightRepository.GetByIdAsync(highlightId);
            if (existingHighlight == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Highlight not found.",
                    Errors = new[] { "The specified highlight does not exist." },
                    Data = false
                };
            }

            // Kiểm tra xem user có phải là người tạo highlight không
            if (existingHighlight.UserId != userId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Unauthorized: Only the creator can delete this highlight.",
                    Data = false
                };
            }

            // Xóa highlight
            _unitOfWork.HighlightRepository.SoftDelete(existingHighlight);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Highlight deleted successfully.",
                Data = true
            };
        }

        // Lấy Highlight theo ID
        public async Task<ApiResponse<Highlight>> GetHighlightById(Guid highlightId)
        {
            var highlight = await _unitOfWork.HighlightRepository.GetByIdAsync(highlightId);
            if (highlight == null)
            {
                return new ApiResponse<Highlight>
                {
                    Succeeded = false,
                    Message = "Highlight not found.",
                    Errors = new[] { "The specified highlight does not exist." }
                };
            }

            return new ApiResponse<Highlight>
            {
                Succeeded = true,
                Message = "Highlight retrieved successfully.",
                Data = highlight
            };
        }

        // Lấy tất cả các Highlight theo RecapVersion ID
        public async Task<ApiResponse<List<Highlight>>> GetHighlightByRecapId(Guid recapId, Guid userId)
        {
            var highlights = await _unitOfWork.HighlightRepository.GetHighlightByUserId(recapId,userId);
            if (!highlights.Any())
            {
                return new ApiResponse<List<Highlight>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy highlight nào.",
                    Data = new List<Highlight>()
                };
            }

            return new ApiResponse<List<Highlight>>
            {
                Succeeded = true,
                Message = "Highlights retrieved successfully.",
                Data = highlights
            };
        }

        // Lấy tất cả các Highlight
        public async Task<ApiResponse<List<Highlight>>> GetAllHighlights()
        {
            var highlights = await _unitOfWork.HighlightRepository.GetAllAsync();
            if (!highlights.Any())
            {
                return new ApiResponse<List<Highlight>>
                {
                    Succeeded = false,
                    Message = "No highlights found.",
                    Errors = new[] { "No data available." },
                    Data = new List<Highlight>()
                };
            }

            return new ApiResponse<List<Highlight>>
            {
                Succeeded = true,
                Message = "Highlights retrieved successfully.",
                Data = highlights
            };
        }
    }
}
