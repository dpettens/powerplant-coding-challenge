using System.Text.Json.Serialization;

namespace PowerPlantChallenge.WebApi.DTOs
{
    public class FuelsDto
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public decimal GasPricePerMWh { get; set; }
        
        [JsonPropertyName("kerosine(euro/MWh)")]
        public decimal KerosenePricePerMWh { get; set; }
        
        [JsonPropertyName("wind(%)")]
        public decimal WindPercentage { get; set; }
    }
}