using BusinessObject.Models;
using BusinessObject.ViewModels.ReviewNotes;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IReviewNoteService
    {
        Task<ApiResponse<ReviewNote>> CreateReviewNote(CreateReviewNoteRequest reviewNote, Guid staffId);
        Task<ApiResponse<ReviewNote>> UpdateReviewNote(UpdateReviewNoteRequest reviewNote, Guid staffId);
        Task<ApiResponse<bool>> DeleteReviewNote(Guid reviewNoteId, Guid staffId);
        Task<ApiResponse<bool>> SoftDeleteReviewNote(Guid reviewNoteId, Guid staffId);
        Task<ApiResponse<ReviewNote>> GetReviewNoteById(Guid Id);
        Task<ApiResponse<List<ReviewNote>>> GetReviewNoteByReviewId(Guid reviewId);
        Task<ApiResponse<List<ReviewNote>>> GetAllReviewNotes();
    }
}
