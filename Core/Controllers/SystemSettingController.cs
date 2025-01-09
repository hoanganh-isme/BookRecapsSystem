using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.SystemSetting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/systemsettings")]
[AllowAnonymous]
public class SystemSettingController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly IMapper _mapper;

    public SystemSettingController(ISystemSettingService systemSettingService, IMapper mapper)
    {
        _systemSettingService = systemSettingService;
        _mapper = mapper;
    }

    // Lấy tất cả cài đặt hệ thống
    [HttpGet("getallsettings")]
    public async Task<IActionResult> GetAllSettings()
    {
        var response = await _systemSettingService.GetAllSystemSettings();
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Lấy thông tin cài đặt theo ID
    [HttpGet("getsettingbyid/{id:guid}")]
    public async Task<IActionResult> GetSettingById(Guid id)
    {
        var response = await _systemSettingService.GetSystemSettingById(id);
        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Tạo mới cài đặt hệ thống
    [HttpPost("createsetting")]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSystemSetting request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _systemSettingService.CreateSystemSetting(request);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    // Cập nhật cài đặt hệ thống
    [HttpPut("updatesetting/{id:guid}")]
    public async Task<IActionResult> UpdateSetting(Guid id, [FromBody] SystemSettingUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _systemSettingService.UpdateSystemSetting(id, request);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Xóa cài đặt hệ thống
    [HttpDelete("deletesetting/{id:guid}")]
    public async Task<IActionResult> DeleteSetting(Guid id)
    {
        var response = await _systemSettingService.DeleteSystemSetting(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    // Soft delete cài đặt hệ thống
    [HttpDelete("softdeletesetting/{id:guid}")]
    public async Task<IActionResult> SoftDeleteSetting(Guid id)
    {
        var response = await _systemSettingService.SoftDeleteSystemSetting(id);
        if (!response.Succeeded)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
