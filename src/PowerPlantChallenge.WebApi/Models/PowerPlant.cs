namespace PowerPlantChallenge.WebApi.Models
{
    public class PowerPlant
    {
        public PowerPlant(string name, PowerPlantType type, double efficiency, int pMin, int pMax)
        {
            Name = name;
            Type = type;
            Efficiency = efficiency;
            PMin = pMin;
            PMax = pMax;
        }

        public string Name { get; }
        public PowerPlantType Type { get; }
        public double Efficiency { get; }
        public int PMin { get; }
        public int PMax { get; }
    }
}