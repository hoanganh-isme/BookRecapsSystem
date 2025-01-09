using AutoMapper;
using BusinessObject.ViewModels.ContractAttachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/contract-attachment")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ContractAttachmentController : ControllerBase
    {
        private readonly IContractAttachmentService _contractAttachmentService;
        private readonly IMapper _mapper;

        public ContractAttachmentController(IContractAttachmentService contractAttachmentService, IMapper mapper)
        {
            _contractAttachmentService = contractAttachmentService;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllContractAttachments()
        {
            var response = await _contractAttachmentService.GetAllContractAttachments();
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("getallattachmentbycontractid/{contractid}")]
        public async Task<IActionResult> GetAllContractAttachmentsByContractId(Guid contractid)
        {
            var response = await _contractAttachmentService.GetAllContractAttachmentsByContractId(contractid);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("getcontractattachmentbyid/{id:guid}")]
        public async Task<IActionResult> GetContractAttachmentById(Guid id)
        {
            var response = await _contractAttachmentService.GetContractAttachmentById(id);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("createcontractattachment")]
        public async Task<IActionResult> CreateContractAttachment([FromForm] CreateContractAttachment request, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string fileExtension = Path.GetExtension(file.FileName);
                Stream fileStream = file.OpenReadStream();

                var response = await _contractAttachmentService.CreateContractAttachment(request, fileStream, fileExtension);
                if (!response.Succeeded)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("update/{id:guid}")]
        public async Task<IActionResult> UpdateContractAttachment(Guid id, [FromForm] UpdateContractAttachment request, IFormFile newFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string fileExtension = newFile != null ? Path.GetExtension(newFile.FileName) : null;
                Stream newFileStream = newFile?.OpenReadStream();

                var response = await _contractAttachmentService.UpdateContractAttachment(id, request, newFileStream, fileExtension);
                if (!response.Succeeded)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteContractAttachment(Guid id)
        {
            var response = await _contractAttachmentService.DeleteContractAttachment(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("softdelete/{id:guid}")]
        public async Task<IActionResult> SoftDeleteContractAttachment(Guid id)
        {
            var response = await _contractAttachmentService.SoftDeleteContractAttachment(id);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
