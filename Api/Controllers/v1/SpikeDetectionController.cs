using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SpikeDetectionController> _logger;

        public SpikeDetectionController(IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<SpikeDetectionController> logger) : base(configuration, unitOfWork)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestNetworkBytesIn()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.NetworkData.GetLatestBytesIn(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestNetworkBytesOut()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.NetworkData.GetLatestBytesOut(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestPacketsIn()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.NetworkData.GetLatestPacketsIn(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestPacketsOut()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.NetworkData.GetLatestPacketsOut(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestCpuData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.CpuData.GetLatest(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestMemoryData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.MemoryData.GetLatest(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestSystemLoadData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.SystemLoadData.GetLatest(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
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

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "noget skidt skete");
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostChangepointData(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogChangepointData(data);
                if (isValid)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpPost]
        public ActionResult PostPredictionData(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogPredictionData(data);
                if (isValid)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpPost]
        public ActionResult PostPredictionSystemLoad(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogPredictionSystemLoad(data);
                if (isValid)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpPost]
        public ActionResult PostPredictionCpupctTime(Data data)
        {
            try
            {
                bool isValid = UnitOfWork.DetectionLogging.LogPredictionDataCpupctTime(data);
                if (isValid)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetChangepointGraphData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.DetectionLogging.GetAllChangepoints(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetGraphData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.DetectionLogging.GetAll(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPredictionGraphData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.DetectionLogging.GetAllPredictions(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetSystem()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.DetectionLogging.GetAllSystemLoadPredictions(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetPredictionCpuPctTimeGraphData()
        {
            try
            {
                return ReturnResponse(await UnitOfWork.DetectionLogging.GetAllCpupctTimePredictions(), 200);
            }
            catch (Exception ex)
            {
                return CatchResponse(ex, ex.Message, 501);
            }
        }
    }
}