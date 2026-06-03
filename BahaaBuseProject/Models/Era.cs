namespace BahaaBuseProject.Models
{
    public class Era
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string NodeLabel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string BgColor { get; set; } = string.Empty;
        public string Stat1 { get; set; } = "75%";
        public string Stat2 { get; set; } = "75%";
        public string Stat3 { get; set; } = "75%";
        public string SectionIcon { get; set; } = string.Empty;
        public string SectionBody { get; set; } = string.Empty;

        /* lists of all connected data for the era */
        public ICollection<Figure> Figures { get; set; } = new List<Figure>();
        public ICollection<City> Cities { get; set; } = new List<City>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<Source> Sources { get; set; } = new List<Source>();
        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }
}