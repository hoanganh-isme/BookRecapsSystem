using AutoMapper;
using BusinessObject.ViewModels.Review;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/review")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public ReviewController(IReviewService reviewService, IMapper mapper,
            ICurrentUserService currentUser)
        {
            _reviewService = reviewService;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        // GET: api/review/{id:guid}
        [HttpGet("getreviewbyid/{id}")]
        public async Task<IActionResult> GetReviewById(Guid id)
        {
            var response = await _reviewService.GetReviewById(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("getreviewbyrecapversionid/{id}")]
        public async Task<IActionResult> GetReviewByRecapVersionId(Guid id)
        {
            var response = await _reviewService.GetReviewByRecapVersion(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("getreviewbyStaffId/{staffId}")]
        public async Task<IActionResult> GetReviewByStaffId(Guid staffId)
        {
            
            var response = await _reviewService.GetReviewByStaffId(staffId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // GET: api/review
        [HttpGet("getallreview")]
        public async Task<IActionResult> GetAllReviews()
        {
            var response = await _reviewService.GetAllReviews();
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // POST: api/review
        [HttpPost("createreview")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gọi hàm GetCurrentUserId để lấy StaffId
            var staff = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(staff))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(staff, out Guid staffId))
            {
                return BadRequest("Invalid Staff ID.");
            }
            // Gán StaffId vào request
            request.StaffId = staffId;

            // Gọi service để tạo review
            var response = await _reviewService.CreateReview(request);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("updatereview/{id}")]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.Id = id;
            var currentUserId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentUserId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewService.UpdateReview(request, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // DELETE: api/review/{id:guid}
        [HttpDelete("deletereview/{id:guid}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var currentUserId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentUserId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewService.DeleteReview(id, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdeletereview/{id:guid}")]
        public async Task<IActionResult> SoftDeleteReview(Guid id)
        {
            var currentUserId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentUserId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewService.SoftDeleteReview(id, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
