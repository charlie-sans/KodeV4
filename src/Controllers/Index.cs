using Microsoft.AspNetCore.Mvc;
using KodeRunner.Models; // Add this using directive for the model
using System.IO;
using System.Linq;
namespace KodeRunner.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    public class IndexController : Controller
    {
        [HttpGet("/")]
        public IActionResult GetCustom()
        {
            return View("Index");
        }
        [HttpGet("/style.css")]
        public IActionResult GetCSS()
        {
            var contents = System.IO.File.ReadAllText("src/www/Style.css");
            return Content(contents, "text/css");
        }
        [HttpGet("/index.js")]
        public IActionResult GetJS()
        {
            var contents = System.IO.File.ReadAllText("src/www/index.js");
            return Content(contents, "application/javascript");
        }
    }
}
