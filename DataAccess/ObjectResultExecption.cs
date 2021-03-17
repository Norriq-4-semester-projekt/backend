using Microsoft.AspNetCore.Mvc;
using System;

namespace DataAccess
{
    public class ObjectResultExecption : Exception
    {
        public ObjectResultExecption(ObjectResult objectResult)
         : base(objectResult.Value.ToString()) { }
    }
}