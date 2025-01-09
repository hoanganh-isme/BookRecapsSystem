using BusinessObject.Data;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Books;
using BusinessObject.ViewModels.Viewtrackings;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        public BookRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksByIdsAsync(ICollection<Guid> bookIds)
        {
            return await context.Books
        .Where(b => bookIds.Contains(b.Id))
        .ToListAsync();
        }
        public async Task<Book?> GetBookByIdWithIncludesAsync(Guid bookId)
        {
            return await context.Books
                .Where(b => b.Id == bookId)
                .Select(book => new Book
                {
                    Id = book.Id,
                    Title = book.Title,
                    OriginalTitle = book.OriginalTitle,
                    Description = book.Description,
                    PublicationYear = book.PublicationYear,
                    CoverImage = book.CoverImage,
                    AgeLimit = book.AgeLimit,
                    ISBN_10 = book.ISBN_10,
                    ISBN_13 = book.ISBN_13,
                    PublisherId = book.PublisherId,
                    Publisher = book.Publisher,
                    Authors = book.Authors ?? new List<Author>(),
                    Categories = book.Categories ?? new List<Category>(),
                    Recaps = book.Recaps
                        .Where(recap => recap.CurrentVersionId != null)
                        .Select(recap => new Recap
                        {
                            Id = recap.Id,
                            Name = recap.Name,
                            isPublished = recap.isPublished,
                            isPremium = recap.isPremium,
                            UserId = recap.UserId,
                            LikesCount = recap.LikesCount,
                            ViewsCount = recap.ViewsCount,
                            CurrentVersionId = recap.CurrentVersionId,
                            CurrentVersion = recap.CurrentVersion,
                            Contributor = recap.Contributor != null
                                ? new User
                                {
                                    Id = recap.Contributor.Id,
                                    FullName = recap.Contributor.FullName,
                                    ImageUrl = recap.Contributor.ImageUrl
                                }
                                : null,
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<Book?> GetBookByIdNotPublishedAsync(Guid bookId)
        {
            return await context.Books
                .Where(b => b.Id == bookId)
                .Select(book => new Book
                {
                    Id = book.Id,
                    Title = book.Title,
                    OriginalTitle = book.OriginalTitle,
                    Description = book.Description,
                    PublicationYear = book.PublicationYear,
                    CoverImage = book.CoverImage,
                    AgeLimit = book.AgeLimit,
                    ISBN_10 = book.ISBN_10,
                    ISBN_13 = book.ISBN_13,
                    PublisherId = book.PublisherId,
                    Publisher = book.Publisher,
                    Authors = book.Authors ?? new List<Author>(),
                    Categories = book.Categories ?? new List<Category>(),
                    Recaps = book.Recaps
                        .Where(recap => recap.isPublished && recap.CurrentVersionId != null)
                        .Select(recap => new Recap
                        {
                            Id = recap.Id,
                            Name = recap.Name,
                            isPublished = recap.isPublished,
                            isPremium = recap.isPremium,
                            UserId = recap.UserId,
                            LikesCount = recap.LikesCount,
                            ViewsCount = recap.ViewsCount,
                            CurrentVersionId = recap.CurrentVersionId,
                            CurrentVersion = recap.CurrentVersion,
                            Contributor = recap.Contributor != null
                                ? new User
                                {
                                    Id = recap.Contributor.Id,
                                    FullName = recap.Contributor.FullName,
                                    ImageUrl = recap.Contributor.ImageUrl
                                }
                                : null,
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<Book>> GetBooksByPublisherIdAsync(Guid publisherId)
        {
            // Lấy danh sách các sách của publisherId
            var books = await context.Books
                .Where(b => b.PublisherId == publisherId)
                .Include(b => b.Publisher) // Bao gồm thông tin Publisher
                .Include(b => b.Recaps)   // Bao gồm thông tin các Recaps của Book
                .ToListAsync();

            // Lấy tất cả các Recap của những sách trên
            var bookIds = books.Select(b => b.Id).ToList();
            var recaps = await context.Recaps
                .Where(r => bookIds.Contains(r.BookId)) // Bao gồm thông tin Contributor
                .ToListAsync();

            // Lấy ViewTracking dựa trên các Recap
            var recapIds = recaps.Select(r => r.Id).ToList();
            var viewTrackings = await context.ViewTrackings
                .Where(vt => recapIds.Contains(vt.RecapId))
                .ToListAsync();

            // Áp dụng ViewTracking vào Recaps, sau đó gán lại cho Books
            foreach (var recap in recaps)
            {
                recap.ViewTrackings = viewTrackings.Where(vt => vt.RecapId == recap.Id).ToList();
            }

            foreach (var book in books)
            {
                book.Recaps = recaps.Where(r => r.BookId == book.Id).ToList();
            }

            return books;
        }
        public async Task<List<BooksDTO>> GetBooksWithApprovedContractsAsync()
        {
            var books = await context.Books
                .Where(b => b.Contracts.Any(c => c.Status == ContractStatus.Approved))
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .ToListAsync();

            var bookDtos = books.Select(b => new BooksDTO
            {
                Id = b.Id,
                Title = b.Title,
                OriginalTitle = b.OriginalTitle,
                Description = b.Description,
                PublicationYear = b.PublicationYear,
                CoverImage = b.CoverImage,
                AgeLimit = b.AgeLimit,
                ISBN_10 = b.ISBN_10,
                ISBN_13 = b.ISBN_13,
                PublisherName = b.Publisher?.PublisherName,  // Giả sử Publisher có thuộc tính Name
                AuthorNames = b.Authors.Select(a => a.Name).ToList(),  // Giả sử Author có thuộc tính Name
                CategoryNames = b.Categories.Select(c => c.Name).ToList()  // Giả sử Category có thuộc tính Name
            }).ToList();

            return bookDtos;
        }
        public async Task<List<Book>> GetBooksWithFiltersAsync()
        {
            var query = context.Books.AsQueryable();

            // Phân trang: sử dụng Skip và Take để phân trang
            var books = await query
                .Where(b => b.Contracts.Any(c => c.Status == ContractStatus.Approved))
                .Include(b => b.Authors)  // Giả sử bạn cần lấy các tác giả liên quan
                .Include(b => b.Categories) // Bao gồm các thể loại
                .Include(b => b.Contracts) // Bao gồm các hợp đồng
                .Include(b => b.Recaps)
                .Include(b => b.Publisher)
                .ToListAsync();

            return books;
        }


        public async Task<List<BooksDTO>> GetBooksWithNoApprovedContractsAsync()
        {
            var books = await context.Books
                .Where(b => !b.Contracts.Any(c => c.Status == ContractStatus.Approved || c.Status == ContractStatus.NotStarted))  // Lọc sách không có hợp đồng đã phê duyệt
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .ToListAsync();

            var bookDtos = books.Select(b => new BooksDTO
            {
                Id = b.Id,
                Title = b.Title,
                OriginalTitle = b.OriginalTitle,
                Description = b.Description,
                PublicationYear = b.PublicationYear,
                CoverImage = b.CoverImage,
                AgeLimit = b.AgeLimit,
                ISBN_10 = b.ISBN_10,
                ISBN_13 = b.ISBN_13,
                PublisherName = b.Publisher?.PublisherName,  // Giả sử Publisher có thuộc tính PublisherName
                AuthorNames = b.Authors.Select(a => a.Name).ToList(),  // Giả sử Author có thuộc tính Name
                CategoryNames = b.Categories.Select(c => c.Name).ToList()  // Giả sử Category có thuộc tính Name
            }).ToList();

            return bookDtos;
        }




    }
}
