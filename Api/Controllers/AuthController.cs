using DataAccess;
using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        public async Task<ActionResult> Login(String Username, String Password)
        {
            User u = new User(Username, Password);
            try
            {
                ObjectResult result = await _unitOfWork.Users.Login(u) as ObjectResult;
                User user = result.Value as User;
                return Ok(GenerateJWTToken(user));
            }
            catch (Exception ex)
            {
                throw new Exception("Login failed", ex);
            }
        }

        //Opretter JWT token
        private string GenerateJWTToken(User user)
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

            //ToDo add claims with user info!
            //Gør den ikke det i forvejen?
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
        public async Task<ActionResult> Register(string Username, string Password)
        {
            User user = new User(Username, Password);
            try
            {
                return Ok(await _unitOfWork.Users.AddAsync(user));
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        //Sletter en User ud fra Username
        [HttpPost]
        public async Task<ActionResult> Delete(string Username)
        {
            User u = new User(Username);
            int result;

            result = await _unitOfWork.Users.DeleteByQueryAsync(u);
            if (result == 500)
            {
                return StatusCode(500);
            }

            return new StatusCodeResult(200);
        }

        //Opdaterer username på en user
        [HttpPost]
        public async Task<ActionResult> Update(string Username, string NewUsername)
        {
            User u = new User(Username);
            User u1 = new User(NewUsername);

            var result = await _unitOfWork.Users.UpdateByQueryAsync(u, u1);
            if (result != null)
            {
                return StatusCode(500);
            }

            return new StatusCodeResult(200);
        }

        [HttpGet]
        public async Task<ActionResult> GetBy(string Username)
        {
            User u = new User(Username);
            int result;

            result = await _unitOfWork.Users.GetByQueryAsync(u);
            if (result == 500)
            {
                return StatusCode(500);
            }

            return new StatusCodeResult(200);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(List<User> users)
        {
            //List<User> users = new List<User>();
            int result;

            result = await _unitOfWork.Users.GetAll(users);
            if (result == 500)
            {
                return StatusCode(500);
            }

            return new StatusCodeResult(200);
        }
    }
}