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
        public decimal RealPMin { get; }
        public decimal RealPMax { get; }
        public decimal Power { get; private set; }

        public void ChangePower(decimal power)
        {
            if (power < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(power), "power should not be negative");
            }

            Power = power;
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