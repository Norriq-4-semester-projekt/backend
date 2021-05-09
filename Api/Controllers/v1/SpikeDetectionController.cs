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
        public async Task<ActionResult> GetLatestNetworkBytesIn()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.NetworkData.GetLatestBytesIn(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetLatestNetworkBytesOut()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.NetworkData.GetLatestBytesOut(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetLatestCpuData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.CpuData.GetLatest(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetLatestMemoryData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.MemoryData.GetLatest(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetLatestSystemLoadData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.SystemLoadData.GetLatest(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
    }
}
