﻿using DataAccess;
using DataAccess.Entities;
using DataAccess.Interfaces;
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
            //Opretter en forbindelse til Elastic
            User u = new User(Username);
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            settings.BasicAuthentication("elastic", "changeme");
            var client = new ElasticClient(settings);

            try
            {
                //Check om useren eksisterer ud fra username
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(Username))));
                if (rs.Hits.Count > 0)
                {
                    foreach (var hit in rs.Hits)
                    {
                        //Checker at password er rigtigt
                        u = hit.Source;
                        if (PasswordHelper.ComparePass(Password, u.PasswordHash, u.Salt))
                        {
                            return Ok(GenerateJWTToken(u));
                        }
                    }
                }
                return new StatusCodeResult(500);
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
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

        //Register burde kalde Add() fra UserRepository, men lige nu har den sin egen implementation
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

            //Forbindelse til Elastic
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            settings.BasicAuthentication("elastic", "changeme");
            var client = new ElasticClient(settings);

            try
            {
                //Finder User ud fra username
                var rs = client.Search<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(Username))));

                if (rs.Hits.Count > 0)
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
            //Opdaterer User informationer. 
            //Opdaterer ikke _id???
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