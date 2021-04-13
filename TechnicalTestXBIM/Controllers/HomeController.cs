using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TechnicalTestXBIM.Models;

namespace TechnicalTestXBIM.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileProvider _fileProvider;

        public HomeController(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Files()
        {
            var model = new FilesViewModel();
            foreach (var item in _fileProvider.GetDirectoryContents(""))
            {
                if (IsIfcType(item.PhysicalPath))
                {
                    model.Files.Add(
                        new FileDetails { Name = item.Name, Path = item.PhysicalPath });
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Content("Files not selected");

            foreach (var file in files)
            {
                if (file.Length == 0)
                    return Content("One or more files is empty. Please upload a valid IFC file.");

                var supportedTypes = new[] { "ifc" };
                var fileExt = Path.GetExtension(file.FileName).Substring(1);

                if (!supportedTypes.Contains(fileExt))
                {    
                    return Content("File Extension Is InValid. One or more files are not IFC files - Please only Upload IFC File");                   
                }
            }     

            foreach (var file in files)
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file.GetFilename());

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return RedirectToAction("Files");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private Methods

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".ifc", "application/ifc"},                
            };
        }

        private bool IsIfcType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            if(types.ContainsKey(ext))
                return true;

            return false;
             
        }

        #endregion
    }
}
