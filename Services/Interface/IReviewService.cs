using BusinessObject.Models;
using BusinessObject.ViewModels.Review;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IReviewService
    {
        Task<ApiResponse<Review>> CreateReview(CreateReviewRequest review);
        Task<ApiResponse<Review>> UpdateReview(UpdateReviewRequest review, Guid staffId);
        Task<ApiResponse<bool>> DeleteReview(Guid reviewId, Guid staffId);
        Task<ApiResponse<ReviewDTO>> GetReviewByRecapVersion(Guid recapversionId);
        Task<ApiResponse<bool>> SoftDeleteReview(Guid reviewId, Guid staffId);
        Task<ApiResponse<ReviewDTO>> GetReviewById(Guid reviewId);
        Task<ApiResponse<List<Review>>> GetReviewByStaffId(Guid staffId);
        Task<ApiResponse<List<Review>>> GetAllReviews();
    }
}
