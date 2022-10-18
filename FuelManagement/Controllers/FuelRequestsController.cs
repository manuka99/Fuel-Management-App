using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FuelManagement.Util;

namespace FuelManagement.Controllers;

[Route("api/fuel-request")]
[ApiController]
public class FuelRequestsController : ControllerBase
{
    private readonly FuelRequestService _fuelRequestService;
    private readonly TokenService _tokenService;

    public FuelRequestsController(FuelRequestService service, TokenService tokenService)
    {
        _fuelRequestService = service;
        _tokenService = tokenService;
    }

    [HttpGet("next-token/{id}")]
    public async Task<ActionResult<IEnumerable<FuelRequests>>> GetNextTokenIdtAll(string id)
    {
        var token = await _fuelRequestService.GetNextTokenId(id);
        return Ok(token + 1);
    }

    [HttpGet("id/{id}")]
    public async Task<ActionResult<FuelRequests>> GetById(string id)
    {
        var user = await _fuelRequestService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet("shed/{id}")]
    [Authorize]
    public async Task<ActionResult<FuelRequests>> GetByShedId(string id)
    {
        var fuelRequest = await _fuelRequestService.GetByShedAsync(id);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [HttpGet("shed-user/{id}")]
    [Authorize]
    public async Task<ActionResult<FuelRequests>> GetByRequestsOfUser(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var fuelRequest = await _fuelRequestService.GetByShedUserAsync(id, authUser.Id);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(FuelRequests fuelRequest)
    {
        if (fuelRequest != null && fuelRequest.shed != null && fuelRequest.fuelType != null && fuelRequest.fuelQTY != null)
        {
            // validate user is authorized to make changes
            var authUser = _tokenService.getAuthUser(HttpContext);
            fuelRequest.user = authUser.Id;
            fuelRequest.isApproved = true;
            fuelRequest.isCompleted = false;
            await _fuelRequestService.CreateAsync(fuelRequest);
            return Ok(fuelRequest);
        }
        return BadRequest("Required parameters were missing");
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(string id, FuelRequests fuelRequest)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var queriedRequest = await _fuelRequestService.GetByIdAsync(id);
        if (queriedRequest == null)
        {
            return NotFound();
        }

        if (authUser.Id != queriedRequest.user) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        fuelRequest.Id = id;

        await _fuelRequestService.UpdateAsync(id, fuelRequest);
        return NoContent();
    }

    [HttpDelete("id")]
    [Authorize]
    public async Task<IActionResult> DeleteById(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var fuelRequest = await _fuelRequestService.GetByIdAsync(id);
        if (fuelRequest == null)
        {
            return NotFound();
        }

        if (authUser.Id != fuelRequest.user) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        await _fuelRequestService.DeleteByIdAsync(id);
        return NoContent();
    }
}