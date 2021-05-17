using DataAccess.Entities.Test;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class TestClassRepository : ITestClass
    {
        private static readonly HttpClient client = new HttpClient();
        public Task<ActionResult> AddAsync(TestClass entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(TestClass entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TestClass>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> UpdateByQueryAsync(TestClass entity, TestClass u1)
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult> TestBytesIn()
        {

            string filepath = @"../../../../TestData/50MB.zip";
            string filename = "50MB.zip";

            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, filepath);

            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fullPath));

            content.Add(fileContent, "file", filename);

            try
            {
                HttpResponseMessage response = await client.PostAsync("https://freshcase.dk/", content);
                return new ObjectResult("File has been uploaded!") { StatusCode = 200 };
            }
            catch (Exception ex) { 
                return new ObjectResult("Something went wrong: " + ex.Message) { StatusCode = 500 };
            }
        }
    }
}
