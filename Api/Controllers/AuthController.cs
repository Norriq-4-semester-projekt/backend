using Api.Helpers;
using Api.Models.v1_0;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1.0")] // Set version of controller
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration Configuration;

        public AuthController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public string Login()
        {
            return this.GenerateJWTToken(); ;
        }

        private string GenerateJWTToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtToken:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = Configuration["JwtToken:Issuer"];
            var audience = Configuration["JwtToken:Audience"];
            var timeToLive = DateTime.Now.AddMinutes(Convert.ToDouble(Configuration["JwtToken:TokenExpiry"]));

            //ToDo add claims with user info!
            var token = new JwtSecurityToken(
            issuer,
            audience,
            expires: timeToLive,
            signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost]
        public StatusCodeResult Register(string Username, string Password)
        {
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users*");
            var client = new ElasticClient(settings);

            try
            {
                var rs = client.Search<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(Username))));
                if(rs.Hits.Count > 0)
                {
                    return new StatusCodeResult(500);
                }
                
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }

            User u = new User(Username);
            u.Salt = PasswordHelper.GenerateSalt();
            u.PasswordHash = PasswordHelper.ComputeHash(Password, u.Salt);

            try
            {
                client.IndexDocument<User>(u);

                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }
    }
}
