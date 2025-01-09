using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Threading.Tasks;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/likes")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ICurrentUserService _currentUser;
        public LikeController(ILikeService likeService,
             ICurrentUserService currentUser)
        {
            _likeService = likeService;
            _currentUser = currentUser;

        }
        [HttpPost("createlike/{recapId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateLike(Guid recapId)
        {
            var currentUserId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            // Convert the user ID from string to Guid
            if (!Guid.TryParse(currentUserId, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var response = await _likeService.AddLikeAsync(recapId, userId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Xóa Like khỏi một Recap
        [HttpDelete("remove/{recapId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveLike(Guid recapId)
        {
            var currentUserId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            // Convert the user ID from string to Guid
            if (!Guid.TryParse(currentUserId, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var response = await _likeService.RemoveLikeAsync(recapId, userId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Lấy số lượng Like của một Recap
        [HttpGet("count/{recapId:guid}")]
        public async Task<IActionResult> GetLikesCount(Guid recapId)
        {
            var response = await _likeService.GetLikesCountByRecapIdAsync(recapId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
