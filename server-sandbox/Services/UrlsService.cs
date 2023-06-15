using Microsoft.AspNetCore.Hosting;
using Playground.Shared.Models;
using Playground.Shared.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace server_sandbox.Services
{
    public class UrlsService
    {
        public IWebHostEnvironment HostingEnvironment { get; set; }
        public string AbsolutePath => HostingEnvironment.ContentRootPath;

        public List<string> Directories { get; set; }
        public BaseAppUrlsService BaseAppUrlsService { get; set; }
        
        public UrlsService(IWebHostEnvironment hostingEnvironment, BaseAppUrlsService baseUrlsService)
        {
            HostingEnvironment = hostingEnvironment;
            BaseAppUrlsService = baseUrlsService;

            Directories = Directory.GetDirectories(AbsolutePath, "*Pages", SearchOption.TopDirectoryOnly).ToList();
        }

        public List<ComponentDirectory> GetComponentDirectories()
        {
            return BaseAppUrlsService.GetDirectories(Directories);
        }
    }
}
