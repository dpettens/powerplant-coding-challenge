using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Validators;

namespace PowerPlantChallenge.WebApi.Tests.Validators
{
    [TestClass]
    public class PowerPlantValidatorTest
    {
        private PowerPlantValidator _validator;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new PowerPlantValidator();
        }
        
        [TestMethod]
        public void PowerPlantValidator_NameIsNull_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant(null, PowerPlantType.WindTurbine, 0.5m, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Name);
        }
        
        [TestMethod]
        public void PowerPlantValidator_NameIsEmpty_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant(string.Empty, PowerPlantType.WindTurbine, 0.5m, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Name);
        }
        
        [TestMethod]
        public void PowerPlantValidator_TypeIsNotInEnum_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", (PowerPlantType) 4, 1.2m, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Type);
        }
        
        [TestMethod]
        public void PowerPlantValidator_EfficiencyIsNegative_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, -0.5m, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Efficiency);
        }
        
        [TestMethod]
        public void PowerPlantValidator_EfficiencyIsZero_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, 0, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Efficiency);
        }
        
        [TestMethod]
        public void PowerPlantValidator_EfficiencyIsGreaterThan1_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, 1.2m, 0, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.Efficiency);
        }
        
        [TestMethod]
        public void PowerPlantValidator_PMinIsNegative_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, 0.8m, -20, 100);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.PMin);
        }
        
        [TestMethod]
        public void PowerPlantValidator_PMaxIsSmallerThanPMin_ReturnsValidationError()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, 0.8m, 20, 10);

            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(p => p.PMax);
        }

        [TestMethod]
        public void PowerPlantValidator_MultipleInvalidProps_ReturnsMultipleValidationErrors()
        {
            // Arrange
            var powerPlant = new PowerPlant(null, PowerPlantType.Turbojet, 12, -4, 4);
        
            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(f => f.Name);
            result.ShouldHaveValidationErrorFor(f => f.Efficiency);
            result.ShouldHaveValidationErrorFor(f => f.PMin);
        }
        
        
        [TestMethod]
        public void PowerPlantValidator_ValidPowerPlant_ReturnsNoValidationErrors()
        {
            // Arrange
            var powerPlant = new PowerPlant("Test", PowerPlantType.WindTurbine, 0.5m, 0, 100);
        
            // Act  
            var result = _validator.TestValidate(powerPlant);
            
            // Assert
            result.IsValid.Should().BeTrue();
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}