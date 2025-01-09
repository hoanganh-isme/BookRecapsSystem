using BusinessObject.ViewModels.Categories;
using BusinessObject.ViewModels.KeyIdea;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/keyidea")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class KeyIdeaController : ControllerBase
    {
        private readonly IKeyIdeaService _keyIdeaService;
        private readonly IRecapVersionService _recapVersionService;
        public KeyIdeaController(IKeyIdeaService keyIdeaService, IRecapVersionService recapVersionService)
        {
            _keyIdeaService = keyIdeaService;
            _recapVersionService = recapVersionService;
        }
        [HttpPost("createkeyidea")]
        public async Task<IActionResult> CreatePrepareIdea([FromForm] PrepareIdea request, IFormFile? ImageKeyIdea)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra RecapVersion trước khi tạo KeyIdea
            var versionResponse = await _recapVersionService.GetVersionById(request.RecapVersionId);
            if (!versionResponse.Succeeded)
            {
                return BadRequest(versionResponse);
            }

            try
            {
                Stream ImageStream = null;
                string ImageContentType = null;

                // Kiểm tra và lấy dữ liệu từ ảnh nếu có
                if (ImageKeyIdea != null && ImageKeyIdea.Length > 0)
                {
                    ImageStream = ImageKeyIdea.OpenReadStream();
                    ImageContentType = ImageKeyIdea.ContentType;
                }

                // Tiến hành tạo KeyIdea sau khi xác nhận RecapVersion tồn tại
                var response = await _keyIdeaService.CreatePrepareKeyIdea(request, ImageStream, ImageContentType);
                if (!response.Succeeded)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}"); // Ghi lại chi tiết lỗi
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost("createmultiplekeyideas")]
        public async Task<IActionResult> CreateMultiplePrepareIdeas([FromForm] string requests, List<IFormFile>? imageFiles)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Deserialize JSON từ string sang List<PrepareIdea>
                var prepareIdeas = JsonConvert.DeserializeObject<List<PrepareIdea>>(requests);

                if (prepareIdeas == null || !prepareIdeas.Any())
                {
                    return BadRequest(new { message = "No valid PrepareIdeas found in the request." });
                }

                foreach (var request in prepareIdeas)
                {
                    var versionResponse = await _recapVersionService.GetVersionById(request.RecapVersionId);
                    if (!versionResponse.Succeeded)
                    {
                        return BadRequest(versionResponse);
                    }
                }

                var response = await _keyIdeaService.CreateMultiplePrepareKeyIdeas(prepareIdeas, imageFiles);
                if (!response.Succeeded)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpDelete("delete/{keyIdeaId}")]
        public async Task<IActionResult> DeleteKeyIdea([FromRoute] Guid keyIdeaId)
        {
            var response = await _keyIdeaService.DeleteKeyIdea(keyIdeaId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdelete/{keyIdeaId}")]
        public async Task<IActionResult> SoftDeleteKeyIdea([FromRoute] Guid keyIdeaId)
        {
            var response = await _keyIdeaService.SoftDeleteKeyIdea(keyIdeaId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("getallkeyidea")]
        public async Task<IActionResult> GetAllKeyIdeas()
        {
            var response = await _keyIdeaService.GetAllKeyIdeas();
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("getideabyid/{keyIdeaId}")]
        public async Task<IActionResult> GetKeyIdeaById([FromRoute] Guid keyIdeaId)
        {
            var response = await _keyIdeaService.GetKeyIdeaById(keyIdeaId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPut("updatekeyidea/{id}")]
        public async Task<IActionResult> UpdateKeyIdea(Guid id, [FromForm] UpdateKeyIdeaRequest request, IFormFile? ImageKeyIdea)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Stream imageStream = ImageKeyIdea?.OpenReadStream();

            var response = await _keyIdeaService.UpdateKeyIdea(id, request, imageStream, ImageKeyIdea?.ContentType);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //[HttpPut("updatemultiplekeyideas")]
        //public async Task<IActionResult> UpdateMultipleKeyIdeas([FromForm] List<UpdateKeyIdeaRequest> requests, List<IFormFile>? imageFiles)
        //{
        //    // Kiểm tra xem requests có null hoặc rỗng không
        //    if (requests == null || !requests.Any())
        //    {
        //        return BadRequest(new
        //        {
        //            Succeeded = false,
        //            Message = "Request list cannot be null or empty."
        //        });
        //    }

        //    // Kiểm tra tính hợp lệ của ModelState
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new
        //        {
        //            Succeeded = false,
        //            Message = "Invalid request data.",
        //            Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
        //        });
        //    }

        //    // Gọi service để cập nhật nhiều KeyIdeas
        //    var response = await _keyIdeaService.UpdateMultipleKeyIdeas(requests, imageFiles);

        //    // Kiểm tra phản hồi từ service
        //    if (!response.Succeeded)
        //    {
        //        return BadRequest(new
        //        {
        //            Succeeded = false,
        //            Message = response.Message,
        //            Errors = response.Errors
        //        });
        //    }

        //    // Phản hồi thành công
        //    return Ok(new
        //    {
        //        Succeeded = true,
        //        Message = response.Message,
        //        Data = response.Data
        //    });
        //}




    }
}
