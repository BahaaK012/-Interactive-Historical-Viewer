using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class Figure
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        /* ignore era in jason */
        [JsonIgnore] public Era? Era { get; set; }
    }
}