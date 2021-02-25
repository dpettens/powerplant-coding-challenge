namespace PowerPlantChallenge.WebApi.Models
{
    public class PowerPlant
    {
        public PowerPlant(string name, PowerPlantType type, decimal efficiency, decimal pMin, decimal pMax)
        {
            Name = name;
            Type = type;
            Efficiency = efficiency;
            PMin = pMin;
            PMax = pMax;
        }

        public string Name { get; }
        public PowerPlantType Type { get; }
        public decimal Efficiency { get; }
        public decimal PMin { get; }
        public decimal PMax { get; }
    }
}