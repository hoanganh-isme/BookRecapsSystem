using BusinessObject.ViewModels.ReviewNotes;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/reviewnote")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ReviewNoteController : ControllerBase
    {
        private readonly IReviewNoteService _reviewNoteService;
        private readonly ICurrentUserService _currentUser;

        public ReviewNoteController(IReviewNoteService reviewNoteService, ICurrentUserService currentUser)
        {
            _reviewNoteService = reviewNoteService;
            _currentUser = currentUser;
        }

        [HttpPost("createreviewnote")]
        public async Task<IActionResult> CreateReviewNote([FromBody] CreateReviewNoteRequest request)
        {
            var currentStaffId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentStaffId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentStaffId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewNoteService.CreateReviewNote(request, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("updatereviewnote/{id}")]
        public async Task<IActionResult> UpdateReviewNote(Guid id,[FromBody] UpdateReviewNoteRequest request)
        {
            var currentStaffId = _currentUser.GetCurrentUserId();
            request.Id = id;
            if (string.IsNullOrEmpty(currentStaffId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentStaffId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewNoteService.UpdateReviewNote(request, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("delete/{reviewNoteId}")]
        public async Task<IActionResult> DeleteReviewNote(Guid reviewNoteId)
        {
            var currentStaffId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentStaffId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentStaffId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewNoteService.DeleteReviewNote(reviewNoteId, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdelete/{reviewNoteId}")]
        public async Task<IActionResult> SoftDeleteReviewNote(Guid reviewNoteId)
        {
            var currentStaffId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentStaffId))
            {
                return BadRequest("User is not authenticated.");
            }

            if (!Guid.TryParse(currentStaffId, out Guid staffId))
            {
                return BadRequest("Invalid staff ID.");
            }

            var response = await _reviewNoteService.SoftDeleteReviewNote(reviewNoteId, staffId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("getallnotebyreviewid/{reviewId}")]
        public async Task<IActionResult> GetReviewNoteByReviewId(Guid reviewId)
        {
            var response = await _reviewNoteService.GetReviewNoteByReviewId(reviewId);

            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}
