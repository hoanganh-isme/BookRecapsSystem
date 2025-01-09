using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Review;
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
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Review>> CreateReview(CreateReviewRequest reviewRequest)
        {
            var recapVersion = await _unitOfWork.RecapVersionRepository
    .FirstOrDefaultAsync(
        rv => rv.Id == reviewRequest.RecapVersionId && rv.Status == RecapStatus.Pending,
        include: query => query.Include(rv => rv.Review)
    );
            if (recapVersion == null)
            {
                return new ApiResponse<Review>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Recap hoặc Recap không ở trạng thái chờ.",
                    Errors = new[] { "Invalid RecapVersionId or status." }
                };
            }

            // Step 2: Kiểm tra xem RecapVersion đã có review chưa
            if (recapVersion.Review != null)
            {
                return new ApiResponse<Review>
                {
                    Succeeded = false,
                    Message = "Recap này đã có người Review.",
                    Errors = new[] { "Review already exists." }
                };
            }

            // Step 3: Map dữ liệu từ CreateReviewRequest sang đối tượng Review
            var newReview = _mapper.Map<Review>(reviewRequest);

            // Step 4: Gán RecapVersion và StaffId vào Review
            newReview.RecapVersionId = recapVersion.Id;
            newReview.StaffId = reviewRequest.StaffId;

            // Step 5: Thêm Review mới vào database
            await _unitOfWork.ReviewRepository.AddAsync(newReview);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<Review>
                {
                    Succeeded = false,
                    Message = "Failed to create review.",
                    Errors = new[] { "Database error while saving review." }
                };
            }

            // Step 6: Cập nhật recapVersion với review mới
            recapVersion.Review = newReview;
            _unitOfWork.RecapVersionRepository.Update(recapVersion);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Review>
            {
                Succeeded = true,
                Message = "Tạo Review thành công.",
                Data = newReview
            };

        }
        public async Task<ApiResponse<Review>> UpdateReview(UpdateReviewRequest review, Guid staffId)
        {
            var existingReview = await _unitOfWork.ReviewRepository.GetByIdAsync(review.Id);
            if (existingReview == null)
            {
                return new ApiResponse<Review>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." }
                };
            }

            // Kiểm tra StaffId
            if (existingReview.StaffId != staffId)
            {
                return new ApiResponse<Review>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền cập nhật."
                };
            }

            // Cập nhật thông tin review
            _mapper.Map(review, existingReview);
            _unitOfWork.ReviewRepository.Update(existingReview);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Review>
            {
                Succeeded = true,
                Message = "Cập nhật Review thành công.",
                Data = existingReview
            };
        }

        // Xoá review
        public async Task<ApiResponse<bool>> DeleteReview(Guid reviewId, Guid staffId)
        {
            var existingReview = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
            if (existingReview == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." },
                    Data = false
                };
            }

            if (existingReview.StaffId != staffId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền xóa.",
                    Data = false
                };
            }

            _unitOfWork.ReviewRepository.Delete(existingReview);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa Review thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteReview(Guid reviewId, Guid staffId)
        {
            var existingReview = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
            if (existingReview == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." },
                    Data = false
                };
            }

            if (existingReview.StaffId != staffId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới có quyền xóa.",
                    Data = false
                };
            }

            _unitOfWork.ReviewRepository.SoftDelete(existingReview);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa Review thành công.",
                Data = true
            };
        }

        // Lấy review theo ID
        public async Task<ApiResponse<ReviewDTO>> GetReviewById(Guid reviewId)
        {
            var review = await _unitOfWork.ReviewRepository
                .QueryWithIncludes(x => x.ReviewNotes, x => x.Staff, x => x.RecapVersion)
                .FirstOrDefaultAsync(x => x.Id == reviewId);
            if (review == null)
            {
                return new ApiResponse<ReviewDTO>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." }
                };
            }
            var reviewWithStaffDto = _mapper.Map<ReviewDTO>(review);

            return new ApiResponse<ReviewDTO>
            {
                Succeeded = true,
                Message = "Review retrieved successfully.",
                Data = reviewWithStaffDto
            };
        }
        public async Task<ApiResponse<ReviewDTO>> GetReviewByRecapVersion(Guid recapversionId)
        {
            var review = await _unitOfWork.ReviewRepository
                .QueryWithIncludes(x => x.ReviewNotes, x => x.Staff, x => x.RecapVersion)
                .FirstOrDefaultAsync(x => x.RecapVersionId == recapversionId);
            if (review == null)
            {
                return new ApiResponse<ReviewDTO>
                {
                    Succeeded = false,
                    Message = "Review not found.",
                    Errors = new[] { "The specified review does not exist." }
                };
            }
            var reviewWithStaffDto = _mapper.Map<ReviewDTO>(review);
            return new ApiResponse<ReviewDTO>
            {
                Succeeded = true,
                Message = "Review retrieved successfully.",
                Data = reviewWithStaffDto
            };
        }

        // Lấy các review của Staff theo StaffId
        public async Task<ApiResponse<List<Review>>> GetReviewByStaffId(Guid staffId)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByStaffAsync(staffId);
            if (reviews == null || !reviews.Any())
            {
                return new ApiResponse<List<Review>>
                {
                    Succeeded = false,
                    Message = "No reviews found for this staff.",
                    Data = new List<Review>()
                };
            }

            return new ApiResponse<List<Review>>
            {
                Succeeded = true,
                Message = "Reviews retrieved successfully.",
                Data = reviews
            };
        }

        // Lấy tất cả các review
        public async Task<ApiResponse<List<Review>>> GetAllReviews()
        {
            var reviews = await _unitOfWork.ReviewRepository
                .QueryWithIncludes(x => x.ReviewNotes)
                .ToListAsync();
            if (!reviews.Any())
            {
                return new ApiResponse<List<Review>>
                {
                    Succeeded = false,
                    Message = "No reviews found.",
                    Errors = new[] { "No data available." },
                    Data = new List<Review>()
                };
            }

            return new ApiResponse<List<Review>>
            {
                Succeeded = true,
                Message = "Reviews retrieved successfully.",
                Data = reviews
            };
        }

    }

}
