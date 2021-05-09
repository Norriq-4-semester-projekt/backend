using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DataAccess.Interfaces;

namespace Api.Controllers.v1
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/[controller]/[action]")]
    public class SpikeDetectionController : BaseController
    {
        public SpikeDetectionController(IConfiguration configuration, IUnitOfWork unitOfWork) : base(configuration, unitOfWork)
        {
        }

        [HttpGet]
        public async Task<ActionResult> NetworkBytesIn()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.Data.GetLatest(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> CpuCalcData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.CpuCalc.GetLatest(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
    }
}
