using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Validators;

namespace PowerPlantChallenge.WebApi.Tests.Validators
{
    [TestClass]
    public class ProductionPlanValidatorTest
    {
        private ProductionPlanValidator _validator;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new ProductionPlanValidator();
        }

        [TestMethod]
        public void ProductionPlanValidator_LoadIsNegative_ReturnsValidationError()
        {
            // Arrange
            var productionPlan = new ProductionPlan(-50, new Fuels(10, 10, 10), new List<PowerPlant>());

            // Act  
            var result = _validator.TestValidate(productionPlan);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Load);
        }

        [TestMethod]
        public void ProductionPlanValidator_FuelsIsNull_ReturnsValidationError()
        {
            // Arrange
            var productionPlan = new ProductionPlan(50, null, new List<PowerPlant>());

            // Act  
            var result = _validator.TestValidate(productionPlan);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Fuels);
        }

        [TestMethod]
        public void ProductionPlanValidator_PowerPlantsIsNull_ReturnsValidationError()
        {
            // Arrange
            var productionPlan = new ProductionPlan(50, new Fuels(10, 10, 80), null);

            // Act  
            var result = _validator.TestValidate(productionPlan);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.PowerPlants);
        }

        [TestMethod]
        public void ProductionPlanValidator_PowerPlantsIsEmpty_ReturnsValidationError()
        {
            // Arrange
            var productionPlan = new ProductionPlan(50, new Fuels(10, 10, 80), null);

            // Act  
            var result = _validator.TestValidate(productionPlan);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.PowerPlants);
        }

        [TestMethod]
        public void ProductionPlanValidator_MultipleInvalidProps_ReturnsMultipleValidationErrors()
        {
            // Arrange
            var productionPlan = new ProductionPlan(-50, null, new List<PowerPlant>());
        
            // Act  
            var result = _validator.TestValidate(productionPlan);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Load);
            result.ShouldHaveValidationErrorFor(p => p.Fuels);
            result.ShouldHaveValidationErrorFor(p => p.PowerPlants);
        }


        [TestMethod]
        public void ProductionPlanValidator_ValidProductionPlan_ReturnsNoValidationErrors()
        {
            // Arrange
            var productionPlan = new ProductionPlan(50, new Fuels(10, 10, 80), new List<PowerPlant>
            {
                new("Test", PowerPlantType.WindTurbine, 0.5, 0, 100)
            });

            // Act  
            var result = _validator.TestValidate(productionPlan);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}