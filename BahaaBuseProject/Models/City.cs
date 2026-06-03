using System.Text.Json.Serialization;
namespace BahaaBuseProject.Models
{
    public class City
    {
        public int Id { get; set; }
        public int EraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        /* ignore era in jason to stop infinite loops */
        [JsonIgnore] public Era? Era { get; set; }
    }
}