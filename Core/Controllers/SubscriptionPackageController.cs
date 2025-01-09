using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.SubscriptionPackage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/subscriptionpackages")]
public class SubscriptionPackageController : ControllerBase
{
    private readonly ISubscriptionPackageService _subscriptionPackageService;
    private readonly IMapper _mapper;

    public SubscriptionPackageController(ISubscriptionPackageService subscriptionPackageService, IMapper mapper)
    {
        _subscriptionPackageService = subscriptionPackageService;
        _mapper = mapper;
    }

    // Lấy tất cả gói đăng ký
    [HttpGet("getallpackages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPackages()
    {
        var response = await _subscriptionPackageService.GetAllSubscriptionPackages();
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Lấy thông tin gói đăng ký theo ID
    [HttpGet("getpackagebyid/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackageById(Guid id)
    {
        var response = await _subscriptionPackageService.GetSubscriptionPackageById(id);
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Tạo mới gói đăng ký
    [HttpPost("createpackage")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> CreatePackage([FromBody] CreateSubscriptionPackage request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _subscriptionPackageService.CreateSubscriptionPackage(request);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    // Cập nhật gói đăng ký
    [HttpPut("updatepackage/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Staff")]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdateSubscriptionPackage request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _subscriptionPackageService.UpdateSubscriptionPackage(id, request);
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Xóa gói đăng ký
    [HttpDelete("deletepackage/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Staff")]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var response = await _subscriptionPackageService.DeleteSubscriptionPackage(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    // Soft delete gói đăng ký
    [HttpDelete("softdeletepackage/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Staff")]
    public async Task<IActionResult> SoftDeletePackage(Guid id)
    {
        var response = await _subscriptionPackageService.SoftDeleteSubscriptionPackage(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}

