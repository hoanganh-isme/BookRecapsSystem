using AutoMapper;
using BusinessObject.ViewModels.Highlight;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/highlight")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class HighlightController : ControllerBase
    {
        private readonly IHighlightService _highlightService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public HighlightController(IHighlightService highlightService, IMapper mapper, ICurrentUserService currentUser)
        {
            _highlightService = highlightService;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        // GET: api/highlight/{id:guid}
        [HttpGet("gethighlightbyid/{id}")]
        public async Task<IActionResult> GetHighlightById(Guid id)
        {
            var response = await _highlightService.GetHighlightById(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // GET: api/highlight/recap/{recapId:guid}
        [HttpGet("gethighlightbyrecapid/{recapId}")]
        public async Task<IActionResult> GetHighlightByRecapId(Guid recapId, Guid userId)
        {
            var response = await _highlightService.GetHighlightByRecapId(recapId, userId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // GET: api/highlight
        [HttpGet]
        public async Task<IActionResult> GetAllHighlights()
        {
            var response = await _highlightService.GetAllHighlights();
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // POST: api/highlight
        [HttpPost("createhighlight")]
        public async Task<IActionResult> CreateHighlight([FromBody] CreateHighlightRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            request.UserId = Guid.Parse(userId);

            var response = await _highlightService.CreateHighlight(request);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // PUT: api/highlight/{id:guid}
        [HttpPut("updatehighlight/{id}")]
        public async Task<IActionResult> UpdateHighlight(Guid id, [FromBody] UpdateHighlightRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            request.Id = id;
            var response = await _highlightService.UpdateHighlight(request, Guid.Parse(userId));
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteHighlight(Guid id)
        {
            var userId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _highlightService.DeleteHighlight(id, Guid.Parse(userId));
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdelete/{id:guid}")]
        public async Task<IActionResult> SoftDeleteHighlight(Guid id)
        {
            var userId = _currentUser.GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _highlightService.SoftDeleteHighlight(id, Guid.Parse(userId));
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
