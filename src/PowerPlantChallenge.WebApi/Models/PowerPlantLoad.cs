using System;

namespace PowerPlantChallenge.WebApi.Models
{
    public class PowerPlantLoad
    {
        public PowerPlantLoad(string name, PowerPlantType type, double pMin, double pMax, double windPercentage)
        {
            Name = name;
            RealPMin = CalculateRealPower(pMin, type, windPercentage);
            RealPMax = CalculateRealPower(pMax, type, windPercentage);
        }
        
        public PowerPlantLoad(string name, double realPMin, double realPMax, double power)
        {
            Name = name;
            RealPMin = realPMin;
            RealPMax = realPMax;
            Power = power;
        }

        public string Name { get; }
        public double RealPMin { get; }
        public double RealPMax { get; }
        public double Power { get; private set; }

        public void ChangePower(double power)
        {
            if (power < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(power), "power should not be negative");
            }

            Power = power;
        }
        
        private static double CalculateRealPower(double power, PowerPlantType type, double windPercentage)
        {
            var realPower = type == PowerPlantType.WindTurbine ? power * (windPercentage / 100) : power;
            
            // Keep only one digit after decimal point if necessary
            return Math.Floor(realPower * 10) / 10;
        }
    }
}