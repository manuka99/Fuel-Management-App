using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace FuelManagement.Controllers;

[Route("api/station")]
[ApiController]
public class ShedController : ControllerBase
{
    private readonly ShedService _shedService;

    public ShedController(ShedService service)
    {
        _shedService = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shed>>> GetAll()
    {
        var sheds = await _shedService.GetAllAsync();
        return Ok(sheds);
    }

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

    [HttpPost]
    public async Task<IActionResult> Create(Shed shed)
    {
        await _shedService.CreateAsync(shed);
        return Ok(shed);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, Shed updatedShed)
    {
        var queriedShed = await _shedService.GetByIdAsync(id);
        if (queriedShed == null)
        {
            return NotFound();
        }
        updatedShed.Id = id;
        await _shedService.UpdateAsync(id, updatedShed);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var shed = await _shedService.GetByIdAsync(id);
        if (shed == null)
        {
            return NotFound();
        }
        await _shedService.DeleteAsync(id);
        return NoContent();
    }
}