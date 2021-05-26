using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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
                return ReturnResponse(await UnitOfWork.TestClass.TestBytesIn(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }
    }
}