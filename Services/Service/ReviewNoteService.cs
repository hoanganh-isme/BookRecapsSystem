using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.ReviewNotes;
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
    public class ReviewNoteService : IReviewNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewNoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ReviewNote>> CreateReviewNote(CreateReviewNoteRequest request, Guid staffId)
        {
            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." }
                };
            }

            // Kiểm tra xem staffId có khớp với StaffId của review không
            if (review.StaffId != staffId)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền thêm ghi chú."
                };
            }

            var reviewNote = _mapper.Map<ReviewNote>(request);
            reviewNote.Review = review;

            await _unitOfWork.ReviewNoteRepository.AddAsync(reviewNote);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Failed to create review note.",
                    Errors = new[] { "Database error while saving review note." }
                };
            }

            return new ApiResponse<ReviewNote>
            {
                Succeeded = true,
                Message = "Tạo ghi chú Review thành công.",
                Data = reviewNote
            };
        }

        public async Task<ApiResponse<ReviewNote>> UpdateReviewNote(UpdateReviewNoteRequest request, Guid staffId)
        {
            var reviewNote = await _unitOfWork.ReviewNoteRepository.GetByIdAsync(request.Id);
            if (reviewNote == null)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Review note not found.",
                    Errors = new[] { "The specified review note does not exist." }
                };
            }

            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewNote.ReviewId);
            if (review == null || review.StaffId != staffId)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền cập nhật ghi chú."
                };
            }

            _mapper.Map(request, reviewNote);
            _unitOfWork.ReviewNoteRepository.Update(reviewNote);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Failed to update review note.",
                    Errors = new[] { "Database error while updating review note." }
                };
            }

            return new ApiResponse<ReviewNote>
            {
                Succeeded = true,
                Message = "Cập nhật ghi chú thành công.",
                Data = reviewNote
            };
        }

        public async Task<ApiResponse<bool>> DeleteReviewNote(Guid reviewNoteId, Guid staffId)
        {
            var reviewNote = await _unitOfWork.ReviewNoteRepository.GetByIdAsync(reviewNoteId);
            if (reviewNote == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Review note not found.",
                    Errors = new[] { "The specified review note does not exist." },
                    Data = false
                };
            }

            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewNote.ReviewId);
            if (review == null || review.StaffId != staffId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền xóa ghi chú.",
                    Data = false
                };
            }

            _unitOfWork.ReviewNoteRepository.Delete(reviewNote);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa ghi chú thất bại.",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa ghi chú thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteReviewNote(Guid reviewNoteId, Guid staffId)
        {
            var reviewNote = await _unitOfWork.ReviewNoteRepository.GetByIdAsync(reviewNoteId);
            if (reviewNote == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Review note not found.",
                    Errors = new[] { "The specified review note does not exist." },
                    Data = false
                };
            }

            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewNote.ReviewId);
            if (review == null || review.StaffId != staffId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền xóa ghi chú.",
                    Data = false
                };
            }

            _unitOfWork.ReviewNoteRepository.SoftDelete(reviewNote);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa ghi chú thất bại.",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa ghi chú thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<ReviewNote>> GetReviewNoteById(Guid reviewNoteId)
        {
            var reviewNote = await _unitOfWork.ReviewNoteRepository.GetByIdAsync(reviewNoteId);
            if (reviewNote == null)
            {
                return new ApiResponse<ReviewNote>
                {
                    Succeeded = false,
                    Message = "Review note not found.",
                    Errors = new[] { "The specified review note does not exist." }
                };
            }

            return new ApiResponse<ReviewNote>
            {
                Succeeded = true,
                Message = "Review note retrieved successfully.",
                Data = reviewNote
            };
        }

        public async Task<ApiResponse<List<ReviewNote>>> GetAllReviewNotes()
        {
            var reviewNotes = await _unitOfWork.ReviewNoteRepository.GetAllAsync();
            if (reviewNotes == null || !reviewNotes.Any())
            {
                return new ApiResponse<List<ReviewNote>>
                {
                    Succeeded = false,
                    Message = "No review notes found.",
                    Data = new List<ReviewNote>()
                };
            }

            return new ApiResponse<List<ReviewNote>>
            {
                Succeeded = true,
                Message = "Review notes retrieved successfully.",
                Data = reviewNotes
            };
        }
        public async Task<ApiResponse<List<ReviewNote>>> GetReviewNoteByReviewId(Guid reviewId)
        {
            // Truy vấn tất cả ReviewNotes liên kết với ReviewId
            var reviewNotes = await _unitOfWork.ReviewNoteRepository.GetAllAsync(rn => rn.ReviewId == reviewId);

            if (reviewNotes == null || !reviewNotes.Any())
            {
                return new ApiResponse<List<ReviewNote>>
                {
                    Succeeded = false,
                    Message = "No review notes found for the specified review.",
                    Data = new List<ReviewNote>()
                };
            }

            return new ApiResponse<List<ReviewNote>>
            {
                Succeeded = true,
                Message = "Review notes retrieved successfully.",
                Data = reviewNotes
            };
        }

    }
}