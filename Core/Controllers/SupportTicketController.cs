using BusinessObject.ViewModels.Appeals;
using BusinessObject.ViewModels.SupportTicket;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/supportticket")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly ICurrentUserService _currentUser;

        public SupportTicketController(ISupportTicketService supportTicketService, ICurrentUserService currentUser)
        {
            _supportTicketService = supportTicketService;
            _currentUser = currentUser;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSupportTicket([FromBody] CreateSupportTicketRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            request.UserId = Guid.Parse(userId);
            var response = await _supportTicketService.CreateSupportTicket(request);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSupportTicket(Guid id, [FromBody] UpdateSupportTicketRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _supportTicketService.UpdateSupportTicket(request, Guid.Parse(userId), id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPut("responseticket/{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ResponseTicket([FromBody] ResponseSupportTicket request, Guid id)
        {         
            var result = await _supportTicketService.ResponseTicket(request, id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("changeappealstatus")]
        public async Task<IActionResult> ChangeAppealStatus([FromBody] UpdateStatusTicket request, Guid id)
        {
            var currentId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentId, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var result = await _supportTicketService.ChangeTicketStatus(request, userId, id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSupportTicket(Guid id)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _supportTicketService.DeleteSupportTicket(id, Guid.Parse(userId));
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdelete/{id}")]
        public async Task<IActionResult> SoftDeleteSupportTicket(Guid id)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _supportTicketService.SoftDeleteSupportTicket(id, Guid.Parse(userId));
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetSupportTicketById(Guid id)
        {
            var response = await _supportTicketService.GetSupportTicketById(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("getsupportticketbyuser/{userId}")]
        public async Task<IActionResult> GetSupportTicketsByUserId(Guid userId)
        {
            var response = await _supportTicketService.GetSupportTicketByUserId(userId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("getallsupportticket")]
        public async Task<IActionResult> GetAllSupportTickets()
        {
            var response = await _supportTicketService.GetAllSupportTicketsWithCurrentVersionAndReview();
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }

}
