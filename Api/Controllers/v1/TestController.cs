using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers.v1
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/[controller]/[action]")]
    public class TestController : BaseController
    {
        public TestController(IConfiguration configuration, IUnitOfWork unitOfWork) : base(configuration, unitOfWork)
        {
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestNetworkBytesIn()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.TestClass.TestBytesIn(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
    }
}
