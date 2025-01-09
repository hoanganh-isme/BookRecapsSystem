using BusinessObject.ViewModels.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ContractController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpPost("create")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateContract([FromBody] CreateContract request)
    {
        var result = await _contractService.CreateContract(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
    [HttpPost("createprepare")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> CreatePrepareContract([FromBody] CreatePrepareContract request)
    {
        var result = await _contractService.CreatePrepareContract(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateContract(Guid id, [FromBody] UpdateContract request)
    {
        var result = await _contractService.UpdateContract(id, request);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPut("change-status/{id}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> ChangeStatusContract(Guid id, [FromBody] ChangeStatusContract request)
    {
        var result = await _contractService.ChangeStatusContract(id, request);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpDelete("delete/{id}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteContract(Guid id)
    {
        var result = await _contractService.DeleteContract(id);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpDelete("soft-delete/{id}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> SoftDeleteContract(Guid id)
    {
        var result = await _contractService.SoftDeleteContract(id);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getcontractby/{id}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Publisher")]
    public async Task<IActionResult> GetContractById(Guid id)
    {
        var result = await _contractService.GetContractById(id);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    [HttpGet("getcontractbypublisher/{publisherId}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Publisher")]
    public async Task<IActionResult> GetContractByPublisherId(Guid publisherId)
    {
        var result = await _contractService.GetContractByPublisherId(publisherId);
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    [HttpGet("getallcontract")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin, Publisher")]
    public async Task<IActionResult> GetAllContracts()
    {
        var result = await _contractService.GetAllContracts();
        if (result.Succeeded)
        {
            return Ok(result);
        }
        return NotFound(result);
    }
    [HttpPost("addattachment/{contractId}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> AddAttachmentToContract(Guid contractId, [FromBody] AddContractAttachment attachment)
    {
        var result = await _contractService.AddContractAttachment(contractId, attachment);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    [HttpPut("addbooktocontract/{contractId}")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
    public async Task<IActionResult> AddBooksToContract(Guid contractId, [FromBody] AddBookToContract addBookToContract, Guid publisherId)
    {
        var result = await _contractService.AddOrUpdateBooksInContract(contractId, addBookToContract, publisherId);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

}
