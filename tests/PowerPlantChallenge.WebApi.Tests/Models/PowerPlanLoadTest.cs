using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Tests.Models
{
    [TestClass]
    public class PowerPlanLoadTest
    {
        [TestMethod]
        public void PowerPlanLoad_GasFiredWithPMinAndPMaxAsInteger_NoNeedToChangePMinAndPMax()
        {
            // Arrange & Act
            var result = new PowerPlantLoad("Test", PowerPlantType.GasFired, 100, 200, 80, 0);
            
            // Assert
            result.RealPMin.Should().Be(100);
            result.RealPMax.Should().Be(200);
        }
        
        [TestMethod]
        public void PowerPlanLoad_WindTurbine_NeedToCalculateRealPowerButNoNeedToFloor()
        {
            // Arrange & Act  
            var result = new PowerPlantLoad("Test", PowerPlantType.WindTurbine, 0, 36, 60, 0);
            
            // Assert
            result.RealPMin.Should().Be(0);
            result.RealPMax.Should().Be(21.6m);
        }
        
        [TestMethod]
        public void PowerPlanLoad_WindTurbine_NeedToCalculateRealPowerAndToFloorToOneDigitAfterDecimalPoint()
        {
            // Arrange & Act  
            var result = new PowerPlantLoad("Test", PowerPlantType.WindTurbine, 0, 85, 25.4m, 0);
            
            // Assert
            result.RealPMin.Should().Be(0);
            result.RealPMax.Should().Be(21.5m);
        }
    }
}