using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Books;
using BusinessObject.ViewModels.Viewtrackings;
using Core.Auth.Permissions;
using Core.Auth.Services;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Services.Interface;
using Services.Responses;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/book")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public BookController(IBookService bookService, IMapper mapper, IUriService uriService)
        {
            _bookService = bookService;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet("getallbooks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBooks()
        {
            var response = await _bookService.GetAllBooks();
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("info/{volumeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookInfo(string volumeId)
        {
            var bookInfo = await _bookService.GetBookInfoAsync(volumeId);
            return Ok(bookInfo);
        }

        [HttpGet("getbookbyid/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookById(Guid id)
        {
            var response = await _bookService.GetBookById(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("getbookbyidforpublisher/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookByIdForPubliser(Guid id)
        {
            var response = await _bookService.GetBookByIdRecapNotPublished(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("createbook")]
        public async Task<IActionResult> CreateBook([FromForm] BookCreateRequest request,
                                                    IFormFile? coverImage,
                                                    List<IFormFile>? authorImages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                // Parse danh sách tác giả từ form
                if (!string.IsNullOrEmpty(Request.Form["authors"]))
                {
                    request.Authors = JsonConvert.DeserializeObject<List<AuthorRequest>>(Request.Form["authors"]);
                }

                // Kiểm tra danh sách tác giả
                if (request.Authors == null || !request.Authors.Any())
                {
                    return BadRequest(new { message = "Authors list is required." });
                }

                // Xử lý ảnh bìa
                Stream? coverImageStream = null;
                string? coverImageContentType = null;

                if (coverImage != null && coverImage.Length > 0)
                {
                    coverImageStream = coverImage.OpenReadStream();
                    coverImageContentType = coverImage.ContentType;
                }

                // Xử lý ảnh tác giả
                // Xử lý ảnh tác giả
                var authorImageStreams = new Dictionary<Guid, Stream>();
                if (authorImages != null && authorImages.Any())
                {
                    int imageIndex = 0;

                    foreach (var author in request.Authors)
                    {
                        // Kiểm tra nếu là tác giả mới
                        if (author.Id == null && imageIndex < authorImages.Count)
                        {
                            var authorImage = authorImages[imageIndex];
                            if (authorImage.Length > 0)
                            {
                                var authorImageStream = authorImage.OpenReadStream();
                                var authorGuid = Guid.NewGuid(); // Tạo GUID mới cho mỗi ảnh tác giả
                                authorImageStreams.Add(authorGuid, authorImageStream); // Lưu GUID và ảnh vào dictionary
                                author.Image = authorGuid.ToString(); // Gắn GUID vào AuthorRequest
                                imageIndex++; // Chỉ tăng index khi ảnh được gắn
                            }
                        }
                    }
                }


                // Gọi service để tạo sách
                var response = await _bookService.CreateBook(request, coverImageStream, coverImageContentType, authorImageStreams);
                if (!response.Succeeded)
                {
                    return BadRequest(new { message = response.Message, errors = response.Errors });
                }

                return Ok(new { message = response.Message, data = response.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal server error occurred.", details = ex.Message });
            }
        }





        [HttpPut("updatebook/{id:guid}")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromForm] BookUpdateRequest request, IFormFile coverImage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Thiết lập ID cho request
            request.Id = id;

            // Chuyển đổi IFormFile thành Stream
            Stream coverImageStream = coverImage?.OpenReadStream();

            // Gọi service để cập nhật sách
            var response = await _bookService.UpdateBook(request, coverImageStream, coverImage?.ContentType);

            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpDelete("deletebook/{id:guid}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            var response = await _bookService.DeleteBook(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("softdeletebook/{id:guid}")]
        public async Task<IActionResult> SoftDeleteBook(Guid id)
        {
            var response = await _bookService.SoftDeleteBook(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getallbookswithcontract")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBooksWithApprovedContracts()
        {
            var response = await _bookService.GetAllBooksWithApprovedContractsAsync();

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet("getallbooksbyfilterforcontributor")]
        [AllowAnonymous]
        public async Task<ApiResponse<PagedResponse<List<BookContributorDTO>>>> GetBooksWithFiltersAsync([FromQuery] BookPaginationFilter filter)
        {
            try
            {
                // Lấy tất cả sách đã được chuyển đổi thành DTO từ service
                var bookDtos = await _bookService.GetBooksWithFiltersAsync();

                // Tạo bộ lọc hợp lệ từ BookPaginationFilter
                var validFilter = new BookPaginationFilter(
                    filter.PageNumber,
                    filter.PageSize,
                    filter.SortBy,
                    filter.SortOrder,
                    filter.SearchTerm,
                    filter.FilterBy,
                    filter.FilterValue,
                    filter.CategoryId,
                    filter.PublisherId,
                    filter.ContributorRevenueShareMin
                );

                // Lọc theo SearchTerm nếu có
                if (!string.IsNullOrEmpty(validFilter.SearchTerm))
                {
                    var searchTerm = validFilter.SearchTerm;

                    bookDtos = bookDtos.Where(b =>
                           (!string.IsNullOrEmpty(b.Title) && b.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (!string.IsNullOrEmpty(b.OriginalTitle) && b.OriginalTitle.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (!string.IsNullOrEmpty(b.Description) && b.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (!string.IsNullOrEmpty(b.ISBN_10) && b.ISBN_10.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (!string.IsNullOrEmpty(b.ISBN_13) && b.ISBN_13.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           b.AuthorNames.Any(a => a.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           b.CategoryNames.Any(c => c.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                           ).ToList();

                    // Thêm lọc theo CategoryId nếu SearchTerm là ID category
                    if (Guid.TryParse(searchTerm, out var categoryId))
                    {
                        bookDtos = bookDtos.Where(b => b.CategoryIds.Contains(categoryId)).ToList();
                    }
                }

                // Lọc theo CategoryId nếu có
                if (validFilter.CategoryId.HasValue)
                {
                    bookDtos = bookDtos.Where(b => b.CategoryIds.Contains(validFilter.CategoryId.Value)).ToList();
                }

                // Lọc theo PublisherId nếu có
                if (validFilter.PublisherId.HasValue)
                {
                    bookDtos = bookDtos.Where(b => b.PublisherId == validFilter.PublisherId.Value).ToList();
                }

                // Lọc theo Contributor Revenue Share Minimum nếu có
                if (validFilter.ContributorRevenueShareMin.HasValue)
                {
                    bookDtos = bookDtos.Where(b => b.ContributorSharePercentage >= validFilter.ContributorRevenueShareMin.Value).ToList();
                }

                // Sắp xếp theo SortBy và SortOrder
                if (!string.IsNullOrEmpty(validFilter.SortBy))
                {
                    var parameter = Expression.Parameter(typeof(BookContributorDTO), "b");
                    var property = Expression.Property(parameter, validFilter.SortBy);
                    var lambda = Expression.Lambda(property, parameter);
                    var methodName = validFilter.SortOrder.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
                    var resultExpression = Expression.Call(typeof(Queryable), methodName,
                        new Type[] { typeof(BookContributorDTO), property.Type },
                        bookDtos.AsQueryable().Expression, Expression.Quote(lambda));
                    bookDtos = bookDtos.AsQueryable().Provider.CreateQuery<BookContributorDTO>(resultExpression).ToList();
                }

                // Tính tổng số bản ghi
                var totalRecords = bookDtos.Count;

                // Phân trang: Sử dụng Skip và Take để phân trang
                var pagedBooks = bookDtos
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToList();

                // Tạo PagedResponse sử dụng PaginationHelper để phân trang
                var pagedResponse = PaginationHelper.CreateBookPagedResponse(
                    pagedBooks,
                    validFilter,
                    totalRecords,
                    _uriService,
                    Request.Path.Value
                );

                return new ApiResponse<PagedResponse<List<BookContributorDTO>>>
                {
                    Succeeded = true,
                    Message = "Lấy dữ liệu thành công.",
                    Data = pagedResponse
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponse<List<BookContributorDTO>>>
                {
                    Succeeded = false,
                    Message = "An error occurred.",
                    Errors = new[] { ex.Message },
                    Data = null
                };
            }
        }






        [HttpGet("getallbookswithnoapprovedcontract")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBooksWithNoContracts()
        {
            var response = await _bookService.GetAllBooksNoApprovedContractsAsync();

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
