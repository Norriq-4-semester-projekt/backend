﻿using Api.Helpers;
using Api.Models.v1_0;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories;
using DataAccess;


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
           
            User u = new User(Username);
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            var client = new ElasticClient(settings);

            try
            {
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(Username))));
                if (rs.Hits.Count > 0)
                {
                    foreach (var hit in rs.Hits)
                    {
                        User us = hit.Source;
                        if (PasswordHelper.ComparePass(Password, us.PasswordHash, us.Salt))
                        {
                            return Ok(GenerateJWTToken(us));
                        }
                    }
                    //foreach (IHit<User> user in rs.Hits)
                    //{
                    //    User us = user as User;
                    //    if (PasswordHelper.ComparePass(Password, us.PasswordHash, us.Salt))
                    //    {
                    //        return Ok(GenerateJWTToken(us));
                    //    }
                    //}
                }
                return new StatusCodeResult(500);
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

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
        public ActionResult Register(string Username, string Password)
        {
            UserValidator uv = new UserValidator();
            User user = new User(Username);
            user.Password = Password;
            ValidationResult result = uv.Validate(user);
            if (!result.IsValid)
            {
                return new ObjectResult(result.Errors) { StatusCode = 500 };
            }

            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            var client = new ElasticClient(settings);

            try
            {
                var rs = client.Search<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(Username))));

                if (rs.Hits.Count> 0)
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

        [HttpPost]
        public async Task<ActionResult> Delete(string Username)
        {

            User u = new User(Username);
            int result;

           
            result = await _unitOfWork.Users.DeleteByQueryAsync(u);
            if (result == 200)
              {
                return StatusCode(200);
              }
            
            return new StatusCodeResult(200);

        }
        
    }
}




