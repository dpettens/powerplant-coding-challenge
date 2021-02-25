namespace PowerPlantChallenge.WebApi.Models
{
    public class Fuels
    {
        public Fuels(decimal gasPricePerMWh, decimal kerosenePricePerMWh, decimal windPercentage, decimal co2PricePerTon = 0)
        {
            GasPricePerMWh = gasPricePerMWh;
            KerosenePricePerMWh = kerosenePricePerMWh;
            WindPercentage = windPercentage;
            Co2PricePerTon = co2PricePerTon;
        }

        public decimal GasPricePerMWh { get; }
        public decimal KerosenePricePerMWh { get; }
        public decimal WindPercentage { get; }
        public decimal Co2PricePerTon { get; }
    }
}