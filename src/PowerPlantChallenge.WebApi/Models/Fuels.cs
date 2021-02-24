namespace PowerPlantChallenge.WebApi.Models
{
    public class Fuels
    {
        public Fuels(double gasPricePerMWh, double kerosenePricePerMWh, double windPercentage)
        {
            GasPricePerMWh = gasPricePerMWh;
            KerosenePricePerMWh = kerosenePricePerMWh;
            WindPercentage = windPercentage;
        }

        public double GasPricePerMWh { get; }
        public double KerosenePricePerMWh { get; }
        public double WindPercentage { get; }
    }
}