using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Services;

namespace PowerPlantChallenge.WebApi.Tests.Services
{
    [TestClass]
    public class ProductionPlanServiceTest
    {
        private IProductionPlanService _service;
        
        [TestInitialize]
        public void TestInitialize()
        {
            var loggerMock = new Mock<ILogger<ProductionPlanService>>();
            _service = new ProductionPlanService(loggerMock.Object);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ProductionPlanIsNull_ThrowArgumentNullException()
        {
            // Arrange & Act  
            Action act = () => _service.CalculateUnitCommitment(null);
            
            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*The production plan should not be null*")
                .And.ParamName.Should().Be("productionPlan");
        }
        
        [TestMethod]
        //TODO: change when custom exception
        public void CalculateUnitCommitment_LoadIsLesserThanAllPMin_ThrowException()
        {
            // Arrange
            var productionPlan = new ProductionPlan(30, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5, 40, 100),
                new("Gas", PowerPlantType.GasFired, 0.5, 100, 300)
            });
            
            // Act
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Impossible to supply this load because all power plants have a pmin greater than the requested load");
        }
        
        [TestMethod]
        //TODO: change when custom exception
        public void CalculateUnitCommitment_LoadIsGreaterThanAllPMax_ThrowException()
        {
            // Arrange
            var productionPlan = new ProductionPlan(400, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5, 100, 300)
            });
            
            // Act
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Impossible to supply this load because the sum of all pmax is lesser than the requested load");
        }

        [TestMethod]
        //TODO: change when custom exception
        public void CalculateUnitCommitment_LoadCanNotBeObtain_ThrowException()
        {
            // Arrange
            const double load = 18;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5, 20, 40),
                new("Turbojet", PowerPlantType.Turbojet, 0.3, 0, 15)
            });
            
            // Act  
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Impossible to supply this load because no combination of power plants allows to obtain it");
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ProductionPlanWithWindTurbines_WindPercentageIsTakenIntoAccount()
        {
            // Arrange
            const double load = 105;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 70), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 50)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 70, 70),
                new("Wind", 0, 35, 35)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ProductionPlanWithDifferentEfficiency_EfficiencyIsRespected()
        {
            // Arrange
            const double load = 350;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Turbojet", PowerPlantType.Turbojet, 0.3, 0, 25),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5, 100, 300)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 80, 80),
                new("Gas", 100, 300, 270),
                new("Turbojet", 0, 25, 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_LoadIsADecimalNumber_SumOfPowerPlantsIsEqualToLoad()
        {
            // Arrange
            const double load = 350.5;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Turbojet", PowerPlantType.Turbojet, 0.3, 0, 25),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5, 100, 300)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 80, 80.5),
                new("Gas", 100, 300, 270),
                new("Turbojet", 0, 25, 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_WindTurbineRealPMaxHasTwoDigitsAfterDecimalPoint_RoundToOneDecimalIsRespected()
        {
            // Arrange
            const double load = 150;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 85.5), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 92),
                new("Gas", PowerPlantType.GasFired, 0.5, 50, 100)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 78.6, 78.6),
                new("Gas", 50, 100, 71.4)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_LoadLesserThanGasPMin_NeedToUseTurbojet()
        {
            // Arrange
            const double load = 20;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5, 40, 100),
                new("Turbojet", PowerPlantType.Turbojet, 0.3, 0, 25)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Gas", 40, 100, 0),
                new("Turbojet", 0, 25, 20)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }

        [TestMethod]
        public void CalculateUnitCommitment_LoadForceToChangePowerOfOnePlantBecausePMinOfAnotherPlan_LoadIsRespected()
        {
            // Arrange
            const double load = 145;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 50),
                new("Gas", PowerPlantType.GasFired, 0.5, 10, 20),
                new("Gas", PowerPlantType.GasFired, 0.5, 10, 20)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 80, 80),
                new("Wind", 0, 40, 40),
                new("Gas", 10, 20, 15),
                new("Gas", 10, 20, 10)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_LoadForceToChangePowerOfMultiplePlantsBecausePMinOfOtherPlans_LoadIsRespected()
        {
            // Arrange
            const double load = 80;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 100), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 40),
                new("Gas", PowerPlantType.GasFired, 0.5, 25, 30),
                new("Gas", PowerPlantType.GasFired, 0.35, 20, 50)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 40, 35),
                new("Gas", 25, 30, 25),
                new("Gas", 20, 50, 20)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        private static bool ComparePowerPlantLoad(PowerPlantLoad p1, PowerPlantLoad p2)
        {
            return p1.Name == p2.Name && 
                   p1.RealPMin == p2.RealPMin && 
                   p1.RealPMax == p2.RealPMax && 
                   p1.Power == p1.Power;
        }
    }
}