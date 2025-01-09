using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.ViewModels.Appeals;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/appeal")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AppealController : ControllerBase
    {
        private readonly IAppealService _appealService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public AppealController(IAppealService appealService, IMapper mapper, ICurrentUserService currentUser)
        {
            _appealService = appealService;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        [HttpPost("createappeal")]
        [Authorize]
        public async Task<IActionResult> CreateAppeal([FromBody] CreateAppealRequest request)
        {
            var currentContributorId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentContributorId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentContributorId, out Guid contributorId))
            {
                return BadRequest("Invalid contributor ID.");
            }
            request.ContributorId = contributorId;
            var result = await _appealService.CreateAppeal(request);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("updateappeal")]
        [Authorize]
        public async Task<IActionResult> UpdateAppeal([FromBody] UpdateAppealContributor request)
        {
            var currentContributorId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentContributorId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentContributorId, out Guid contributorId))
            {
                return BadRequest("Invalid contributor ID.");
            }
            var result = await _appealService.UpdateAppeal(request, contributorId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("responseappealbystaff")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ResponseAppeal([FromBody] UpdateAppealResponse request, RecapStatus recapVersionStatus)
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
            var result = await _appealService.ResponseAppeal(request, staffId, recapVersionStatus);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("deleteappeal/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAppeal(Guid id)
        {
            var currentContributorId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentContributorId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentContributorId, out Guid contributorId))
            {
                return BadRequest("Invalid contributor ID.");
            }
            var result = await _appealService.DeleteAppeal(id, contributorId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpDelete("softdeleteappeal/{id}")]
        [Authorize]
        public async Task<IActionResult> SoftDeleteAppeal(Guid id)
        {
            var currentContributorId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentContributorId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentContributorId, out Guid contributorId))
            {
                return BadRequest("Invalid contributor ID.");
            }
            var result = await _appealService.SoftDeleteAppeal(id, contributorId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getappealbyid/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAppealById(Guid id)
        {
            var result = await _appealService.GetAppealById(id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
        [HttpGet("getappealbyreviewid/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAppealByReviewId(Guid id)
        {
            var result = await _appealService.GetAppealByReviewId(id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("getappealbystaff/{staffId}")]
        public async Task<IActionResult> GetAppealsByStaffId(Guid staffId)
        {
            var result = await _appealService.GetAppealByStaffId(staffId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetAppealsByUserId(Guid userId)
        {
            var result = await _appealService.GetAppealByUserId(userId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("getappealunderreview")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetAllAppealsWithUnderReviewStatus()
        {
            var result = await _appealService.GetAllAppealsWithUnderReviewStatus();
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("getallappeals")]
        [Authorize]
        public async Task<IActionResult> GetAllAppeals()
        {
            var result = await _appealService.GetAllAppeals();
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpPut("changeappealstatus")]
        [Authorize]
        public async Task<IActionResult> ChangeAppealStatus([FromBody] UpdateAppealStatus request)
        {
            var currentContributorId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentContributorId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentContributorId, out Guid contributorId))
            {
                return BadRequest("Invalid contributor ID.");
            }
            var result = await _appealService.ChangeAppealStatus(request, contributorId);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
