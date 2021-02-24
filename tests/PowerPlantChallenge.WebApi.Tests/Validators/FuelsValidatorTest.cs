using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Validators;

namespace PowerPlantChallenge.WebApi.Tests.Validators
{
    [TestClass]
    public class FuelsValidatorTest
    {
        private FuelsValidator _validator;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new FuelsValidator();
        }
        
        [TestMethod]
        public void FuelsValidator_GasPricePricePerMWhIsNegative_ReturnsValidationError()
        {
            // Arrange
            var fuels = new Fuels(-10, 10, 10);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.GasPricePerMWh);
        }
        
        [TestMethod]
        public void FuelsValidator_KerosenePricePerMWhIsNegative_ReturnsValidationError()
        {
            // Arrange
            var fuels = new Fuels(10, -10, 10);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.KerosenePricePerMWh);
        }
        
        [TestMethod]
        public void FuelsValidator_WindPercentageIsNegative_ReturnsValidationError()
        {
            // Arrange
            var fuels = new Fuels(10, 10, -10);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.WindPercentage);
        }
        
        [TestMethod]
        public void FuelsValidator_WindPercentageIsGreaterThan100_ReturnsValidationError()
        {
            // Arrange
            var fuels = new Fuels(10, 10, 120);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.WindPercentage);
        }
        
        [TestMethod]
        public void FuelsValidator_MultipleInvalidProps_ReturnsMultipleValidationErrors()
        {
            // Arrange
            var fuels = new Fuels(-10, -10, 120);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.GasPricePerMWh);
            result.ShouldHaveValidationErrorFor(f => f.KerosenePricePerMWh);
            result.ShouldHaveValidationErrorFor(f => f.WindPercentage);
        }

        
        [TestMethod]
        public void FuelsValidator_ValidFuels_ReturnsNoValidationErrors()
        {
            // Arrange
            var fuels = new Fuels(10, 10, 80);

            // Act  
            var result = _validator.TestValidate(fuels);
            
            // Assert
            result.IsValid.Should().BeTrue();
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}