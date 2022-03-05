using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkerServiceController : ControllerBase
    {
        private static readonly WorkerService[] WorkerServices = new[]
        {
            new WorkerService("GeoIP Worker Service", "Description of GeoIP Worker Service"),
            new WorkerService("RDAP Worker Service", "Description of RDAP Worker Service")
        };

        private readonly ILogger<WorkerServiceController> _logger;

        public WorkerServiceController(ILogger<WorkerServiceController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WorkerService> Get()
        {
            return WorkerServices;
        }

        [HttpPost]
        public async Task<ActionResult<WorkerService>> Post(Input input)
        {
            if (string.IsNullOrEmpty(input.IPAddress) && string.IsNullOrEmpty(input.DomainAddress))
            {
                return BadRequest();
            }

            return Ok(input);
        }
    }
}
