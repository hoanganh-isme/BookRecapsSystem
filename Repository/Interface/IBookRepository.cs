using BusinessObject.Models;
using BusinessObject.ViewModels.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBookRepository : IBaseRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByIdsAsync(ICollection<Guid> bookIds);
        Task<Book?> GetBookByIdWithIncludesAsync(Guid bookId);
        Task<Book?> GetBookByIdNotPublishedAsync(Guid bookId);
        Task<List<Book>> GetBooksByPublisherIdAsync(Guid publisherId);
        Task<List<BooksDTO>> GetBooksWithApprovedContractsAsync();
        Task<List<BooksDTO>> GetBooksWithNoApprovedContractsAsync();
        Task<List<Book>> GetBooksWithFiltersAsync();
    }
}
