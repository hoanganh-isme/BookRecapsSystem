using BusinessObject.Models;
using BusinessObject.ViewModels.Author;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAuthorService
    {
        Task<ApiResponse<Author>> CreateAuthor(AuthorCreateRequest author, Stream? ImageStream, string? ImageContentType);
        Task<ApiResponse<Author>> UpdateAuthor(AuthorUpdateRequest author, Stream? ImageStream, string? ImageContentType);
        Task<ApiResponse<bool>> DeleteAuthor(Guid authorId);
        Task<ApiResponse<bool>> SoftDeleteAuthor(Guid authorId);
        Task<ApiResponse<Author>> GetAuthorById(Guid authorId);
        Task<ApiResponse<List<Author>>> GetAllAuthors();
    }
}
