using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Playground.Shared.Services;
using Playground.Shared.Models;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ResourceService service = new ResourceService();

        [HttpGet]
        public IEnumerable<Resource> Get()
        {
            return service.GetResources();
        }
    }
}
