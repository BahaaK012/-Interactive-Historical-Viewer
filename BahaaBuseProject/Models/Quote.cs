using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        [JsonIgnore] public Era? Era { get; set; }
    }
}
