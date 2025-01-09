using Azure;
using BusinessObject.Models;
using BusinessObject.ViewModels.Books;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBookService
    {
        Task<ApiResponse<Book>> CreateBook(BookCreateRequest bookRequest, Stream coverImageStream, string coverImageContentType, Dictionary<Guid, Stream> authorImageStreams);
        Task<ApiResponse<Book>> UpdateBook(BookUpdateRequest bookRequest, Stream coverImageStream, string coverImageContentType);
        Task<ApiResponse<bool>> DeleteBook(Guid bookId);
        Task<ApiResponse<bool>> SoftDeleteBook(Guid bookId);
        Task<ApiResponse<Book>> GetBookById(Guid bookId);
        Task<ApiResponse<Book>> GetBookByIdRecapNotPublished(Guid bookId);
        Task<ApiResponse<List<Book>>> GetAllBooks();
        Task<GoogleBooksResponse> GetBookInfoAsync(string query);
        Task<ApiResponse<List<BooksDTO>>> GetAllBooksWithApprovedContractsAsync();
        Task<ApiResponse<List<BooksDTO>>> GetAllBooksNoApprovedContractsAsync();
        Task<List<BookContributorDTO>> GetBooksWithFiltersAsync();
    }
}
