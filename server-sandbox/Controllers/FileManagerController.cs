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
using Telerik.Blazor.Components.FileManager;
using System.Text.Json;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;
using System.Net.Mime;
using Microsoft.AspNetCore.StaticFiles;
using Playground.Shared.Models.FileManager;
using Telerik.Blazor.Components.FileManager.Models;

namespace server_sandbox.Controllers
{
    [Route("api/[controller]/[action]")]
    public class FileManagerController : Controller
    {
        const string JpgExtension = ".jpg";
        const string TxtExtension = ".txt";
        const string DocxExtension = ".docx";
        const string PdfExtension = ".pdf";
        const string Folder1 = "folder1";
        const string Folder11 = "folder11";
        const string Folder111 = "folder111";
        const string Folder2 = "folder2";
        const string File0 = "file0";
        const string File11 = "file11";
        const string File111 = "file111";
        const string File1111 = "file1111";
        const string File12 = "file12";
        const string File21 = "file21";
        const string File22 = "file22";
        readonly string PathSeparator = Path.DirectorySeparatorChar.ToString();

        public List<FileManagerEntry<object>> FileSystem { get; set; } = new List<FileManagerEntry<object>>();

        const string FilesCacheKey = "filemanager-files";
        readonly string SampleDownloadFileName = $"{File0}{JpgExtension}";

        internal string UploadPath => Path.Combine(HostingEnvironment?.WebRootPath, FileManagerConstants.UploadFolderName);

        public IWebHostEnvironment HostingEnvironment { get; set; }

        public IUploadService UploadService { get; set; }

        public FileManagerController(IWebHostEnvironment hostingEnvironment, IUploadService uploadService)
        {
            HostingEnvironment = hostingEnvironment;
            UploadService = uploadService;
        }

        [HttpPost]
        //public async Task<List<FileManagerEntry<object>>> Read(string path = null)
        public List<FileManagerEntry<object>> Read(string path = null)
        {
            //await ReadSessionFilesAsync();
            ReadSessionFiles();

            var files = ReadFromFileSystem(path);

            return files;
        }

        [HttpPost]
        public IActionResult Upload(string path = null, List<IFormFile> files = null)
        {
            UploadFiles(path, files);

            return Content("");
        }

        [HttpPost]
        public IActionResult Upload_Error(IEnumerable<IFormFile> files)
        {
            return Problem("upload error occurred");
        }

        [HttpPost]
        public IActionResult UploadHierarchicalFile(string path = null, List<IFormFile> files = null)
        {
            UploadHierarchicalFiles(path, files);

            return Content("");
        }

        [HttpPost]
        public IActionResult Download(string path = null, string fileName = null)
        {
            var file = DownloadFile(path, fileName);
            return file;
        }

        [HttpPost]
        public IActionResult Download_Error(string path = null)
        {
            return Problem("download error occurred");
        }

        [HttpPost]
        public IActionResult DownloadBase64(string path = null)
        {
            var base64 = DownloadFilesAsBase64(path);

            return Content(base64);
        }

        [HttpPost]
        //public async Task<FileManagerEntry<object>> CreateFolder(string path = null, string fileName = null)
        public FileManagerEntry<object> CreateFolder(string path = null, string fileName = null)
        {
            //await ReadSessionFilesAsync();
            ReadSessionFiles();

            var newEntry = new FileManagerEntry<object>()
            {
                Path = string.Join(PathSeparator, path, fileName),
                Name = fileName,
                IsDirectory = true
            };

            var result = CreateFolderInFileSystem(path, newEntry);

            SetFilesToSession(FileSystem);

            return result;
        }

        [HttpPost]
        public IActionResult CreateFolder_Error(string path = null, string fileName = null)
        {
            return Problem("download error occurred");
        }

        [HttpPost]
        //public async Task<IActionResult> Delete(string path, List<string> fileNames)
        public IActionResult Delete(string path, List<string> fileNames)
        {
            ReadSessionFiles();

            DeleteFromFileSystem(path, fileNames);

            SetFilesToSession(FileSystem);

            return Content("");
        }

        [HttpPost]
        public IActionResult Delete_Error(string[] fileNames)
        {
            return Problem("delete error occurred");
        }

        [HttpPost]
        public FileManagerEntry<object> Rename(string path, string fileName, string newFileName)
        {
            ReadSessionFiles();

            var entries = GetEntries(path);
            var existingFile = GetFile(path, fileName);
            var existingDirectory = GetDirectory(path, fileName);
            var existingEntry = existingFile ?? existingDirectory;
            FileManagerEntry<object> existingEntryWithNewName = null;

            if (existingEntry == null)
            {
                return null;
            }

            if (existingEntry.IsDirectory)
            {
                existingEntryWithNewName = entries?.FirstOrDefault(x =>
                    x.IsDirectory && x.Name == newFileName);
            }
            else
            {
                existingEntryWithNewName = entries?.FirstOrDefault(x =>
                    x.IsDirectory == false && x.Path == existingEntry.Path && x.Name == newFileName);
            }

            var newName = newFileName;

            if (existingEntryWithNewName != null && existingEntryWithNewName != existingEntry)
            {
                newName = GenerateUniqueEntryName(path, existingEntryWithNewName);
            }

            existingEntry.Name = newName;

            SetFilesToSession(FileSystem);

            //return Content("");
            return existingEntry;
        }

        [HttpPost]
        public IActionResult Rename_Error(string path, string fileName, string newFileName)
        {
            return Problem("delete error occurred");
        }

        #region Session

        private List<FileManagerEntry<object>> GetFilesFromSession()
        {
            var jsonFiles = HttpContext.Session.GetString(FilesCacheKey);
            var rawFiles = !string.IsNullOrEmpty(jsonFiles) ?
                JsonSerializer.Deserialize<List<FileManagerEntry<object>>>(jsonFiles) :
                null;

            var files = rawFiles != null && rawFiles.Count > 0 ? rawFiles : null;
            return files;
        }

        private void SetFilesToSession(List<FileManagerEntry<object>> files)
        {
            var serializedFiles = JsonSerializer.Serialize(files);
            HttpContext.Session.SetString(FilesCacheKey, serializedFiles);
        }

        // read files, but store a copy of them in the session
        // so that one user does not delete all files to all other users
        private async Task ReadSessionFilesAsync()
        {
            var cachedFiles = GetFilesFromSession();

            if (cachedFiles == null)
            {
                // get sample files
                cachedFiles = await GetSampleFilesAsync();

                // allow multiple users of the demos project
                // to work with their own set of files
                SetFilesToSession(cachedFiles);
            }

            FileSystem = cachedFiles;
        }

        // read files, but store a copy of them in the session
        // so that one user does not delete all files to all other users
        private void ReadSessionFiles()
        {
            var cachedFiles = GetFilesFromSession();

            if (cachedFiles == null)
            {
                // get sample files
                cachedFiles = GetSampleFiles();

                // allow multiple users of the demos project
                // to work with their own set of files
                SetFilesToSession(cachedFiles);
            }

            FileSystem = cachedFiles;
        }

        #endregion Session

        #region File system

        public List<FileManagerEntry<object>> GetFileSystem()
        {
            #region folder 1 config

            var folder1 = new FileManagerEntry<object>()
            {
                Name = Folder1,
                IsDirectory = true,
                HasDirectories = true,
                //HasDirectories = false,
                DateCreated = new DateTime(2020, 1, 2),
                DateCreatedUtc = new DateTime(2020, 1, 2),
                DateModified = new DateTime(2020, 2, 3),
                DateModifiedUtc = new DateTime(2020, 2, 3),
                Path = Folder1,
                //Path = null,
                Extension = null,
                Size = 1024
            };

            var folder11 = new FileManagerEntry<object>()
            {
                Name = Folder11,
                IsDirectory = true,
                HasDirectories = true,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = string.Join(PathSeparator, Folder1, Folder11),
                //Path = string.Join(PathSeparator, Folder1),
                Extension = null,
                Size = 1024 * 1024
            };

            var file11 = new FileManagerEntry<object>()
            {
                Name = File11,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = Folder1,
                Extension = JpgExtension,
                Size = 1024 * 1024 * 1024
            };

            var file12 = new FileManagerEntry<object>()
            {
                Name = File12,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = Folder1,
                Extension = PdfExtension,
                Size = 1024 * 1024 * 1024
            };

            folder1.Items.Add(folder11);
            folder1.Items.Add(file11);
            folder1.Items.Add(file12);

            var folder111 = new FileManagerEntry<object>()
            {
                Name = Folder111,
                IsDirectory = true,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = string.Join(PathSeparator, Folder1, Folder11, Folder111),
                //Path = string.Join(PathSeparator, Folder1, Folder11),
                Extension = null,
                Size = 1024 * 1024 * 1024
            };

            var file111 = new FileManagerEntry<object>()
            {
                Name = File111,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = string.Join(PathSeparator, Folder1, Folder11),
                Extension = PdfExtension,
                Size = 1024 * 1024 * 1024
            };

            folder11.Items.Add(folder111);
            folder11.Items.Add(file111);

            var file1111 = new FileManagerEntry<object>()
            {
                Name = File1111,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = string.Join(PathSeparator, Folder1, Folder11, Folder111),
                Extension = PdfExtension,
                Size = 1
            };

            folder111.Items.Add(file1111);

            #endregion folder 1 config

            #region folder 2 config

            var folder2 = new FileManagerEntry<object>()
            {
                Name = Folder2,
                IsDirectory = true,
                HasDirectories = false,
                DateCreated = new DateTime(2022, 3, 4),
                DateCreatedUtc = new DateTime(2022, 3, 4),
                DateModified = new DateTime(2022, 5, 6),
                DateModifiedUtc = new DateTime(2022, 5, 6),
                Path = Folder2,
                //Path = null,
                Extension = null,
                Size = 1024 * 1024
            };

            var file21 = new FileManagerEntry<object>()
            {
                Name = File21,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = Folder2,
                Extension = TxtExtension,
                Size = Int32.MaxValue
            };

            var file22 = new FileManagerEntry<object>()
            {
                Name = File22,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2021, 1, 2),
                DateCreatedUtc = new DateTime(2021, 1, 2),
                DateModified = new DateTime(2021, 2, 3),
                DateModifiedUtc = new DateTime(2021, 2, 3),
                Path = Folder2,
                Extension = DocxExtension,
                Size = 2
            };

            folder2.Items.Add(file21);
            folder2.Items.Add(file22);

            #endregion folder 2 config

            #region root dir

            var file0 = new FileManagerEntry<object>()
            {
                Name = File0,
                IsDirectory = false,
                HasDirectories = false,
                DateCreated = new DateTime(2020, 1, 2),
                DateCreatedUtc = new DateTime(2020, 1, 2),
                DateModified = new DateTime(2020, 2, 3),
                DateModifiedUtc = new DateTime(2020, 2, 3),
                Path = null,
                Extension = JpgExtension,
                Size = 3 * 1024 * 1024
            };

            var folders = new List<FileManagerEntry<object>>()
            {
                folder1,
                folder2,
                file0
            };

            #endregion root dir

            return folders;
        }

        private List<FileManagerEntry<object>> GetSampleFiles(string path = null)
        {
            var files = GetFileSystem();

            return files;
        }

        private Task<List<FileManagerEntry<object>>> GetSampleFilesAsync(string path = null)
        {
            var files = GetFileSystem();

            // simulate async operation
            return Task.FromResult(files);
        }

        #endregion File system

        #region Create

        public FileManagerEntry<object> CreateFolderInFileSystem(string path, FileManagerEntry<object> entry)
        {
            var newFolderName = GenerateUniqueEntryName(path, entry);
            entry.Name = newFolderName;
            entry.IsDirectory = true;
            entry.HasDirectories = false;
            entry.DateCreated = DateTime.Now;
            entry.DateCreatedUtc = DateTime.Now;
            entry.DateModified = DateTime.Now;
            entry.DateModifiedUtc = DateTime.Now;
            entry.Path = string.Join(PathSeparator, path, newFolderName);
            entry.Extension = null;

            var parentDirectory = GetDirectory(path);

            if (parentDirectory != null)
            {
                // simulate add in file system
                parentDirectory.Items.Add(entry);
            }
            else
            {
                // create a folder in the root dir
                FileSystem.Add(entry);
            }

            return entry;
        }

        private string GenerateUniqueEntryName(string path, FileManagerEntry<object> entry)
        {
            var uniqueEntryName = entry.Name + entry.Extension;
            var entryNameCounter = 0;
            var parentDirectory = GetDirectory(path);

            if (entry.IsDirectory)
            {
                var existingSubdirectory = GetSubdirectory(parentDirectory, uniqueEntryName);

                while (existingSubdirectory != null)
                {
                    entryNameCounter++;
                    uniqueEntryName = $"{entry.Name} ({entryNameCounter})";
                    existingSubdirectory = GetSubdirectory(parentDirectory, uniqueEntryName);
                }

                return uniqueEntryName;
            }
            else
            {
                // create a new file - not possible by the component UI
                //var existingFile = GetFile(parentDirectory, uniqueEntryName);

                //while (existingFile != null)
                //{
                //    entryNameCounter++;
                //    uniqueEntryName = $"{entry.Name}({entryNameCounter})";
                //    existingFile = GetSubdirectory(parentDirectory, uniqueEntryName);
                //}

                //// remove the extension and use plain name only
                //var uniqueName = uniqueEntryName.Replace($"{entry.Extension}", string.Empty);
                //return uniqueName;
            }

            return uniqueEntryName;
        }

        #endregion Create

        #region Read

        public List<FileManagerEntry<object>> ReadFromFileSystem(string path)
        {
            var entries = GetEntries(path);
            return entries;
        }

        #endregion Read

        #region Upload

        private List<HierarchicalFileEntry> UploadHierarchicalFiles(string path = null, List<IFormFile> filesToUpload = null)
        {
            // save files to file system

            // process the files
            //var files = filesToUpload
            //    .Select(x => new HierarchicalFileEntry()
            //    {
            //        Name = x.FileName,
            //        IsDirectory = false,
            //        HasDirectories = false,
            //        DateCreated = DateTime.Now,
            //        DateCreatedUtc = DateTime.Now,
            //        DateModified = DateTime.Now,
            //        DateModifiedUtc = DateTime.Now,
            //        Path = Path.Combine(path, x.FileName),
            //        Extension = x.FileName.Substring(x.FileName.IndexOf(".")),
            //        Size = x.Length
            //    })
            //    .ToList();

            var files = new List<HierarchicalFileEntry>();

            return files;
        }

        private void UploadFiles(string path = null, List<IFormFile> filesToUpload = null)
        {
            var files = filesToUpload
                .Select(x => new FileManagerEntry<object>()
                {
                    Name = x.FileName,
                    IsDirectory = false,
                    HasDirectories = false,
                    DateCreated = DateTime.Now,
                    DateCreatedUtc = DateTime.Now,
                    DateModified = DateTime.Now,
                    DateModifiedUtc = DateTime.Now,
                    Path = path,
                    Extension = x.FileName.Substring(x.FileName.IndexOf(".")),
                    Size = x.Length
                })
                .ToList();

            UploadFiles(path, files);
        }

        private void UploadFiles(string path = null, List<FileManagerEntry<object>> files = null)
        {
            UploadToFileSystem(path, files);
        }

        public void UploadToFileSystem(string path, List<FileManagerEntry<object>> files)
        {
            FileSystem = GetFilesFromSession();

            var directory = GetDirectory(path);

            if (directory == null)
            {
                // root dir
                FileSystem.AddRange(files);
            }
            else
            {
                directory.Items.AddRange(files);
            }

            SetFilesToSession(FileSystem);
        }

        #endregion Upload

        #region Download

        private FileStreamResult DownloadFile(string path = null, string fileName = null)
        {
            var result = DownloadFromFileSystem(path, fileName);
            return result;
        }

        private string DownloadFilesAsBase64(string path = null)
        {
            var fileName = SampleDownloadFileName;
            var filePath = Path.Combine(
                HostingEnvironment.WebRootPath,
                FileManagerConstants.UploadFolderName,
                fileName);

            var bytes = System.IO.File.ReadAllBytes(filePath);

            var result = Convert.ToBase64String(bytes);

            return result;
        }

        private FileStreamResult DownloadFromFileSystem(string path = null, string fileName = null)
        {
            var sanitizedPath = path ?? string.Empty;
            var fullFilePath = Path.Combine(
                HostingEnvironment.WebRootPath,
                FileManagerConstants.UploadFolderName,
                sanitizedPath,
                fileName);

            if (!System.IO.File.Exists(fullFilePath) || Directory.Exists(fullFilePath))
            {
                // download sample file if the original does not exist
                // for sample purposes only
                // in case the target is a directory, it can be converted
                // to a .zip or something else and then downloaded
                fileName = SampleDownloadFileName;

                fullFilePath = Path.Combine(
                    HostingEnvironment.WebRootPath,
                    FileManagerConstants.UploadFolderName,
                    SampleDownloadFileName);
            }

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
            var result = File(fileStream, contentType, fileName);
            return result;
        }

        #endregion Download

        #region Delete

        public void DeleteFromFileSystem(string path, List<string> fileNames)
        {
            for (int i = 0; i < fileNames.Count; i++)
            {
                var fileName = fileNames[i];
                DeleteFromFileSystem(path, fileName);
            }
        }
        public void DeleteFromFileSystem(string path, string fileName)
        {
            // delete from file system
            //if (Directory.Exists(path))
            //{
            //    Directory.Delete(path, recursive: true);
            //}
            //if (System.IO.File.Exists(physicalPath))
            //{
            //    System.IO.File.Delete(physicalPath);
            //}

            var directory = GetDirectory(path);

            if (directory == null)
            {
                // root dir
                var entry = FileSystem
                    .FirstOrDefault(x => x.Name == fileName || $"{x.Name}{x.Extension}" == fileName);
                FileSystem.Remove(entry);
            }
            else
            {
                var entry = GetFile(directory, fileName);
                directory.Items.Remove(entry);
            }
        }

        #endregion Delete

        #region File system processing

        private List<FileManagerEntry<object>> GetEntries(string path)
        {
            var fileEntries = GetFileEntries(path);
            var directoryEntries = GetDirectoryEntries(path);

            var entries = directoryEntries.Union(fileEntries).Distinct().ToList();

            return entries;
        }

        private List<FileManagerEntry<object>> GetFileEntries(string path)
        {
            var directory = GetDirectory(path);

            if (directory == null)
            {
                // root dir
                var fileEntries = FileSystem
                    .Where(x => x.IsDirectory == false && x.Path == path)
                    .ToList();

                return fileEntries;
            }

            var files = directory?.Items?
                .Where(x => x.IsDirectory == false)
                .ToList();

            return files;
        }

        private List<FileManagerEntry<object>> GetDirectoryEntries(string path)
        {
            var directory = GetDirectory(path);

            if (directory == null)
            {
                // root dir
                var directoryEntries = FileSystem
                    .Where(x => x != null && x.IsDirectory)
                    .ToList();

                return directoryEntries;
            }

            var dirs = directory?.Items?
                .Where(x => x.IsDirectory)
                .ToList();

            return dirs;
        }

        public FileManagerEntry<object> GetFile(FileManagerEntry<object> parentDirectory, string fileName)
        {
            var path = parentDirectory?.Path;
            var file = GetFile(path, fileName);

            return file;
        }

        public FileManagerEntry<object> GetFile(string path, string fileName)
        {
            var parentDirectory = GetDirectory(path);

            if (parentDirectory == null)
            {
                // root dir
                var file = FileSystem
                    .FirstOrDefault(x =>
                        x.IsDirectory == false &&
                        (x.Name == fileName || $"{x.Name}{x.Extension}" == fileName));

                return file;
            }
            else
            {
                var file = parentDirectory?.Items?
                    .FirstOrDefault(x =>
                        x.IsDirectory == false &&
                        (x.Name == fileName || $"{x.Name}{x.Extension}" == fileName));

                return file;
            }
        }

        private IEnumerable<FileManagerEntry<object>> SubdirectoriesSelector(FileManagerEntry<object> entry)
        {
            if (entry != null && entry.IsDirectory)
            {
                var subdiretories = entry.Items?.Where(x => x.IsDirectory);
                return subdiretories;
            }

            return null;
        }

        private FileManagerEntry<object> GetParentDirectory(string path)
        {
            var directory = FileSystem.FirstOrDefault(x => x.IsDirectory && x.Path == path);

            if (directory == null)
            {
                directory = GetSubdirectory(path);
            }

            return directory;
        }

        private FileManagerEntry<object> GetDirectory(string path)
        {
            var directory = FileSystem.FirstOrDefault(x => x.IsDirectory && x.Path == path);

            if (directory == null)
            {
                directory = GetSubdirectory(path);
            }

            return directory;
        }

        private FileManagerEntry<object> GetDirectory(string path, string childDirectoryName)
        {
            var directory = GetDirectory(path);

            if (directory == null)
            {
                // root dir
                var entry = FileSystem.FirstOrDefault(x => x.IsDirectory && x.Name == childDirectoryName);
                return entry;
            }
            else
            {
                var entry = directory.Items.FirstOrDefault(x => x.IsDirectory && x.Name == childDirectoryName);
                return entry;
            }
        }

        private FileManagerEntry<object> GetSubdirectory(string path)
        {
            var subdirectory = FileSystem
                .SelectRecursive(x => SubdirectoriesSelector(x))
                .FirstOrDefault(x => x.IsDirectory && x.Path == path);

            return subdirectory;
        }

        public FileManagerEntry<object> GetSubdirectory(FileManagerEntry<object> parentDirectory, string subdirectoryName)
        {
            if (parentDirectory == null)
            {
                // root dir
                var subdirectory = FileSystem
                    .FirstOrDefault(x => x.IsDirectory && x.Name == subdirectoryName);

                return subdirectory;
            }
            else
            {
                var subdirectory = parentDirectory?.Items?
                    .FirstOrDefault(x => x.IsDirectory && x.Name == subdirectoryName);

                return subdirectory;
            }
        }

        #endregion File system processing
    }
}
