using Microsoft.AspNetCore.Mvc;

namespace BahaaBuseProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionApiController : ControllerBase
    {
        // get /api/SessionApi → returns visitor's last era + page view count
        [HttpGet]
        public IActionResult Get()
        {
            // get data from the seassion storage or defualt to 0 if noting found
            var lastEra = HttpContext.Session.GetString("visitor_last_era") ?? "0";
            var views   = HttpContext.Session.GetString("visitor_page_views") ?? "0";
            
            // return it as a jason object for the frontend to use
            return Ok(new
            {
                lastEra   = int.TryParse(lastEra, out var e) ? e : 0,
                pageViews = int.TryParse(views,   out var v) ? v : 0
            });
        }

        // post /api/SessionApi/era/{index} → stores current era in session
        [HttpPost("era/{index:int}")]
        public IActionResult SetEra(int index)
        {
            // save the new era index to seassion
            HttpContext.Session.SetString("visitor_last_era", index.ToString());
            
            // count how many times the user switch eras to track activity
            int views = int.TryParse(HttpContext.Session.GetString("visitor_page_views"), out var v) ? v : 0;
            HttpContext.Session.SetString("visitor_page_views", (views + 1).ToString());
            
            return Ok(); // nothing to return just confirm success (hopefully)
        }
    }
}