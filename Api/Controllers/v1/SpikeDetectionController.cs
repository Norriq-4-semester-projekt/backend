using DataAccess.Entities;
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

        [HttpPost]
        public async Task<ActionResult> PostDetectionData(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogDetectionData(data);
                if (isValid)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostPredictionData(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogPredictionData(data);
                if (isValid)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetGraphData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.DetectionLogging.GetAll(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPredictionGraphData()
        {
            try
            {
                return this.ReturnResponse(await UnitOfWork.DetectionLogging.GetAllPredictions(), 200);
            }
            catch (Exception Ex)
            {
                return this.CatchResponse(Ex, Ex.Message, 501);
            }
        }
    }
}