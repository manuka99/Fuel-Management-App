using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FuelManagement.Util;
using Swashbuckle.AspNetCore.Annotations;

namespace FuelManagement.Controllers;

[Route("api/shed")]
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

    [SwaggerOperation(Summary = "Search for shed with name and city")]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Shed>>> Search(string text)
    {
        var sheds = await _shedService.search(text);
        return Ok(sheds);
    }

    [SwaggerOperation(Summary = "Get all available sheds")]
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Shed>>> GetAll()
    {
        var sheds = await _shedService.GetAllAsync();
        return Ok(sheds);
    }

    [SwaggerOperation(Summary = "Get all available sheds")]
    [HttpGet("owned")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Shed>>> GetOwnedSheds()
    {
        var authUser = _tokenService.getAuthUser(HttpContext);
        var sheds = await _shedService.GetByOwnerAsync(authUser.Id);
        return Ok(sheds);
    }

    [SwaggerOperation(Summary = "Get shed by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Shed>> GetById(string id)
    {
        var shed = await _shedService.GetByIdAsync(id);
        if (shed == null)
        {
            return NotFound();
        }
        return Ok(shed);
    }

    [SwaggerOperation(Summary = "Create a shed, user must have the role shed owner")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Shed shed)
    {
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (!authUser.isStationOwner) return BadRequest("User is not assigned the role of a shed owner, please update your profile");
        shed.owner = authUser.Id;
        await _shedService.CreateAsync(shed);
        return Ok(shed);
    }

    [SwaggerOperation(Summary = "Update shed data")]
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

    [SwaggerOperation(Summary = "Delete a shed")]
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