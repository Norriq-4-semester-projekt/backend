using DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;

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
        [Authorize]
        public ContentResult Spike()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = GetAbsolutePath(@"../../../../TestData/spike.html")
            };
        }

        [HttpGet]
        public ContentResult Prediction()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = GetAbsolutePath(@"../../../../TestData/prediction.html")
            };
        }

        private string GetAbsolutePath(string relativePath)
        {
            string filepath = relativePath;

            FileInfo dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, filepath);

            var fileContent = System.IO.File.ReadAllText(fullPath);

            return fileContent;
        }
    }
}
