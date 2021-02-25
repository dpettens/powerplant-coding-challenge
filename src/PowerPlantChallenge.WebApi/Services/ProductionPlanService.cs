using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Validators;

namespace PowerPlantChallenge.WebApi.Services
{
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly ILogger<ProductionPlanService> _logger;

        public ProductionPlanService(ILogger<ProductionPlanService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calculate the output power for each power plants in order to supply the requested load
        /// </summary>
        public IEnumerable<PowerPlantLoad> CalculateUnitCommitment(ProductionPlan productionPlan)
        {
            if (productionPlan == null)
            {
                _logger.LogError("The production plan should not be null");
                throw new ArgumentNullException(nameof(productionPlan), "The production plan should not be null");
            }

            var validator = new ProductionPlanValidator();
            validator.ValidateAndThrow(productionPlan);

            // Create PowerPlantLoad and order it by merit order and maximum power
            var powerPlantLoadsByMeritOrder = productionPlan.PowerPlants
                .OrderBy(p => CalculateMeritOrder(p, productionPlan.Fuels))
                .ThenByDescending(p => p.PMax)
                .Select(p => new PowerPlantLoad(p.Name, p.Type, p.PMin, p.PMax, productionPlan.Fuels.WindPercentage))
                .ToList();
            
            //TODO: create custom exception
            // Check if there is at least one power plant who has a minimum power lesser or equal than the requested load
            var smallestPMin = powerPlantLoadsByMeritOrder.Min(p => p.RealPMin);
            if (productionPlan.Load < smallestPMin)
                throw new Exception("Impossible to supply this load because all power plants have a pmin greater than the requested load");
            
            //TODO: create custom exception
            // Check if the maximum real power of all power plants is greater or equal than the requested load
            var totalPMaxPossible = powerPlantLoadsByMeritOrder.Sum(p => p.RealPMax);
            if (productionPlan.Load > totalPMaxPossible)
                throw new Exception("Impossible to supply this load because the sum of all pmax is lesser than the requested load");
            
            // Find best output power for each plant
            var remainingLoadToPlan = productionPlan.Load;
            for(var i = 0; i < powerPlantLoadsByMeritOrder.Count; i++)
            {
                var (power, newRemainingLoadToPlan) = CalculateOutputPowerPlant(
                    powerPlantLoadsByMeritOrder[i],
                    powerPlantLoadsByMeritOrder.Take(i),
                    remainingLoadToPlan
                );

                remainingLoadToPlan = newRemainingLoadToPlan;
                powerPlantLoadsByMeritOrder[i].ChangePower(power);
            }

            //TODO: create custom exception
            // If there is some remaining load, it's not possible to find a solution
            if (remainingLoadToPlan > 0)
                throw new Exception("Impossible to supply this load because no combination of power plants allows to obtain it");

            return powerPlantLoadsByMeritOrder;
        }

        /// <summary>
        /// Calculate merit order of a specific power plant based on its efficiency and its fuel price
        /// </summary>
        private static double CalculateMeritOrder(PowerPlant powerPlant, Fuels fuels)
        {
            // Produce electricity from wind is free
            var pricePerMWh = powerPlant.Type switch
            {
                PowerPlantType.WindTurbine => 0,
                PowerPlantType.GasFired => fuels.GasPricePerMWh,
                PowerPlantType.Turbojet => fuels.KerosenePricePerMWh,
                _ => throw new ArgumentOutOfRangeException(nameof(powerPlant),
                    $"{powerPlant.Type} is an unknown power plant type")
            };
            
            return pricePerMWh / powerPlant.Efficiency;
        }
        
        /// <summary>
        /// Calculate how much power the plant will deliver in order to achieve the requested load
        /// </summary>
        private static (double, double) CalculateOutputPowerPlant(PowerPlantLoad powerPlantLoad,
            IEnumerable<PowerPlantLoad> previousPowerPlantLoads, double remainingLoadToPlan)
        {
            // The load needed is between RealPMin and RealPMax so use the remaining load as load for this plant 
            if (powerPlantLoad.RealPMin <= remainingLoadToPlan && remainingLoadToPlan <= powerPlantLoad.RealPMax)
                return (remainingLoadToPlan, 0);

            // The load needed is greater than RealPMax of the plant
            // So use maximum power of the plan and subtract it from the remaining
            if (remainingLoadToPlan > powerPlantLoad.RealPMax)
                return (powerPlantLoad.RealPMax, remainingLoadToPlan - powerPlantLoad.RealPMax);
            
            // The load needed is lesser than RealPMin of the plant
            // Calculate extra power already planned
            var extraPower = powerPlantLoad.RealPMin - remainingLoadToPlan;
            var previousPowerPlantLoadsList = previousPowerPlantLoads.ToList();
            
            // Check if we can remove this extra power from previous plants calculated
            // If not don't use this power plant for the load
            if (!CanAdaptPreviousPowerPlantLoads(previousPowerPlantLoadsList, extraPower))
                return (0, remainingLoadToPlan);

            // Adapt the powers of previous power plants to remove the extra power and use the RealPMin for this plant
            AdaptPreviousPowerPlantLoads(previousPowerPlantLoadsList, extraPower);
            return (powerPlantLoad.RealPMin, 0);
        }

        /// <summary>
        /// Check if it's possible to reduce this extra power on one or more previous power plants 
        /// </summary>
        private static bool CanAdaptPreviousPowerPlantLoads(IEnumerable<PowerPlantLoad> previousPowerPlantLoads, 
            double extraPower)
        {
            var remainingExtraPower = extraPower;
            
            // Start from the last calculated power plant because it's the least efficient 
            foreach (var powerPlantLoad in previousPowerPlantLoads.Reverse())
            {
                // Check if the power of the plant can be decreased the remaining extra power
                var differencePower = powerPlantLoad.Power - remainingExtraPower;
                if (differencePower >= powerPlantLoad.RealPMin)
                    return true;
                
                // If not remove the difference between actual power and its minimum
                remainingExtraPower -= powerPlantLoad.Power - powerPlantLoad.RealPMin;
            }

            // If the remaining extra power > 0, it's not possible to adapt previous plants
            return remainingExtraPower == 0;
        }

        /// <summary>
        /// Adapt previous power plants by removing the extra power of one or more plants
        /// </summary>
        private static void AdaptPreviousPowerPlantLoads(IEnumerable<PowerPlantLoad> previousPowerPlantLoads, 
            double extraPower)
        {
            var remainingExtraPower = extraPower;
            
            // Start from the last calculated power plant because it's the least efficient 
            foreach (var powerPlantLoad in previousPowerPlantLoads.Reverse())
            {
                // If the power of the plant can be decreased the remaining extra power
                // Reduce it with this amount and end the loop
                var differencePower = powerPlantLoad.Power - remainingExtraPower;
                if (differencePower >= powerPlantLoad.RealPMin)
                {
                    powerPlantLoad.ChangePower(differencePower);
                    return;
                }
                
                // If not, reduce the plant to its minimum
                // Subtract the difference between actual power and its minimum
                remainingExtraPower -= powerPlantLoad.Power - powerPlantLoad.RealPMin;
                powerPlantLoad.ChangePower(powerPlantLoad.RealPMin);
            }
        }
    }
}