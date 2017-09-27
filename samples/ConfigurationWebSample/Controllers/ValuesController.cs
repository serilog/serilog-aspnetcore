using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace SimpleWebSample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            // Directly through Serilog
            Log.Information("This is a handler for {Path}", Request.Path);
            return new string[] { "value1", "value2" };
        }
    }
}
