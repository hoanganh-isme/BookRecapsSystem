using BusinessObject.ViewModels.Recaps;
using BusinessObject.ViewModels.KeyIdea;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;
using BusinessObject.ViewModels.Contents;
using Hangfire;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/recap")]
    [AllowAnonymous]
    public class RecapController : ControllerBase
    {
        private readonly IRecapService _recapService;
        private readonly ICurrentUserService _currentUser;
        private readonly IRecapVersionService _recapVersionService;
        private readonly IRecapEarningService _recapEarningService;
        public RecapController(IRecapService recapService, 
            ICurrentUserService currentUser,
            IRecapVersionService recapVersionService,
            IRecapEarningService recapEarningService)
        {
            _recapService = recapService;
            _currentUser = currentUser;
            _recapVersionService = recapVersionService;
            _recapEarningService = recapEarningService;
        }

        [HttpGet("get-all-recapsversionbycontributorId")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetAllRecapVersionsByContributor()
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

            // Call the service to get all recap versions for the current user
            var response = await _recapVersionService.GetAllRecapVersions(userId);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("get-all-recapsbycontributorId")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetAllRecapByContributor(string? published)
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

            // Call the service to get all recap versions for the current user
            var response = await _recapService.GetAllRecapByContributorId(userId, published);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPost("createrecap")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateRecap([FromBody] CreateRecapRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy current user ID từ service
            var contributor = _currentUser.GetCurrentUserId();

            // Kiểm tra nếu contributor ID không tồn tại
            if (string.IsNullOrEmpty(contributor))
            {
                return BadRequest("User is not authenticated.");
            }

            // Chuyển đổi từ string sang Guid
            if (!Guid.TryParse(contributor, out Guid contributorId))
            {
                return BadRequest("Invalid Contributor ID.");
            }

            // Gán ContributorId cho request
            request.ContributorId = contributorId;

            // Gọi service để tạo recap
            var response = await _recapService.CreateRecap(request);

            // Kiểm tra kết quả từ service
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPost("create-version")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateRecapVersion([FromBody] CreateRecapVersion request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var contributor = _currentUser.GetCurrentUserId();

            // Kiểm tra nếu contributor ID không tồn tại
            if (string.IsNullOrEmpty(contributor))
            {
                return BadRequest("User is not authenticated.");
            }

            // Chuyển đổi từ string sang Guid
            if (!Guid.TryParse(contributor, out Guid contributorId))
            {
                return BadRequest("Invalid Contributor ID.");
            }

            // Gán ContributorId cho request
            request.ContributorId = contributorId;

            // Call the service method to create a recap version
            var response = await _recapVersionService.CreateRecapVersionAsync(request);

            // Check if the operation was successful
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("/generate-audio")]
        public async Task<IActionResult> GenerateRecapVersionAsync([FromBody] UpdateRecapVersion request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _recapVersionService.GenerateRecapVersionAsync(request);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpPut("/upload-audio")]
        public async Task<IActionResult> UploadAudioByContributor([FromForm] Guid recapVersionId, IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { Message = "Audio file cannot be null or empty." });
            }
            // Call the service method to upload the audio by contributor
            var response = await _recapVersionService.UploadAudioByContributorAsync(recapVersionId, audioFile);

            // Check if the operation was successful
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("/change-recapversion-status")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ChangeRecapVersionStatus([FromBody] ChangeVersionStatus request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = _currentUser.GetCurrentUserId();

            // Gọi service để thay đổi trạng thái phiên bản nội dung
            var response = await _recapVersionService.ChangeVersionStatus(request);

            // Kiểm tra kết quả từ service
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("/version/{versionId}")]
        public async Task<IActionResult> GetRecapVersionById([FromRoute] Guid versionId)
        {
            // Gọi service để lấy phiên bản nội dung theo ID
            var response = await _recapVersionService.GetVersionById(versionId);

            // Kiểm tra kết quả từ service
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        [HttpGet("/getrecapbyId/{recapId}")]
        public async Task<IActionResult> GetRecapById([FromRoute] Guid recapId, [FromQuery] Guid? userId = null)
        {
            // Gọi service để lấy phiên bản nội dung theo ID
            var response = await _recapService.GetRecapById(recapId, userId);

            // Kiểm tra kết quả từ service
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        [HttpDelete("deleterecap/{recapId}")]
        public async Task<IActionResult> DeleteRecap([FromRoute] Guid recapId)
        {
            var response = await _recapService.DeleteRecap(recapId);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("deleteversion/{versionId}")]
        public async Task<IActionResult> DeleteRecapVersion([FromRoute] Guid versionId)
        {
            var response = await _recapVersionService.DeleteRecapVersion(versionId);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdeleteversion/{versionId}")]
        public async Task<IActionResult> SoftDeleteRecapVersion([FromRoute] Guid versionId)
        {
            var response = await _recapVersionService.SoftDeleteRecapVersion(versionId);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("Getallrecap")]
        public async Task<IActionResult> GetAllRecap()
        {
            var response = await _recapService.GetAllRecap();

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpPut("updaterecap/{recapId}")]
        public async Task<IActionResult> UpdateRecap([FromRoute] Guid recapId, [FromBody] UpdateRecapForPublished request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Add RecapId to the request object if required
            request.RecapId = recapId;

            var response = await _recapService.UpdateRecap(request);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPut("choose-version/{recapId}")]
        public async Task<IActionResult> ChooseVersionForRecap([FromRoute] Guid recapId, [FromBody] ChooseVersion version)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _recapService.ChooseVersionForRecap(recapId, version);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("get-all-versionapprovedbyrecapid")]
        public async Task<IActionResult> GetVersionApprovedByRecapId(Guid recapId)
        {

            var response = await _recapVersionService.GetVersionApprovedByRecapId(recapId);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        
        [HttpGet("get-all-versionnotdraft")]
        public async Task<IActionResult> GetAllVersionNotDraft()
        {

            var response = await _recapVersionService.GetListRecapVersionNotDraft();

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
