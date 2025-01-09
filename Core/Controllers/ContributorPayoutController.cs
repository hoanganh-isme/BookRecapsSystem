using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Services.Interface;

namespace Core.Controllers
{
    [Route("api/contributorpayout")]
    [ApiController]
    [AllowAnonymous]
    public class ContributorPayoutController : ControllerBase
    {
        private readonly IRecapEarningService _recapEarningService;
        private readonly IContributorPayoutService _contributorPayoutService;
        public ContributorPayoutController(IRecapEarningService recapEarningService,
            IContributorPayoutService contributorPayoutService)
        {
            _recapEarningService = recapEarningService;
            _contributorPayoutService = contributorPayoutService;
        }
        [HttpGet("getpreparepayoutinfobycontributorid/{contributorId}")]
        public async Task<IActionResult> GetContributorEarnings(Guid contributorId, DateTime toDate)
        {
            var response = await _recapEarningService.GetContributorEarningsAsync(contributorId, toDate);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Tạo khoản thanh toán cho Contributor
        [HttpPost("createpayout/{contributorId}")]
        public async Task<IActionResult> CreateContributorPayout(Guid contributorId, string description, DateTime toDate)
        {
            var response = await _contributorPayoutService.CreateContributorPayoutAsync(contributorId, description, toDate);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getpayoutinfobyid/{id}")]
        public async Task<IActionResult> GetPayoutById(Guid id)
        {
            var response = await _contributorPayoutService.GetPayoutById(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getlistpayoutinfobycontributorid/{contributorid}")]
        public async Task<IActionResult> GetPayoutByContributorId(Guid contributorid)
        {
            var response = await _contributorPayoutService.GetPayoutByContributorId(contributorid);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("getallcontributorswithpayouts")]
        public async Task<IActionResult> GetAllContributorsWithPayouts()
        {
            var response = await _contributorPayoutService.GetAllContributorWithPayoutsAsync();
            if (response == null || response.Count == 0)
            {
                return BadRequest(response);
            }
            return Ok(response); 
        }
    }
}
