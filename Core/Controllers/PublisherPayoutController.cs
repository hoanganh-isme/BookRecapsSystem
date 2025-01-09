using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PublisherPayoutController : ControllerBase
    {
        private readonly IBookEarningService _bookEarningService;
        private readonly IPublisherPayoutService _publisherPayoutService;

        public PublisherPayoutController(IBookEarningService bookEarningService,
            IPublisherPayoutService publisherPayoutService) 
        {
            _bookEarningService = bookEarningService;
            _publisherPayoutService = publisherPayoutService;
        }

        [HttpGet("getpreparepayoutinfobypublisherid/{publisherId}")]
        public async Task<IActionResult> GetPublisherEarnings(Guid publisherId, DateTime toDate)
        {
            var response = await _bookEarningService.GetPublisherEarningsAsync(publisherId, toDate);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        // Tạo khoản thanh toán cho Contributor
        [HttpPost("createpayout/{publisherId}")]
        public async Task<IActionResult> CreatePublisherPayout(Guid publisherId, IFormFile ImageURL, string description, DateTime toDate)
        {
            Stream ImageURLStream = null;
            string ImageURLContentType = null;
            if (ImageURL != null && ImageURL.Length > 0)
            {
                ImageURLStream = ImageURL.OpenReadStream();
                ImageURLContentType = ImageURL.ContentType;
            }
            var response = await _publisherPayoutService.CreatePublisherPayoutAsync(publisherId, ImageURLStream, ImageURLContentType, description, toDate);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getpayoutinfobyid/{id}")]
        public async Task<IActionResult> GetPayoutById(Guid id)
        {
            var response = await _publisherPayoutService.GetPayoutById(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getlistpayoutinfobypublisherid/{publiserId}")]
        public async Task<IActionResult> GetPayoutByPublisherId(Guid publiserId)
        {
            var response = await _publisherPayoutService.GetPayoutByPublisherId(publiserId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getallpublisherswithpayouts")]
        public async Task<IActionResult> GetAllPublishersWithPayouts()
        {
            var response = await _publisherPayoutService.GetAllPublishersWithPayoutsAsync();
            if (response == null || response.Count == 0)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
