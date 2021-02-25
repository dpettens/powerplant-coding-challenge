using FluentValidation;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Validators
{
    public class FuelsValidator : AbstractValidator<Fuels>
    {
        public FuelsValidator()
        {
            RuleFor(f => f.GasPricePerMWh).GreaterThanOrEqualTo(0);
            RuleFor(f => f.KerosenePricePerMWh).GreaterThanOrEqualTo(0);
            RuleFor(f => f.WindPercentage).InclusiveBetween(0, 100);
            RuleFor(f => f.Co2PricePerTon).GreaterThanOrEqualTo(0);
        }
    }
}