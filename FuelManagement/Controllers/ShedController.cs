using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FuelManagement.Util;

namespace FuelManagement.Controllers;

[Route("api/station")]
[ApiController]
public class ShedController : ControllerBase
{
    private readonly ShedService _shedService;
    private readonly TokenService _tokenService;

    public ShedController(ShedService service, TokenService tokenService)
    {
        _shedService = service;
        _tokenService = tokenService;
    }

    [HttpGet("/search/{text}")]
    public async Task<ActionResult<IEnumerable<Shed>>> Search(string text)
    {
        var sheds = await _shedService.search(text);
        return Ok(sheds);
    }

    [HttpGet("/all")]
    public async Task<ActionResult<IEnumerable<Shed>>> GetAll()
    {
        var sheds = await _shedService.GetAllAsync();
        return Ok(sheds);
    }

    [HttpGet("/{id}")]
    public async Task<ActionResult<Shed>> GetById(string id)
    {
        var shed = await _shedService.GetByIdAsync(id);
        if (shed == null)
        {
            return NotFound();
        }
        return Ok(shed);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Shed shed)
    {
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser == null || authUser.Id == null) return BadRequest("Not authorized");
        shed.owner = authUser.Id;
        await _shedService.CreateAsync(shed);
        return Ok(shed);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(string id, Shed updatedShed)
    {
        var queriedShed = await _shedService.GetByIdAsync(id);
        if (queriedShed == null)
        {
            return NotFound();
        }

        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.Id != queriedShed.owner) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        updatedShed.Id = id;
        updatedShed.owner = authUser.Id;
        await _shedService.UpdateAsync(id, updatedShed);
        return NoContent();
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var shed = await _shedService.GetByIdAsync(id);
        if (shed == null)
        {
            return NotFound();
        }

        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.Id != shed.owner) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        await _shedService.DeleteAsync(id);
        return NoContent();
    }
}