using System.Collections.Generic;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Services
{
    public interface IProductionPlanService
    {
        IEnumerable<PowerPlantLoad> CalculateUnitCommitment(ProductionPlan productionPlan);
    }
}