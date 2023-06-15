using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Playground.Shared.Models;
using Playground.Shared.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("api/urls")]
    public class UrlsController : ControllerBase
    {
        public IWebHostEnvironment WebHostEnvironment { get; set; }
        public string AbsolutePath => WebHostEnvironment.ContentRootPath;
        public BaseAppUrlsService UrlsService { get; set; }
        public List<string> Directories { get; set; }

        public UrlsController(IWebHostEnvironment webHostEnvironment, BaseAppUrlsService urlsService)
        {
            WebHostEnvironment = webHostEnvironment;
            UrlsService = urlsService;

            Directories = Directory.GetDirectories(AbsolutePath, "*Pages", SearchOption.TopDirectoryOnly).ToList();

            // Get Pages from the shared project
            if (WebHostEnvironment.EnvironmentName.Equals("Development"))
            {
                Directories.Add(Path.Combine(AbsolutePath, @"../../", "server-sandbox", "Pages"));
                Directories.Add(Path.Combine(AbsolutePath, @"../", "Client", "ClientPages"));
            }
        }

        [HttpGet]
        public List<ComponentDirectory> Get()
        {
            return UrlsService.GetDirectories(Directories);
        }
    }
}
