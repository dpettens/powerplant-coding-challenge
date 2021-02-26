using FluentValidation;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Validators
{
    public class PowerPlantValidator : AbstractValidator<PowerPlant>
    {
        public PowerPlantValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Type).IsInEnum();
            RuleFor(p => p.Efficiency).GreaterThan(0).LessThanOrEqualTo(1);
            RuleFor(p => p.PMin).GreaterThanOrEqualTo(0);
            RuleFor(p => p.PMax).GreaterThanOrEqualTo(p => p.PMin);
        }
    }
}