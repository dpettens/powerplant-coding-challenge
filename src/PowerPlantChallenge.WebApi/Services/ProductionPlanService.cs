﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PowerPlantChallenge.WebApi.Exceptions;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Validators;

namespace PowerPlantChallenge.WebApi.Services
{
    public class ProductionPlanService : IProductionPlanService
    {
        private const decimal Co2GeneratedPerMWh = 0.3m;
        private readonly ILogger<ProductionPlanService> _logger;

        public ProductionPlanService(ILogger<ProductionPlanService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calculate the output power for each powerplant in order to supply the requested load
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
                .OrderBy(p => CalculatePricePerMWh(p, productionPlan.Fuels))
                .ThenByDescending(p => p.PMax)
                .Select(p => new PowerPlantLoad(p.Name, p.Type, p.PMin, p.PMax, productionPlan.Fuels.WindPercentage, 0))
                .ToList();
            
            // Check if there is at least one powerplant which has a minimum power less than or equal to the requested load
            var smallestPMin = powerPlantLoadsByMeritOrder.Min(p => p.RealPMin);
            if (productionPlan.Load < smallestPMin)
                throw new ImpossibleToSupplyException(
                    productionPlan.Load, 
                    $"All powerplants have a PMin greater than the requested load (smallest PMin {smallestPMin})"
                );
            
            // Check if the maximum real power of all powerplants is greater than or equal to the requested load
            var totalPMaxPossible = powerPlantLoadsByMeritOrder.Sum(p => p.RealPMax);
            if (productionPlan.Load > totalPMaxPossible)
                throw new ImpossibleToSupplyException(
                    productionPlan.Load, 
                    $"The sum of all PMax is lesser than the requested load (total PMax {totalPMaxPossible})"
                );
            
            // Find best output power for each powerplant
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

                // No more power to find
                if (remainingLoadToPlan == 0) break;
            }
            
            // If there is some remaining load, it's not possible to find a solution
            if (remainingLoadToPlan > 0)
                throw new ImpossibleToSupplyException(
                    productionPlan.Load, 
                    "No combination of powerplants allows to obtain it"
                );

            return powerPlantLoadsByMeritOrder;
        }

        /// <summary>
        /// Calculate price per MWh of a specific powerplant based on its efficiency, fuel price and Co2 for gas-fired
        /// </summary>
        private static decimal CalculatePricePerMWh(PowerPlant powerPlant, Fuels fuels)
        {
            // Produce electricity from wind is free
            // Only Gas-fired powerplant can generate Co2
            var pricePerMWh = powerPlant.Type switch
            {
                PowerPlantType.WindTurbine => 0,
                PowerPlantType.GasFired => fuels.GasPricePerMWh + Co2GeneratedPerMWh * fuels.Co2PricePerTon,
                PowerPlantType.Turbojet => fuels.KerosenePricePerMWh,
                _ => throw new ArgumentOutOfRangeException(nameof(powerPlant),
                    $"{powerPlant.Type} is an unknown powerplant type")
            };

            return pricePerMWh / powerPlant.Efficiency;
        }
        
        /// <summary>
        /// Calculate how much power the powerplant will deliver in order to achieve the requested load
        /// </summary>
        private static (decimal, decimal) CalculateOutputPowerPlant(PowerPlantLoad powerPlantLoad,
            IEnumerable<PowerPlantLoad> previousPowerPlantLoads, decimal remainingLoadToPlan)
        {
            // The request load is already supplied, so no need to power this powerplant
            if (remainingLoadToPlan == 0)
                return (0, 0);
            
            // The load needed is between RealPMin and RealPMax so use the remaining load as load for this powerplant 
            if (powerPlantLoad.RealPMin <= remainingLoadToPlan && remainingLoadToPlan <= powerPlantLoad.RealPMax)
                return (remainingLoadToPlan, 0);

            // The load needed is greater than RealPMax of the powerplant
            // So use maximum power of the plan and subtract it from the remaining
            if (remainingLoadToPlan > powerPlantLoad.RealPMax)
                return (powerPlantLoad.RealPMax, remainingLoadToPlan - powerPlantLoad.RealPMax);
            
            // The load needed is lesser than RealPMin of the powerplant
            // Calculate the extra power produced by using the RealPMin of this powerplant
            var extraPower = powerPlantLoad.RealPMin - remainingLoadToPlan;
            var previousPowerPlantLoadsList = previousPowerPlantLoads.ToList();
            
            // Check if we can remove this extra power from previous powerplants already calculated
            // If not don't use this powerplant for the load
            if (!CanAdaptPreviousPowerPlantLoads(previousPowerPlantLoadsList, extraPower))
                return (0, remainingLoadToPlan);

            // Adapt the powers of previous powerplants to remove the extra power and use the RealPMin for this powerplant
            AdaptPreviousPowerPlantLoads(previousPowerPlantLoadsList, extraPower);
            return (powerPlantLoad.RealPMin, 0);
        }

        /// <summary>
        /// Check if it's possible to reduce this extra power on one or more previous powerplants 
        /// </summary>
        private static bool CanAdaptPreviousPowerPlantLoads(IEnumerable<PowerPlantLoad> previousPowerPlantLoads, 
            decimal extraPower)
        {
            var remainingExtraPower = extraPower;
            
            // Start from the last calculated powerplant because it's the least efficient 
            foreach (var powerPlantLoad in previousPowerPlantLoads.Reverse())
            {
                // Check if the power of the powerplant can be decreased the remaining extra power
                var differencePower = powerPlantLoad.Power - remainingExtraPower;
                if (differencePower >= powerPlantLoad.RealPMin)
                    return true;
                
                // If not remove the difference between actual power and its minimum
                remainingExtraPower -= powerPlantLoad.Power - powerPlantLoad.RealPMin;
            }

            // If the remaining extra power > 0, it's not possible to adapt previous powerplants
            return remainingExtraPower == 0;
        }

        /// <summary>
        /// Adapt power by removing the extra power on one or more of the previous powerplants
        /// </summary>
        private static void AdaptPreviousPowerPlantLoads(IEnumerable<PowerPlantLoad> previousPowerPlantLoads, 
            decimal extraPower)
        {
            var remainingExtraPower = extraPower;
            
            // Start from the last calculated powerplant because it's the least efficient 
            foreach (var powerPlantLoad in previousPowerPlantLoads.Reverse())
            {
                // If the power of the powerplant can be decreased the remaining extra power
                // Reduce it with this amount and end the loop
                var differencePower = powerPlantLoad.Power - remainingExtraPower;
                if (differencePower >= powerPlantLoad.RealPMin)
                {
                    powerPlantLoad.ChangePower(differencePower);
                    return;
                }
                
                // If not, reduce the powerplant to its minimum
                // Subtract the difference between actual power and its minimum
                remainingExtraPower -= powerPlantLoad.Power - powerPlantLoad.RealPMin;
                powerPlantLoad.ChangePower(powerPlantLoad.RealPMin);
            }
        }
    }
}