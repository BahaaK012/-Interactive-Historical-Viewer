using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class QuizOption
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        /* ignore question in jason */
        [JsonIgnore] public QuizQuestion? QuizQuestion { get; set; }
    }
}