using FluentValidation;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Validators
{
    public class ProductionPlanValidator : AbstractValidator<ProductionPlan>
    {
        public ProductionPlanValidator()
        {
            RuleFor(p => p.Load).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Fuels).NotNull().SetValidator(new FuelsValidator());
            RuleFor(p => p.PowerPlants).NotEmpty().ForEach(p => p.SetValidator(new PowerPlantValidator()));
        }
    }
}