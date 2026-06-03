using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class Video
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        [JsonIgnore] public Era? Era { get; set; }
    }
}
