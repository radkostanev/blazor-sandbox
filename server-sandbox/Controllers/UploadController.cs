using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Playground.Shared.RemoteServices;
using Playground.Shared.Constants;

namespace server_sandbox.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UploadController : Controller
    {
        internal string UploadPath => Path.Combine(HostingEnvironment?.WebRootPath, UploadConstants.UploadFolderName);

        public IWebHostEnvironment HostingEnvironment { get; set; }

        public IUploadService UploadService { get; set; }

        public UploadController(IWebHostEnvironment hostingEnvironment, IUploadService uploadService)
        {
            HostingEnvironment = hostingEnvironment;

            UploadService = uploadService;
            UploadService.CreateFolder(UploadPath);

            if (UploadService.GetDirectorySizeInMB(UploadPath) > 50)
            {
                UploadService.DeleteFolderContent(UploadPath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(IEnumerable<IFormFile> files)
        {
            await UploadService.SaveFilesAsync(files, UploadPath);

            return Content("");
        }

        [HttpPost]
        public ActionResult Save_Error(IEnumerable<IFormFile> files)
        {
            return Problem("save error occurred");
        }

        [HttpPost]
        public ActionResult Remove(string[] files)
        {
            UploadService.DeleteFiles(files, UploadPath);

            return Content("");
        }

        [HttpPost]
        public ActionResult Remove_Error(string[] files)
        {
            return Problem("remove error occurred");
        }
    }
}
