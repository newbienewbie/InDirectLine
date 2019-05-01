using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Itminus.InDirectLine.Models;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Itminus.InDirectLine.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;

        // inject a hosting env so that we can get the wwwroot path
        public HomeController(IHostingEnvironment env){
            this._env = env;
        }
        public IActionResult Index(){
            return View();
        }
        public IActionResult GetImages()
        {
            // get the real path of wwwroot/imagesFolder
            var rootDir = this._env.WebRootPath;
            // the extensions allowed to show
            var filters = new String[] { ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp", ".svg" };
            // set the base url = "/"
            var baseUrl = "/";

            
            var imgUrls = Directory.EnumerateFiles(rootDir,"*.*",SearchOption.AllDirectories)
                .Where( fileName => filters.Any(filter => fileName.EndsWith(filter)))
                .Select( fileName => Path.GetRelativePath( rootDir, fileName) ) // get relative path
                .Select ( fileName => Path.Combine(baseUrl, fileName))          // prepend the baseUrl
                .Select( fileName => fileName.Replace("\\","/"))                // replace "\" with "/"
                ;
            return new JsonResult(imgUrls);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
