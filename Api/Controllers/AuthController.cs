using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/[action]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult> Login(string username, string password)
        {
            User u = new User(username, password);
            try
            {
                ObjectResult result = await _unitOfWork.Users.Login(u) as ObjectResult;
                User user = result?.Value as User;
                return Ok(GenerateJwtToken(user));
            }
            catch (Exception ex)
            {
                throw new Exception("Login failed", ex);
            }
        }

        //Opretter JWT token
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtToken:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _configuration["JwtToken:Issuer"];
            var audience = _configuration["JwtToken:Audience"];
            var timeToLive = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtToken:TokenExpiry"]));
            var claims = new List<Claim>
            {
                new Claim("username", user.Username),
                new Claim("LoggedOn", DateTime.Now.ToString())
            };
            
            var token = new JwtSecurityToken(
            issuer,
            audience,
            expires: timeToLive,
            signingCredentials: credentials,
            claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        public async Task<ActionResult> Register(string username, string password)
        {
            User user = new User(username, password);
            try
            {
                return await _unitOfWork.Users.AddAsync(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Register failed", ex);
            }
        }

        //Sletter en User ud fra Username
        [HttpPost]
        public async Task<ActionResult> Delete(string username)
        {
            User u = new User(username);
            try
            {
                return await _unitOfWork.Users.DeleteByQueryAsync(u);
            }
            catch (Exception ex)
            {
                throw new Exception("Delete Failed", ex);
            }
        }

        //Opdaterer username på en user
        [HttpPost]
        public async Task<ActionResult> Update(string username, string newUsername)
        {
            User u = new User(username);
            User u1 = new User(newUsername);
            try
            {
                return await _unitOfWork.Users.UpdateByQueryAsync(u, u1);
            }
            catch (Exception ex)
            {
                throw new Exception("Update Failed", ex);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                return new ObjectResult(JsonSerializer.Serialize(await _unitOfWork.Users.GetAll())) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                throw new Exception("Connection failed", ex);
            }
        }
    }
}