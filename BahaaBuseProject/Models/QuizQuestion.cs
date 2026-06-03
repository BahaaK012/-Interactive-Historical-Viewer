using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int CorrectIndex { get; set; }
        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
        /* ignore era in jason */
        [JsonIgnore] public Era? Era { get; set; }
    }
}