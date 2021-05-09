using DataAccess.Interfaces;
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
    public class TrainingDataController : BaseController
    {
        public TrainingDataController(IConfiguration configuration, IUnitOfWork unitOfWork) : base(configuration, unitOfWork)
        {
        }

        [HttpGet]
        public async Task<ActionResult> GetNetworkBytesIn()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.NetworkData.GetAllBytesIn(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetNetworkBytesOut()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.NetworkData.GetAllBytesOut(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }

    }
}
