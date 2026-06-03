using Microsoft.EntityFrameworkCore;
using BahaaBuseProject.Models;

namespace BahaaBuseProject.Data
{
    /* this is the database bridge that lets us talk to sql */
    public class HistoryContext : DbContext
    {
        public HistoryContext(DbContextOptions<HistoryContext> options) : base(options) { }

        /* tables for all our historical data models */
        public DbSet<Era> Eras { get; set; }
        public DbSet<Figure> Figures { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
    }
}