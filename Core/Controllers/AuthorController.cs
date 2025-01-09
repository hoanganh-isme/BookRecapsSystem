using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Author;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/authors")]
[AllowAnonymous]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly IMapper _mapper;

    public AuthorController(IAuthorService authorService, IMapper mapper)
    {
        _authorService = authorService;
        _mapper = mapper;
    }

    // Lấy tất cả tác giả
    [HttpGet("getallauthors")]
    public async Task<IActionResult> GetAllAuthors()
    {
        var response = await _authorService.GetAllAuthors();
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Lấy thông tin tác giả theo ID
    [HttpGet("getauthorbyid/{id:guid}")]
    public async Task<IActionResult> GetAuthorById(Guid id)
    {
        var response = await _authorService.GetAuthorById(id);
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Tạo mới tác giả
    [HttpPost("createauthor")]
    public async Task<IActionResult> CreateAuthor([FromForm] AuthorCreateRequest request, IFormFile? ImageAuthor)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        Stream ImageStream = null;
        string ImageContentType = null;

        // Kiểm tra và lấy dữ liệu từ ảnh nếu có
        if (ImageAuthor != null && ImageAuthor.Length > 0)
        {
            ImageStream = ImageAuthor.OpenReadStream();
            ImageContentType = ImageAuthor.ContentType;
        }
        
        var response = await _authorService.CreateAuthor(request, ImageStream, ImageContentType);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    // Cập nhật tác giả
    [HttpPut("updateauthor/{id:guid}")]
    public async Task<IActionResult> UpdateAuthor(Guid id, [FromForm] AuthorUpdateRequest request, IFormFile image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.Id = id;
        Stream imageStream = image?.OpenReadStream();

        var response = await _authorService.UpdateAuthor(request, imageStream, image?.ContentType);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    // Xóa tác giả
    [HttpDelete("deleteauthor/{id:guid}")]
    public async Task<IActionResult> DeleteAuthor(Guid id)
    {
        var response = await _authorService.DeleteAuthor(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
    [HttpDelete("softdeleteauthor/{id:guid}")]
    public async Task<IActionResult> SoftDeleteAuthor(Guid id)
    {
        var response = await _authorService.SoftDeleteAuthor(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
