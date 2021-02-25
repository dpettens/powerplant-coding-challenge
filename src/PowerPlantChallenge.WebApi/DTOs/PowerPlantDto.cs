using System.Text.Json.Serialization;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.DTOs
{
    public class PowerPlantDto
    {
        public string Name { get; set; }
        public PowerPlantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
    }
}