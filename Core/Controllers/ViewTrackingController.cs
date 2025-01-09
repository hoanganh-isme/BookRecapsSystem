using BusinessObject.Enums;
using BusinessObject.ViewModels.Viewtrackings;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/viewtracking")]
    [AllowAnonymous]
    public class ViewTrackingController : ControllerBase
    {
        private readonly IViewTrackingService _viewTrackingService;
        private readonly ICurrentUserService _currentUser;

        public ViewTrackingController(IViewTrackingService viewTrackingService,
            ICurrentUserService currentUser)
        {
            _viewTrackingService = viewTrackingService;
            _currentUser = currentUser;
        }
        [HttpPost("createviewtracking")]
        public async Task<IActionResult> CreateViewTracking(Guid recapid, Guid? userId, DeviceType deviceType)
        {
            var result = await _viewTrackingService.CreateViewTrackingAsync(recapid, userId, deviceType);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpGet("getviewtrackingbyid/{id}")]
        public async Task<IActionResult> GetViewTrackingById(Guid id)
        {
            var result = await _viewTrackingService.GetViewTrackingById(id);

            if (!result.Succeeded)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        [HttpPut("updateduration/{id}")]
        public async Task<IActionResult> UpdateDurationViewtracking(Guid id, int duration)
        {
            var result = await _viewTrackingService.UpdateDurationViewtracking(id, duration);

            if (!result.Succeeded)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("getallviewtracking")]
        public async Task<IActionResult> GetAllViewTrackings()
        {
            var result = await _viewTrackingService.GetAllViewTrackings();

            if (!result.Succeeded)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        [HttpGet("getviewtrackingbyuserid/{id}")]
        public async Task<IActionResult> GetViewTrackingByUserId(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _viewTrackingService.GetViewTrackingByUserId(id, pageNumber, pageSize);

            if (!result.Succeeded)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

    }
}
