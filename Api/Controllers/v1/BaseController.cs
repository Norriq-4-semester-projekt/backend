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
        protected readonly IUnitOfWork _unitOfWork;

        public BaseController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        protected IUnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        protected ActionResult CatchResponse(Exception Ex, string Message, int StatusCode)
        {
            //ToDo Gem evt exception et eller andet sted
            return new ObjectResult(Message) { StatusCode = StatusCode };
        }

        protected ActionResult ReturnResponse(dynamic json, int StatusCode)
        {
            return new ObjectResult(JsonSerializer.Serialize(json)) { StatusCode = StatusCode };
        }
    }
}