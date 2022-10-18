using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FuelManagement.Util;

namespace FuelManagement.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public UserController(UserService service, TokenService tokenService)
    {
        _userService = service;
        _tokenService = tokenService;
    }

    [HttpGet("count")]
    public async Task<ActionResult<IEnumerable<Shed>>> GetAll()
    {
        var count = await _userService.GetTotalCount();
        return Ok(count);
    }

    [HttpGet("id/{id}")]
    [Authorize]
    public async Task<ActionResult<Shed>> GetById(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.Id != id) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<ActionResult<User>> GetByEmail(string email)
    {
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.email != email) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        if (user != null && user.password != null)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.password);
            user.password = passwordHash;
        }
        await _userService.CreateAsync(user);
        return Ok(user);
    }

    [HttpPost("checkEmailExists")]
    public async Task<IActionResult> CheckEmailExists(String email)
    {
        var result = await _userService.checkEmailExists(email);
        return Ok(result);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.Id != id) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        // encrypt password
        if (updatedUser != null && updatedUser.password != null)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.password);
            updatedUser.password = passwordHash;
        }

        var queriedShed = await _userService.GetByIdAsync(id);
        if (queriedShed == null)
        {
            return NotFound();
        }
        await _userService.UpdateAsync(id, updatedUser);
        return NoContent();
    }

    [HttpDelete("id")]
    [Authorize]
    public async Task<IActionResult> DeleteById(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.Id != id) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        var shed = await _userService.GetByIdAsync(id);
        if (shed == null)
        {
            return NotFound();
        }
        await _userService.DeleteByIdAsync(id);
        return NoContent();
    }

    [HttpDelete("email")]
    [Authorize]
    public async Task<IActionResult> DeleteByEmail(string email)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        if (authUser.email != email) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        var shed = await _userService.GetByEmailAsync(email);
        if (shed == null)
        {
            return NotFound();
        }
        await _userService.DeleteByEmailAsync(email);
        return NoContent();
    }
}