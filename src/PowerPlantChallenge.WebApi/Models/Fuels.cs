namespace PowerPlantChallenge.WebApi.Models
{
    public class Fuels
    {
        public Fuels(decimal gasPricePerMWh, decimal kerosenePricePerMWh, decimal windPercentage)
        {
            GasPricePerMWh = gasPricePerMWh;
            KerosenePricePerMWh = kerosenePricePerMWh;
            WindPercentage = windPercentage;
        }

        public decimal GasPricePerMWh { get; }
        public decimal KerosenePricePerMWh { get; }
        public decimal WindPercentage { get; }
    }
}