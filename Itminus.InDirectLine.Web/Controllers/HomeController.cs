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
        public IActionResult Index(){
            return new JsonResult("InDirectLine Started"); 
        }

    }
}
