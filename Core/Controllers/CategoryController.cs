using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



[ApiController]
[Route("api/category")]
[AllowAnonymous]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoryController(ICategoryService categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    [HttpGet("getallcategory")]
    public async Task<IActionResult> GetAllCategories()
    {
        var response = await _categoryService.GetAllCategory();
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpGet("getcategorybyid/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var response = await _categoryService.GetCategoryById(id);
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpPost("createcategory")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var response = await _categoryService.CreateCategory(request);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPut("updatecategory/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.Id = id;

        // Gọi service để cập nhật danh mục trực tiếp với request
        var response = await _categoryService.UpdateCategory(request);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }

        return Ok(response);
    }


    [HttpDelete("deletecategory/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var response = await _categoryService.DeleteCategory(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
    [HttpDelete("softdeletecategory/{id:guid}")]
    public async Task<IActionResult> SoftDeleteCategory(Guid id)
    {
        var response = await _categoryService.SoftDeleteCategory(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
