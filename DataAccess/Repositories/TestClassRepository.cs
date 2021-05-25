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
        private static readonly HttpClient Client = new HttpClient();
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

            const string filepath = @"../../../../TestData/50MB.zip";
            const string filename = "50MB.zip";

            FileInfo dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            if (dataRoot.Directory != null)
            {
                string assemblyFolderPath = dataRoot.Directory.FullName;

                string fullPath = Path.Combine(assemblyFolderPath, filepath);

                MultipartFormDataContent content = new MultipartFormDataContent();
                ByteArrayContent fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(fullPath));

                content.Add(fileContent, "file", filename);

                try
                {
                    await Client.PostAsync("https://freshcase.dk/", content);
                    return new ObjectResult("File has been uploaded!") { StatusCode = 200 };
                }
                catch (Exception ex) { 
                    return new ObjectResult("Something went wrong: " + ex.Message) { StatusCode = 500 };
                }
            }
            return new ObjectResult("File not Found!") { StatusCode = 500 };
        }
    }
}
