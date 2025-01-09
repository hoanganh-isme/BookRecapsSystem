using BusinessObject.Models;
using BusinessObject.ViewModels.KeyIdea;
using Microsoft.AspNetCore.Http;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IKeyIdeaService
    {
        Task<ApiResponse<KeyIdea>> CreatePrepareKeyIdea(PrepareIdea idea, Stream ImageStream, string ImageContentType);
        public Task<ApiResponse<IEnumerable<KeyIdea>>> CreateMultiplePrepareKeyIdeas(IEnumerable<PrepareIdea> ideas, IEnumerable<IFormFile>? imageFiles);
        Task<ApiResponse<KeyIdea>> GetKeyIdeaById(Guid keyIdeaId);

        Task<ApiResponse<List<KeyIdea>>> GetAllKeyIdeas();
        Task<ApiResponse<KeyIdea>> UpdateKeyIdea(Guid id, UpdateKeyIdeaRequest request, Stream imageStream, string imageContentType);
        //public Task<ApiResponse<IEnumerable<KeyIdea>>> UpdateMultipleKeyIdeas(IEnumerable<UpdateKeyIdeaRequest> requests, IEnumerable<IFormFile>? imageFiles);
        // Delete a specific KeyIdea
        Task<ApiResponse<bool>> DeleteKeyIdea(Guid keyIdeaId);
        Task<ApiResponse<bool>> SoftDeleteKeyIdea(Guid keyIdeaId);
    }
}
