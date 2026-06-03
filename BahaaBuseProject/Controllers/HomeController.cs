using Microsoft.AspNetCore.Mvc;

namespace BahaaBuseProject.Controllers
{
    public class HomeController : Controller
    {
        // main landing page for the site
        public IActionResult Index()
        {
            return View(); // returns the main index.cshtml page
        }
    }
}