using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;

namespace Api.Controllers.v1
{
    public class BaseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            UnitOfWork = unitOfWork;
        }

        protected IUnitOfWork UnitOfWork { get; }

        protected ActionResult CatchResponse(Exception ex, string message, int statusCode)
        {
            return new ObjectResult(message) { StatusCode = statusCode };
        }

        protected ActionResult ReturnResponse(dynamic json, int statusCode)
        {
            return new ObjectResult(JsonSerializer.Serialize(json)) { StatusCode = statusCode };
        }
    }
}