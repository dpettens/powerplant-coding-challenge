using System;

namespace PowerPlantChallenge.WebApi.Models
{
    public class PowerPlantLoad
    {
        public PowerPlantLoad(string name, PowerPlantType type, decimal pMin, decimal pMax, decimal windPercentage, decimal power)
        {
            Name = name;
            RealPMin = CalculateRealPower(pMin, type, windPercentage);
            RealPMax = CalculateRealPower(pMax, type, windPercentage);
            Power = power;
        }
        
        public PowerPlantLoad(string name, decimal realPMin, decimal realPMax, decimal power)
        {
            Name = name;
            RealPMin = realPMin;
            RealPMax = realPMax;
            Power = power;
        }

        public string Name { get; }
        
        /// <summary>
        /// This is the real minimum power that the power plant can produce
        /// It is only different from PMin for wind turbine
        /// </summary>
        public decimal RealPMin { get; }
        
        /// <summary>
        /// This is the real maximum power that the power plant can produce
        /// It is only different from PMin for wind turbine
        /// </summary>
        public decimal RealPMax { get; }
        
        /// <summary>
        /// This is the power that the power plant will generate for the production plan (0 = produce nothing)
        /// </summary>
        public decimal Power { get; private set; }

        /// <summary>
        /// Adapt the current power that the power plant will generate to the new amount
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPower"/> is negative</exception>
        public void ChangePower(decimal newPower)
        {
            if (newPower < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newPower), "power should not be negative");
            }

            Power = newPower;
        }
        
        private static decimal CalculateRealPower(decimal power, PowerPlantType type, decimal windPercentage)
        {
            var realPower = type == PowerPlantType.WindTurbine ? power * (windPercentage / 100) : power;
            
            // Keep only one digit after decimal point if necessary
            // Always need to round down since it's impossible to add non existing power (ex. 21.55 to 21.5 and not to 21.6)
            return Math.Floor(realPower * 10) / 10;
        }
    }
}