using DataAccess.Entities.Test;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ITestClass : IGenericRepository<TestClass>
    {
        Task<ActionResult> TestBytesIn50mb();
        Task<ActionResult> TestBytesIn20mb();

    }
}