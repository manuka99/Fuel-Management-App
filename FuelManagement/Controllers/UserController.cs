using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FuelManagement.Util;
using Swashbuckle.AspNetCore.Annotations;

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

    [SwaggerOperation(Summary = "Get the total number of users")]
    [HttpGet("count")]
    public async Task<ActionResult<IEnumerable<Shed>>> GetAll()
    {
        var count = await _userService.GetTotalCount();
        return Ok(count);
    }

    [SwaggerOperation(Summary = "Get user by id")]
    [HttpGet("id")]
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

    [SwaggerOperation(Summary = "Get user by email")]
    [HttpGet("email")]
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

    [SwaggerOperation(Summary = "Create a new user profile with unique email")]
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

    [SwaggerOperation(Summary = "Check if email exists")]
    [HttpPost("email-status")]
    public async Task<IActionResult> CheckEmailExists(String email)
    {
        var result = await _userService.checkEmailExists(email);
        return Ok(new {accountExists = result});
    }

    [SwaggerOperation(Summary = "update user profile")]
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

    [SwaggerOperation(Summary = "delete user profile by id")]
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

    [SwaggerOperation(Summary = "update user profile by email")]
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