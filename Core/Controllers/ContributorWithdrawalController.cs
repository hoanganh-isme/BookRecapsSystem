using BusinessObject.ViewModels.Withdrawal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [Route("api/contributorwithdrawal")]
    [ApiController]
    [AllowAnonymous]
    public class ContributorWithdrawalController : ControllerBase
    {
        private readonly IContributorWithdrawalService _contributorWithdrawalService;

        public ContributorWithdrawalController(IContributorWithdrawalService contributorWithdrawalService)
        {
            _contributorWithdrawalService = contributorWithdrawalService;
        }

        // Tạo khoản thanh toán cho Contributor
        [HttpPost("createdrawal/{contributorId}")]
        public async Task<IActionResult> CreateDrawal(Guid contributorId, decimal amount)
        {
            var response = await _contributorWithdrawalService.CreateDrawal(contributorId, amount);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpPut("processwithdrawal")]
        public async Task<IActionResult> AcceptWithdrawal(Guid withdrawalId, [FromForm] ProcessWithdrawal processWithdrawal, IFormFile? ImageURL)
        {
            Stream ImageURLStream = null;
            string ImageURLContentType = null;
            if (ImageURL != null && ImageURL.Length > 0)
            {
                ImageURLStream = ImageURL.OpenReadStream();
                ImageURLContentType = ImageURL.ContentType;
            }
            var response = await _contributorWithdrawalService.AcceptWithdrawal(withdrawalId, ImageURLStream, ImageURLContentType, processWithdrawal);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getdrawalbyid/{id}")]
        public async Task<IActionResult> GetPayoutById(Guid id)
        {
            var response = await _contributorWithdrawalService.GetWithdrawalById(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getlistdrawalbycontributorid/{contributorid}")]
        public async Task<IActionResult> GetPayoutByContributorId(Guid contributorid)
        {
            var response = await _contributorWithdrawalService.GetDrawalByContributorId(contributorid);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getallcontributorswithdrawals")]
        public async Task<IActionResult> GetAllContributorsWithDrawals()
        {
            var response = await _contributorWithdrawalService.GetAllContributorWithDrawalsAsync();
            if (response == null || response.Count == 0)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getalldrawals")]
        public async Task<IActionResult> GetAllDrawals()
        {
            var response = await _contributorWithdrawalService.GetAllDrawalsAsync();
            if (response == null || response.Count == 0)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
