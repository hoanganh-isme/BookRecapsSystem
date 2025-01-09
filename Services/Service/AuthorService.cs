using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Author;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository;
using Services.Interface;
using Services.Responses;
using Services.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GoogleCloudService _googleCloudService;

        public AuthorService(IUnitOfWork unitOfWork, IMapper mapper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleCloudService = new GoogleCloudService(configuration);
        }
        public async Task<ApiResponse<Author>> CreateAuthor(AuthorCreateRequest authorrequest, Stream? ImageStream, string? ImageContentType)
        {


            if (ImageStream != null && !string.IsNullOrEmpty(ImageContentType))
            {
                // Tạo thư mục động cho ảnh bìa dựa theo BookId
                string authorFolderName = $"author_images/";
                string ImageFileName = $"{authorFolderName}author_{Guid.NewGuid()}.jpg";

                var ImageUrl = await _googleCloudService.UploadImageAsync(ImageFileName, ImageStream, ImageContentType);
                authorrequest.Image = ImageUrl;  // Lưu URL ảnh bìa
            }

            var author = _mapper.Map<Author>(authorrequest);
            await _unitOfWork.AuthorRepository.AddAsync(author);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<Author>
            {
                Succeeded = true,
                Message = "Tạo thành công.",
                Errors = new[] { "No data available." },
                Data = author
            };
        }

        public async Task<ApiResponse<Author>> UpdateAuthor(AuthorUpdateRequest request, Stream? imageStream, string? imageContentType)
        {
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.Id);

            if (author == null)
            {
                return new ApiResponse<Author>
                {
                    Succeeded = false,
                    Message = "Author not found.",
                    Errors = new[] { "The specified author does not exist." }
                };
            }

            // Xử lý ảnh nếu có
            if (imageStream != null && !string.IsNullOrEmpty(imageContentType))
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(author.Image))
                {
                    Uri oldUri = new Uri(author.Image);
                    string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                    await _googleCloudService.DeleteFileAsync(oldObjectName);
                }

                // Tạo tên file mới cho ảnh và upload
                string authorFolderName = $"author_images/";
                string imageFileName = $"{authorFolderName}author_{Guid.NewGuid()}.jpg";

                var imageUrl = await _googleCloudService.UploadImageAsync(imageFileName, imageStream, imageContentType);
                author.Image = imageUrl; // Cập nhật URL ảnh mới
            }

            // Cập nhật các thuộc tính nếu request có giá trị
            if (!string.IsNullOrEmpty(request.Name))
            {
                author.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                author.Description = request.Description;
            }
            // Thêm các thuộc tính khác nếu cần

            _unitOfWork.AuthorRepository.Update(author);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Author>
            {
                Succeeded = true,
                Message = "Author updated successfully.",
                Data = author
            };
        }

        public async Task<ApiResponse<bool>> DeleteAuthor(Guid authorId)
        {
           
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(authorId);           
            if (author == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Author not found.",
                    Errors = new[] { "The specified author does not exist." }
                };
            }

            // Check if author connect with one or more books
            var hasBooks = await _unitOfWork.BookRepository.AnyAsync(book => book.Authors.Any(author => author.Id == authorId));
            if (hasBooks)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Author cannot be deleted.",
                    Errors = new[] { "This author is associated with existing books." }
                };
            }

            _unitOfWork.AuthorRepository.Delete(author);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Delete Success.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteAuthor(Guid authorId)
        {

            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(authorId);
            if (author == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Author not found.",
                    Errors = new[] { "The specified author does not exist." }
                };
            }

            // Check if author connect with one or more books
            var hasBooks = await _unitOfWork.BookRepository.AnyAsync(book => book.Authors.Any(author => author.Id == authorId));
            if (hasBooks)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Author cannot be deleted.",
                    Errors = new[] { "This author is associated with existing books." }
                };
            }

            _unitOfWork.AuthorRepository.SoftDelete(author);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Delete Success.",
                Data = true
            };
        }

        public async Task<ApiResponse<List<Author>>> GetAllAuthors()
        {
            try
            {
                // Lấy tất cả các authors kèm theo danh sách books
                var authors = await _unitOfWork.AuthorRepository
                    .QueryWithIncludes(a => a.Books)  // Chỉ bao gồm Books của Authors
                    .Select(author => new Author
                    {
                        Id = author.Id,
                        Name = author.Name,
                        Image = author.Image,
                        Description = author.Description,
                        Books = author.Books.Select(book => new Book
                        {
                            Id = book.Id,
                            Title = book.Title,
                            OriginalTitle = book.OriginalTitle,
                            Description = book.Description,
                            PublicationYear = book.PublicationYear,
                            CoverImage = book.CoverImage,
                            AgeLimit = book.AgeLimit,
                            PublisherId = book.PublisherId,

                            Authors = book.Authors ?? new List<Author>(),
                            Categories = book.Categories ?? new List<Category>(),  // Khởi tạo danh sách rỗng
                            Recaps = book.Recaps ?? new List<Recap>(),  // Khởi tạo danh sách rỗng
                            BookEarnings = book.BookEarnings ?? new List<BookEarning>(),  // Khởi tạo danh sách rỗng
                            Contracts = book.Contracts ?? new List<Contract>(),  // Khởi tạo danh sách rỗng
                            CreatedAt = book.CreatedAt,
                            UpdatedAt = book.UpdatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                if (authors == null || !authors.Any())
                {
                    return new ApiResponse<List<Author>>
                    {
                        Succeeded = false,
                        Message = "No authors found.",
                        Errors = new[] { "No data available." }
                    };
                }

                return new ApiResponse<List<Author>>
                {
                    Succeeded = true,
                    Message = "Authors retrieved successfully.",
                    Data = authors
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                return new ApiResponse<List<Author>>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message }
                };
            }
        }


        public async Task<ApiResponse<Author>> GetAuthorById(Guid authorId)
        {
            try
            {
                // Lấy tác giả theo ID kèm theo danh sách sách
                var author = await _unitOfWork.AuthorRepository
                    .QueryWithIncludes(a => a.Books) // Bao gồm sách
                    .Where(a => a.Id == authorId) // Lọc theo ID
                    .Select(author => new Author
                    {
                        Id = author.Id,
                        Name = author.Name,
                        Image = author.Image,
                        Description = author.Description,
                        Books = author.Books.Select(book => new Book
                        {
                            Id = book.Id,
                            Title = book.Title,
                            OriginalTitle = book.OriginalTitle,
                            Description = book.Description,
                            PublicationYear = book.PublicationYear,
                            CoverImage = book.CoverImage,
                            AgeLimit = book.AgeLimit,
                            PublisherId = book.PublisherId,

                            Authors = book.Authors ?? new List<Author>(),  // Khởi tạo danh sách rỗng nếu null
                            Categories = book.Categories ?? new List<Category>(),  // Khởi tạo danh sách rỗng nếu null
                            Recaps = book.Recaps ?? new List<Recap>(),  // Khởi tạo danh sách rỗng nếu null
                            BookEarnings = book.BookEarnings ?? new List<BookEarning>(),  // Khởi tạo danh sách rỗng nếu null
                            Contracts = book.Contracts ?? new List<Contract>(),  // Khởi tạo danh sách rỗng nếu null
                            CreatedAt = book.CreatedAt,
                            UpdatedAt = book.UpdatedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(); // Lấy một tác giả

                if (author == null)
                {
                    return new ApiResponse<Author>
                    {
                        Succeeded = false,
                        Message = "Author not found.",
                        Errors = new[] { "The specified author does not exist." }
                    };
                }

                return new ApiResponse<Author>
                {
                    Succeeded = true,
                    Message = "Author retrieved successfully.",
                    Data = author
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                return new ApiResponse<Author>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message }
                };
            }
        }




        

    }
}
