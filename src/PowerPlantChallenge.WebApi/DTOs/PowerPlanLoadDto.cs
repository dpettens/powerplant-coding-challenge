using System.Text.Json.Serialization;

namespace PowerPlantChallenge.WebApi.DTOs
{
    public class PowerPlanLoadDto
    {
        public string Name { get; set; }
        
        [JsonPropertyName("p")]
        public decimal Power { get; set; }
    }
}