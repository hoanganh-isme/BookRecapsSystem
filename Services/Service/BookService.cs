using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Author;
using BusinessObject.ViewModels.Books;
using Google.Apis.Services;
using Google.Apis.Books.v1;
using Microsoft.Extensions.Options;
using Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Services.Service.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BusinessObject.ViewModels.Viewtrackings;
using Newtonsoft.Json;
using Services.Responses;
using BusinessObject.Enums;
using Google.Api.Gax.Grpc;

namespace Services.Service
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly GoogleCloudService _googleCloudService;
        public BookService(HttpClient httpClient, 
            IUnitOfWork unitOfWork,
            IMapper mapper, 
            IOptions<GoogleSettings> googleSettings,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _apiKey = googleSettings.Value.ApiKey;
            _httpClient = httpClient;
            _googleCloudService = new GoogleCloudService(configuration);
        }
        public async Task<GoogleBooksResponse> GetBookInfoAsync(string query)
        {
            var url = $"https://www.googleapis.com/books/v1/volumes?q={query}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleBooksResponse>(jsonContent);
        }


        public async Task<ApiResponse<Book>> CreateBook(BookCreateRequest bookRequest, Stream coverImageStream, string coverImageContentType, Dictionary<Guid, Stream> authorImageStreams)
        {
            var errors = new List<string>();

            // Kiểm tra thể loại tồn tại
            if (bookRequest.CategoryIds == null || !bookRequest.CategoryIds.Any())
            {
                errors.Add("Chọn ít nhất 1 thể loại.");
            }
            else
            {
                var invalidCategoryIds = new List<Guid>();
                foreach (var categoryId in bookRequest.CategoryIds)
                {
                    var categoryExists = await _unitOfWork.CategoryRepository.AnyAsync(c => c.Id == categoryId);
                    if (!categoryExists)
                    {
                        invalidCategoryIds.Add(categoryId);
                    }
                }

                if (invalidCategoryIds.Any())
                {
                    errors.Add($"Thể loại không tồn tại: {string.Join(", ", invalidCategoryIds)}");
                }
            }

            // Kiểm tra thông tin tác giả
            if (bookRequest.Authors == null || !bookRequest.Authors.Any(a => a.Id.HasValue || !string.IsNullOrWhiteSpace(a.Name)))
            {
                errors.Add("Cần ít nhất 1 tác giả hợp lệ (có ID hoặc Tên).");
            }
            else
            {
                foreach (var authorRequest in bookRequest.Authors)
                {
                    if (authorRequest.Id.HasValue && !await _unitOfWork.AuthorRepository.AnyAsync(a => a.Id == authorRequest.Id.Value))
                    {
                        errors.Add($"Tác giả không tồn tại: {authorRequest.Id}");
                    }
                }
            }

            // Xử lý danh sách tác giả
            var authors = new List<Author>();
            foreach (var authorRequest in bookRequest.Authors)
            {
                Author? author = null;

                if (authorRequest.Id.HasValue)
                {
                    // Tìm tác giả đã có trong DB
                    author = await _unitOfWork.AuthorRepository.GetByIdAsync(authorRequest.Id.Value);
                }
                else if ( author == null && !string.IsNullOrWhiteSpace(authorRequest.Name))
                {
                    // Tạo tác giả mới nếu không có ID
                    
                        // Tạo tác giả mới nếu không có ID
                        string? authorImageUrl = null;

                        // Kiểm tra nếu GUID ảnh đã được gửi từ frontend
                        var authorGuid = Guid.TryParse(authorRequest.Image, out var parsedGuid) ? parsedGuid : Guid.Empty; // Lấy GUID từ request

                        if (authorGuid != Guid.Empty && authorImageStreams.ContainsKey(authorGuid))
                        {
                            // Lưu ảnh tác giả lên Google Cloud (hoặc nơi bạn muốn lưu ảnh)
                            string authorFolderName = "author_images/";
                            string authorImageFileName = $"{authorFolderName}author_{Guid.NewGuid()}.jpg";
                            var authorImageStream = authorImageStreams[authorGuid]; // Lấy ảnh từ dictionary theo GUID
                            authorImageUrl = await _googleCloudService.UploadImageAsync(authorImageFileName, authorImageStream, "image/jpeg");
                        }

                        var newAuthor = new Author
                        {
                            Name = authorRequest.Name,
                            Description = authorRequest.Description,
                            Image = authorImageUrl,
                        };

                        await _unitOfWork.AuthorRepository.AddAsync(newAuthor);
                        await _unitOfWork.SaveChangesAsync();

                        author = newAuthor;
                }

                if (author != null)
                {
                    authors.Add(author);
                }
            }

            // Xử lý upload ảnh bìa nếu có
            if (coverImageStream != null && !string.IsNullOrEmpty(coverImageContentType))
            {
                string bookFolderName = $"book_covers/";
                string coverImageFileName = $"{bookFolderName}bookcover_{Guid.NewGuid()}.jpg";
                var coverImageUrl = await _googleCloudService.UploadImageAsync(coverImageFileName, coverImageStream, coverImageContentType);
                bookRequest.CoverImage = coverImageUrl;
            }

            // Tạo sách
            var book = _mapper.Map<Book>(bookRequest);
            book.Authors = authors;

            // Thêm thể loại vào sách
            book.Categories = new List<Category>();
            foreach (var categoryId in bookRequest.CategoryIds)
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
                book.Categories.Add(category);
            }

            await _unitOfWork.BookRepository.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Book>
            {
                Succeeded = true,
                Message = "Tạo sách thành công.",
                Data = book
            };
        }




        public async Task<ApiResponse<bool>> DeleteBook(Guid bookId)
        {
            
            var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy sách.",
                    Errors = new[] { "The specified book does not exist." },
                    Data = false
                };
            }

            _unitOfWork.BookRepository.Delete(book);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa sách thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteBook(Guid bookId)
        {

            var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy sách.",
                    Errors = new[] { "The specified book does not exist." },
                    Data = false
                };
            }

            _unitOfWork.BookRepository.SoftDelete(book);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa sách thành công.",
                Data = true
            };
        }
        // Lấy tất cả các sách
        public async Task<ApiResponse<List<Book>>> GetAllBooks()
        {
            try
            {
                // Lấy tất cả các sách kèm theo thông tin chi tiết
                var books = await _unitOfWork.BookRepository
                    .QueryWithIncludes(b => b.Authors, b => b.Categories, b => b.Recaps,
                                      b => b.BookEarnings, b => b.Contracts)
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
                        Publisher = book.Publisher, // Có thể là null nếu không có nhà xuất bản
                        Authors = book.Authors ?? new List<Author>(), // Khởi tạo danh sách rỗng nếu null
                        Categories = book.Categories ?? new List<Category>(), // Khởi tạo danh sách rỗng nếu null
                        Recaps = book.Recaps ?? new List<Recap>(), // Khởi tạo danh sách rỗng nếu null
                        BookEarnings = book.BookEarnings ?? new List<BookEarning>(), // Khởi tạo danh sách rỗng nếu null
                        Contracts = book.Contracts ?? new List<Contract>() // Khởi tạo danh sách rỗng nếu null
                    })
                    .ToListAsync();

                if (!books.Any())
                {
                    return new ApiResponse<List<Book>>
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new[] { "No data available." },
                        Data = new List<Book>()
                    };
                }

                return new ApiResponse<List<Book>>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = books
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Book>>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message }
                };
            }
        }      

        // Lấy thông tin sách theo ID
        public async Task<ApiResponse<Book>> GetBookById(Guid bookId)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetBookByIdWithIncludesAsync(bookId);

                if (book == null)
                {
                    return new ApiResponse<Book>
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new[] { "The specified book does not exist." }
                    };
                }

                return new ApiResponse<Book>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = book
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Book>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message }
                };
            }
        }
        public async Task<ApiResponse<Book>> GetBookByIdRecapNotPublished(Guid bookId)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetBookByIdNotPublishedAsync(bookId);

                if (book == null)
                {
                    return new ApiResponse<Book>
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new[] { "The specified book does not exist." }
                    };
                }

                return new ApiResponse<Book>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = book
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Book>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message }
                };
            }
        }




        public async Task<ApiResponse<Book>> UpdateBook(BookUpdateRequest bookRequest, Stream coverImageStream, string coverImageContentType)
        {
            var errors = new List<string>();

            // Tìm kiếm sách hiện tại trong cơ sở dữ liệu
            var existingBook = await _unitOfWork.BookRepository.GetByIdAsync(bookRequest.Id);

            // Kiểm tra nếu sách không tồn tại
            if (existingBook == null)
            {
                return new ApiResponse<Book>
                {
                    Succeeded = false,
                    Message = $"Không tìm thấy sách.",
                    Errors = new[] { "Book not found." },
                    Data = null
                };
            }

            // Kiểm tra thể loại tồn tại và cập nhật danh sách thể loại cho sách
            if (bookRequest.CategoryIds != null && bookRequest.CategoryIds.Any())
            {
                var newCategories = new List<Category>();
                foreach (var categoryId in bookRequest.CategoryIds)
                {
                    var categoryExists = await _unitOfWork.CategoryRepository.AnyAsync(c => c.Id == categoryId);
                    if (!categoryExists)
                    {
                        errors.Add($"Không tìm thấy thể loại.");
                    }
                    else
                    {
                        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
                        newCategories.Add(category);
                    }
                }

                if (!errors.Any())
                {
                    existingBook.Categories = newCategories; // Cập nhật thể loại nếu không có lỗi
                }
            }

            // Trả về lỗi nếu có
            if (errors.Any())
            {
                return new ApiResponse<Book>
                {
                    Succeeded = false,
                    Message = "Validation failed.",
                    Errors = errors.ToArray()
                };
            }

            // Xử lý upload ảnh bìa nếu có
            if (coverImageStream != null && !string.IsNullOrEmpty(coverImageContentType))
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingBook.CoverImage))
                {
                    // Lấy tên đối tượng từ URL cũ (chỉ phần sau bucket name)
                    Uri oldUri = new Uri(existingBook.CoverImage);
                    string oldObjectName = oldUri.AbsolutePath.TrimStart('/'); // Lấy đường dẫn từ URL

                    // Xóa tệp cũ
                    await _googleCloudService.DeleteFileAsync(oldObjectName);
                }


                // Tạo thư mục động cho ảnh bìa dựa theo BookId
                string bookFolderName = $"book_covers/";
                string coverImageFileName = $"{bookFolderName}bookcover_{Guid.NewGuid()}.jpg";

                var coverImageUrl = await _googleCloudService.UploadImageAsync(coverImageFileName, coverImageStream, coverImageContentType);
                existingBook.CoverImage = coverImageUrl;  // Cập nhật URL ảnh bìa mới
            }

            // Cập nhật các trường khác chỉ khi chúng được cung cấp trong request
            if (!string.IsNullOrWhiteSpace(bookRequest.Title))
            {
                existingBook.Title = bookRequest.Title;
            }

            if (!string.IsNullOrWhiteSpace(bookRequest.OriginalTitle))
            {
                existingBook.OriginalTitle = bookRequest.OriginalTitle;
            }

            if (!string.IsNullOrWhiteSpace(bookRequest.Description))
            {
                existingBook.Description = bookRequest.Description;
            }

            if (bookRequest.PublicationYear != 0)
            {
                existingBook.PublicationYear = bookRequest.PublicationYear;
            }

            if (bookRequest.AgeLimit != 0)
            {
                existingBook.AgeLimit = bookRequest.AgeLimit;
            }

            // Cập nhật dữ liệu vào cơ sở dữ liệu
            _unitOfWork.BookRepository.Update(existingBook);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Book>
            {
                Succeeded = true,
                Message = "Cập nhật sách thành công.",
                Data = existingBook
            };
        }

        public async Task<ApiResponse<List<BooksDTO>>> GetAllBooksWithApprovedContractsAsync()
        {
            try
            {
                var books = await _unitOfWork.BookRepository.GetBooksWithApprovedContractsAsync();

                if (!books.Any())
                {
                    return new ApiResponse<List<BooksDTO>>
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new[] { "No data available." },
                        Data = null
                    };
                }

                return new ApiResponse<List<BooksDTO>>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = books
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BooksDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message },
                    Data = null
                };
            }
        }
        public async Task<List<BookContributorDTO>> GetBooksWithFiltersAsync()
        {
            // Lấy tất cả sách từ repository (không phân trang)
            var books = await _unitOfWork.BookRepository.GetBooksWithFiltersAsync();

            // Lấy danh sách bookIds từ danh sách sách
            var bookIds = books.Select(b => b.Id).ToList();

            // Gọi hàm lấy các hợp đồng hợp lệ theo bookIds
            var validContracts = await _unitOfWork.ContractRepository.GetValidContractsByBookIdsAsync(bookIds);

            // Lấy RevenueSharePercentage từ cài đặt hệ thống
            var revenueSharePercentage = await _unitOfWork.SystemSettingRepository.GetRevenueSharePercentageAsync();

            // Tạo danh sách bookDTOs với tính toán phần trăm lợi nhuận của Contributor
            var bookDtos = books.Select(b =>
            {
                // Lấy hợp đồng hợp lệ cho cuốn sách
                var contract = validContracts.FirstOrDefault(c => c.Books.Any(book => book.Id == b.Id));

                // Lấy phần trăm lợi nhuận của Publisher từ hợp đồng (hoặc 0 nếu không có hợp đồng)
                var publisherSharePercentage = contract?.RevenueSharePercentage ?? 0m;

                // Tính phần trăm lợi nhuận của Contributor theo công thức
                var contributorSharePercentage = (100 - publisherSharePercentage) * revenueSharePercentage / 100;

                // Chuyển đổi sách thành DTO
                return new BookContributorDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    OriginalTitle = b.OriginalTitle,
                    Description = b.Description,
                    PublicationYear = b.PublicationYear,
                    CoverImage = b.CoverImage,
                    ISBN_10 = b.ISBN_10,
                    ISBN_13 = b.ISBN_13,
                    AgeLimit = b.AgeLimit,
                    ContributorSharePercentage = (decimal)contributorSharePercentage,  // Gán phần trăm lợi nhuận của Contributor
                    TotalPublishedRecaps = b.Recaps.Count(r => r.isPublished),
                    PublisherId = b.PublisherId,
                    PublisherName = b.Publisher?.PublisherName,
                    AuthorNames = b.Authors.Select(a => a.Name).ToList(),
                    CategoryNames = b.Categories.Select(c => c.Name).ToList(),
                    CategoryIds = b.Categories.Select(c => c.Id).ToList()  // Thêm CategoryIds
                };
            }).ToList();

            return bookDtos;
        }


        public async Task<ApiResponse<List<BooksDTO>>> GetAllBooksNoApprovedContractsAsync()
        {
            try
            {
                var books = await _unitOfWork.BookRepository.GetBooksWithNoApprovedContractsAsync();

                if (!books.Any())
                {
                    return new ApiResponse<List<BooksDTO>>
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new[] { "No data available." },
                        Data = null
                    };
                }

                return new ApiResponse<List<BooksDTO>>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = books
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BooksDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message },
                    Data = null
                };
            }
        }

    }
}
