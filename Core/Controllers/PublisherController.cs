using BusinessObject.Data;
using BusinessObject.Models;
using BusinessObject.ViewModels.Publisher;
using Core.Auth.Services;
using Core.Enums;
using Core.Models.UserModels;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [Route("api/publisher")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PublisherController : ControllerBase
    {

        private readonly IPublisherService _publisherService;

        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }
        // Get Publisher by UserId
        [HttpGet("getbypublisheruser/{userId}")]
        public async Task<IActionResult> GetPublisherByUserId(Guid userId)
        {
            var result = await _publisherService.GetPublisherByUserId(userId);
            if (!result.Succeeded)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Get Publisher by PublisherId
        [HttpGet("getbypublisherid/{publisherId}")]
        public async Task<IActionResult> GetPublisherById(Guid publisherId)
        {
            var result = await _publisherService.GetPublisherById(publisherId);
            if (!result.Succeeded)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Get all Publishers
        [HttpGet("getallpublishers")]
        public async Task<IActionResult> GetAllPublishers()
        {
            var result = await _publisherService.GetAllPublisher();
            if (!result.Succeeded)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Update Publisher information
        [HttpPut("updatepublisherinfo/{publisherId}")]
        public async Task<IActionResult> UpdatePublisherInfo(Guid publisherId, [FromBody] UpdatePublisher updatePublisher)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

            var result = await _publisherService.UpdatePublisherInfo(publisherId, updatePublisher);
            if (!result.Succeeded)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
