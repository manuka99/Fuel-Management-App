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

    [SwaggerOperation(Summary = "Get user profile")]
    [HttpGet("self")]
    [Authorize]
    public async Task<ActionResult<Shed>> GetById()
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var user = await _userService.GetByIdAsync(authUser.Id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    //[SwaggerOperation(Summary = "Get user by email")]
    //[HttpGet("email")]
    //[Authorize]
    //public async Task<ActionResult<User>> GetByEmail(string email)
    //{
    //    var authUser = _tokenService.getAuthUser(HttpContext);
    //    if (authUser.email != email) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

    //    var user = await _userService.GetByEmailAsync(email);
    //    if (user == null)
    //    {
    //        return NotFound();
    //    }
    //    return Ok(user);
    //}

    [SwaggerOperation(Summary = "Create a new user profile with unique email")]
    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        if (user != null && user.password != null)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.password);
            user.password = passwordHash;
        }
        var result = await _userService.CreateAsync(user);
        if (result)
            return Ok();
        else return BadRequest("A user profile exists with the provided email");
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
    public async Task<IActionResult> Update(User updatedUser)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var queriedUser = await _userService.GetByIdAsync(authUser.Id);
        if (queriedUser == null)
        {
            return NotFound();
        }

        // encrypt password
        if (updatedUser != null && updatedUser.password != null)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.password);
            updatedUser.password = passwordHash;
        }
        updatedUser.Id = queriedUser.Id;
        updatedUser.email = queriedUser.email;
        await _userService.UpdateAsync(authUser.Id, updatedUser);
        return Ok();
    }

    [SwaggerOperation(Summary = "delete user profile")]
    [HttpDelete("self")]
    [Authorize]
    public async Task<IActionResult> DeleteById()
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        await _userService.DeleteByIdAsync(authUser.Id);
        return Ok();
    }

    //[SwaggerOperation(Summary = "delete user profile by email")]
    //[HttpDelete("email")]
    //[Authorize]
    //public async Task<IActionResult> DeleteByEmail(string email)
    //{
    //    // validate user is authorized to make changes
    //    var authUser = _tokenService.getAuthUser(HttpContext);
    //    if (authUser.email != email) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

    //    var userFetc = await _userService.GetByEmailAsync(email);
    //    if (userFetc == null)
    //    {
    //        return NotFound();
    //    }
    //    await _userService.DeleteByEmailAsync(email);
    //    return NoContent();
    //}
}