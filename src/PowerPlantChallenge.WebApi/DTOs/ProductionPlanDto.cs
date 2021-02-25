using System.Collections.Generic;

namespace PowerPlantChallenge.WebApi.DTOs
{
    public class ProductionPlanDto
    {
        public decimal Load { get; set; }
        public FuelsDto Fuels { get; set; }
        public IEnumerable<PowerPlantDto> PowerPlants { get; set; }
    }
}