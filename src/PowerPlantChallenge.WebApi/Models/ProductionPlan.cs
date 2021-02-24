using System.Collections.Generic;

namespace PowerPlantChallenge.WebApi.Models
{
    public class ProductionPlan
    {
        public ProductionPlan(int load, Fuels fuels, IEnumerable<PowerPlant> powerPlants)
        {
            Load = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        public int Load { get; }
        public Fuels Fuels { get; }
        public IEnumerable<PowerPlant> PowerPlants { get; }
    }
}