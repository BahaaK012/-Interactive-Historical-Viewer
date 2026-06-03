using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class Source
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        [JsonIgnore] public Era? Era { get; set; }
    }
}
