using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ObjectResultExecption : Exception
    {
        public ObjectResultExecption(ObjectResult objectResult)
         : base(objectResult.Value.ToString()) { }
    }
}