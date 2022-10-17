using System;
using System.Security.Claims;
using FuelManagement.Models;
using MongoDB.Driver;

namespace FuelManagement.Services
{
    public class TokenService
    {
        public User getAuthUser(HttpContext httpContext)
        {
            User authUser = new User();
            List<Claim> roleClaims = httpContext.User.Claims.ToList();
            foreach (Claim claim in roleClaims)
            {
                switch (claim.Type)
                {
                    case "UserId":
                        authUser.Id = claim.Value;
                        break;
                    case "UserName":
                        authUser.name = claim.Value;
                        break;
                    case "Email":
                        authUser.email = claim.Value;
                        break;
                    default:
                        break;
                }
            }
            return authUser;
        }
    } 
}

