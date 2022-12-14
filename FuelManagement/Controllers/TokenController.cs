/*
    Manuka Yasas (IT19133850)
    Token controller will expose apis that can be used to manage all operations related to authentication token (JWT).
    Please go through SwaggerOperation summary in each method to understand its usage
*/

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FuelManagement.Models;
using FuelManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FuelManagement.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserService _service;
        public IConfiguration _configuration;
        public TokenController(IConfiguration config, UserService service)
        {
            _configuration = config;
            _service = service;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (email != null && password != null)
            {
                var user = await _service.GetByEmailAsync(email);

                if (user == null)
                    return BadRequest(new { message = "Invalid email" });

                // verify password
                bool verified = BCrypt.Net.BCrypt.Verify(password, user.password);

                if (!verified)
                    return BadRequest(new { message = "Invalid credentials" });

                //create claims details based on the user information
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserId", user.Id ?? "unknown"),
                        new Claim("DisplayName", user.name ?? "unknown"),
                        new Claim("UserName", user.name ?? "unknown"),
                        new Claim("Email", user.email ?? "unknown"),
                        new Claim("isStationOwner", user.isStationOwner ? "true" : "false")
                    };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: signIn);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}

