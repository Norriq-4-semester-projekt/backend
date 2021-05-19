using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Controllers.v1
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/[controller]/[action]")]
    public class IndexController : BaseController
    {
        public IndexController(IConfiguration configuration, IUnitOfWork unitOfWork) : base(configuration, unitOfWork)
        {
        }

        [HttpGet]
        public ContentResult spike()
        {
            string filepath = @"../../../../TestData/spike.html";
            string filename = "spike.html";

            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, filepath);

            var fileContent = System.IO.File.ReadAllText(fullPath);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = fileContent
            };
        }

        [HttpGet]
        public ContentResult prediction()
        {
            string filepath = @"../../../../TestData/prediction.html";
            string filename = "prediction.html";

            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, filepath);

            var fileContent = System.IO.File.ReadAllText(fullPath);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = fileContent
            };
        }
    }
}
