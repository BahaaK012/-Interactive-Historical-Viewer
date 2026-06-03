using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BahaaBuseProject.Data;

namespace BahaaBuseProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryApiController : ControllerBase
    {
        private readonly HistoryContext _context;
        public HistoryApiController(HistoryContext context) { _context = context; }

        /* get all the eras data for the website to show */
        [HttpGet]
        public async Task<IActionResult> GetEras()
        {
            /* fetch all eras with all nested data and keep them in order */
            var eras = await _context.Eras
                .OrderBy(e => e.Id)                        // consistent order on every machine
                .Include(e => e.Figures.OrderBy(f => f.Id))
                .Include(e => e.Cities.OrderBy(c => c.Id))
                .Include(e => e.Quotes.OrderBy(q => q.Id))
                .Include(e => e.Videos.OrderBy(v => v.Id))
                .Include(e => e.Sources.OrderBy(s => s.Id))
                .Include(e => e.QuizQuestions.OrderBy(q => q.Id))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Id))
                .ToListAsync();
            
            /* return all data as jason for the script.js to use */
            return Ok(eras);
        }
    }
}