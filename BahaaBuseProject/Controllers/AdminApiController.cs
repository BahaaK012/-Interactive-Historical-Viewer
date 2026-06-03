using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BahaaBuseProject.Data;
using BahaaBuseProject.Models;

namespace BahaaBuseProject.Controllers
{
    [Authorize] // only logged in admins can touch these endpointss
    [Route("api/[controller]")]
    [ApiController]
    public class AdminApiController : ControllerBase
    {
        private readonly HistoryContext _db;
        public AdminApiController(HistoryContext db) { _db = db; }

        /* eras */
        [HttpGet("eras")]
        public async Task<IActionResult> GetEras()
        {
            /* fetch all eras with all nested data for the admin to edit */
            var eras = await _db.Eras
                .Include(e => e.Figures).Include(e => e.Cities)
                .Include(e => e.Quotes).Include(e => e.Videos).Include(e => e.Sources)
                .Include(e => e.QuizQuestions).ThenInclude(q => q.Options)
                .ToListAsync();
            return Ok(eras);
        }

        [HttpPost("eras")]
        public async Task<IActionResult> CreateEra([FromBody] Era era)
        {
            /* reset id and setup empty lists so we dont get erors when creating */
            era.Id = 0;
            era.Figures = new List<Figure>(); era.Cities = new List<City>();
            era.Quotes = new List<Quote>(); era.Videos = new List<Video>();
            era.Sources = new List<Source>(); era.QuizQuestions = new List<QuizQuestion>();
            _db.Eras.Add(era);
            await _db.SaveChangesAsync();
            return Ok(era);
        }

        [HttpPut("eras/{id}")]
        public async Task<IActionResult> UpdateEra(int id, [FromBody] Era u)
        {
            var era = await _db.Eras.FindAsync(id);
            if (era == null) return NotFound();
            era.Title = u.Title; era.NodeLabel = u.NodeLabel; era.Description = u.Description;
            era.Color = u.Color; era.BgColor = u.BgColor;
            era.Stat1 = u.Stat1; era.Stat2 = u.Stat2; era.Stat3 = u.Stat3;
            era.SectionIcon = u.SectionIcon; era.SectionBody = u.SectionBody;
            await _db.SaveChangesAsync();
            return Ok(era);
        }

        [HttpDelete("eras/{id}")]
        public async Task<IActionResult> DeleteEra(int id)
        {
            /* need to include all things to remove them all at once */
            var era = await _db.Eras
                .Include(e => e.Figures).Include(e => e.Cities)
                .Include(e => e.Quotes).Include(e => e.Videos).Include(e => e.Sources)
                .Include(e => e.QuizQuestions).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (era == null) return NotFound();
            _db.Eras.Remove(era);
            await _db.SaveChangesAsync();
            return Ok();
        }

        /* figures */
        [HttpPost("figures")]
        public async Task<IActionResult> CreateFigure([FromBody] Figure f) { f.Id=0; _db.Figures.Add(f); await _db.SaveChangesAsync(); return Ok(f); }

        [HttpPut("figures/{id}")]
        public async Task<IActionResult> UpdateFigure(int id, [FromBody] Figure u)
        {
            var f = await _db.Figures.FindAsync(id); if (f==null) return NotFound();
            f.Name=u.Name; f.Bio=u.Bio; f.ImageUrl=u.ImageUrl;
            await _db.SaveChangesAsync(); return Ok(f);
        }

        [HttpDelete("figures/{id}")]
        public async Task<IActionResult> DeleteFigure(int id)
        { var f=await _db.Figures.FindAsync(id); if(f==null) return NotFound(); _db.Figures.Remove(f); await _db.SaveChangesAsync(); return Ok(); }

        /* cities */
        [HttpPost("cities")]
        public async Task<IActionResult> CreateCity([FromBody] City c) { c.Id=0; _db.Cities.Add(c); await _db.SaveChangesAsync(); return Ok(c); }

        [HttpPut("cities/{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromBody] City u)
        {
            var c=await _db.Cities.FindAsync(id); if(c==null) return NotFound();
            c.Name=u.Name; c.Info=u.Info; c.ImageUrl=u.ImageUrl;
            await _db.SaveChangesAsync(); return Ok(c);
        }

        [HttpDelete("cities/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        { var c=await _db.Cities.FindAsync(id); if(c==null) return NotFound(); _db.Cities.Remove(c); await _db.SaveChangesAsync(); return Ok(); }

        /* quotes */
        [HttpPost("quotes")]
        public async Task<IActionResult> CreateQuote([FromBody] Quote q) { q.Id=0; _db.Quotes.Add(q); await _db.SaveChangesAsync(); return Ok(q); }

        [HttpPut("quotes/{id}")]
        public async Task<IActionResult> UpdateQuote(int id, [FromBody] Quote u)
        { var q=await _db.Quotes.FindAsync(id); if(q==null) return NotFound(); q.Text=u.Text; q.Author=u.Author; await _db.SaveChangesAsync(); return Ok(q); }

        [HttpDelete("quotes/{id}")]
        public async Task<IActionResult> DeleteQuote(int id)
        { var q=await _db.Quotes.FindAsync(id); if(q==null) return NotFound(); _db.Quotes.Remove(q); await _db.SaveChangesAsync(); return Ok(); }

        /* videos */
        [HttpPost("videos")]
        public async Task<IActionResult> CreateVideo([FromBody] Video v) { v.Id=0; _db.Videos.Add(v); await _db.SaveChangesAsync(); return Ok(v); }

        [HttpPut("videos/{id}")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] Video u)
        { var v=await _db.Videos.FindAsync(id); if(v==null) return NotFound(); v.Title=u.Title; v.Channel=u.Channel; v.Url=u.Url; await _db.SaveChangesAsync(); return Ok(v); }

        [HttpDelete("videos/{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        { var v=await _db.Videos.FindAsync(id); if(v==null) return NotFound(); _db.Videos.Remove(v); await _db.SaveChangesAsync(); return Ok(); }

        /* sources */
        [HttpPost("sources")]
        public async Task<IActionResult> CreateSource([FromBody] Source s) { s.Id=0; _db.Sources.Add(s); await _db.SaveChangesAsync(); return Ok(s); }

        [HttpPut("sources/{id}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] Source u)
        { var s=await _db.Sources.FindAsync(id); if(s==null) return NotFound(); s.Label=u.Label; s.Url=u.Url; await _db.SaveChangesAsync(); return Ok(s); }

        [HttpDelete("sources/{id}")]
        public async Task<IActionResult> DeleteSource(int id)
        { var s=await _db.Sources.FindAsync(id); if(s==null) return NotFound(); _db.Sources.Remove(s); await _db.SaveChangesAsync(); return Ok(); }

        /* quiz questions */
        [HttpPost("quiz")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizQuestion q) { q.Id=0; _db.QuizQuestions.Add(q); await _db.SaveChangesAsync(); return Ok(q); }

        [HttpPut("quiz/{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizQuestion u)
        { var q=await _db.QuizQuestions.FindAsync(id); if(q==null) return NotFound(); q.Question=u.Question; q.CorrectIndex=u.CorrectIndex; await _db.SaveChangesAsync(); return Ok(q); }

        [HttpDelete("quiz/{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var q=await _db.QuizQuestions.Include(x=>x.Options).FirstOrDefaultAsync(x=>x.Id==id);
            if(q==null) return NotFound(); _db.QuizQuestions.Remove(q); await _db.SaveChangesAsync(); return Ok();
        }
    }
}