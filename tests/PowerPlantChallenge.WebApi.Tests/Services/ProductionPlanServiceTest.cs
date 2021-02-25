using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerPlantChallenge.WebApi.Exceptions;
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
        public void CalculateUnitCommitment_LoadIsLesserThanAllPMin_ThrowImpossibleToSupplyException()
        {
            // Arrange
            const decimal load = 30;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5m, 40, 100),
                new("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
            });
            
            // Act
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            const decimal smallestPMin = 40;
            act.Should().Throw<ImpossibleToSupplyException>()
                .WithMessage($"Impossible to supply the requested ({load}) load \n " +
                             $"Reason: All power plants have a PMin greater than the requested load (smallest PMin {smallestPMin})");
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_LoadIsGreaterThanAllPMax_ThrowException()
        {
            // Arrange
            const decimal load = 400;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
            });
            
            // Act
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            const decimal totalPMaxPossible = 380; // 100 * 80% + 300
            act.Should().Throw<ImpossibleToSupplyException>()
                .WithMessage($"Impossible to supply the requested ({load}) load \n " +
                             $"Reason: The sum of all PMax is lesser than the requested load (total PMax {totalPMaxPossible})");
        }

        [TestMethod]
        public void CalculateUnitCommitment_LoadCanNotBeObtain_ThrowException()
        {
            // Arrange
            const decimal load = 18;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5m, 20, 40),
                new("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 15)
            });
            
            // Act  
            Action act = () => _service.CalculateUnitCommitment(productionPlan);
            
            // Assert
            act.Should().Throw<ImpossibleToSupplyException>()
                .WithMessage($"Impossible to supply the requested ({load}) load \n " +
                             $"Reason: No combination of power plants allows to obtain it");
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ProductionPlanWithWindTurbines_WindPercentageIsTakenIntoAccount()
        {
            // Arrange
            const decimal load = 105;
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
            const decimal load = 350;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 25),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
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
            const decimal load = 350.5m;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 25),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 80, 80.5m),
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
            const decimal load = 150;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 85.5m), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 92),
                new("Gas", PowerPlantType.GasFired, 0.5m, 50, 100)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Wind", 0, 78.6m, 78.6m),
                new("Gas", 50, 100, 71.4m)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_LoadLesserThanGasPMin_NeedToUseTurbojet()
        {
            // Arrange
            const decimal load = 20;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5m, 40, 100),
                new("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 25)
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
            const decimal load = 145;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 80), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 50),
                new("Gas", PowerPlantType.GasFired, 0.5m, 10, 20),
                new("Gas", PowerPlantType.GasFired, 0.5m, 10, 20)
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
            const decimal load = 80;
            var productionPlan = new ProductionPlan(load, new Fuels(15, 50, 100), new List<PowerPlant>
            {
                new("Wind", PowerPlantType.WindTurbine, 1, 0, 40),
                new("Gas", PowerPlantType.GasFired, 0.5m, 25, 30),
                new("Gas", PowerPlantType.GasFired, 0.35m, 20, 50)
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
        
        [TestMethod]
        public void CalculateUnitCommitment_GasIsMoreEfficientThanKeroseneButCo2PriceIsVeryHigh_KeroseneIsPrefered()
        {
            // Arrange
            const decimal load = 25;
            var productionPlan = new ProductionPlan(load, new Fuels(25, 30, 60, 87), new List<PowerPlant>
            {
                new("Gas", PowerPlantType.GasFired, 0.5m, 20, 100),
                new("Turbojet", PowerPlantType.Turbojet, 0.4m, 0, 25)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("Turbojet", 0, 25, 25),
                new("Gas", 20, 100, 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ExamplePayload1_ReturnsExpectedResult()
        {
            // Arrange
            const decimal load = 480;
            var productionPlan = new ProductionPlan(load, new Fuels(13.4m, 50.8m, 60, 20), new List<PowerPlant>
            {
                new("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                new("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                new("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                new("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("windpark1", 0, 90, 90),
                new("windpark2", 0, 21.6m, 21.6m),
                new("gasfiredbig1", 100, 460, 368.4m),
                new("gasfiredbig2", 100, 460, 0),
                new("gasfiredsomewhatsmaller", 40, 210, 0),
                new("tj1", 0, 16, 0),
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ExamplePayload2_ReturnsExpectedResult()
        {
            // Arrange
            const decimal load = 480;
            var productionPlan = new ProductionPlan(load, new Fuels(13.4m, 50.8m, 0, 20), new List<PowerPlant>
            {
                new("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                new("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                new("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                new("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("windpark1", 0, 0, 0),
                new("windpark2", 0, 0, 0),
                new("gasfiredbig1", 100, 460, 380),
                new("gasfiredbig2", 100, 460, 100),
                new("gasfiredsomewhatsmaller", 40, 210, 0),
                new("tj1", 0, 16, 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().Equal(expectedResult, ComparePowerPlantLoad);
        }
        
        [TestMethod]
        public void CalculateUnitCommitment_ExamplePayload3_ReturnsExpectedResult()
        {
            // Arrange
            const decimal load = 910;
            var productionPlan = new ProductionPlan(load, new Fuels(13.4m, 50.8m, 60, 20), new List<PowerPlant>
            {
                new("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                new("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                new("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                new("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                new("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            });
            
            // Act  
            var result = _service.CalculateUnitCommitment(productionPlan).ToList();
            
            // Assert
            var expectedResult = new List<PowerPlantLoad>
            {
                new("windpark1", 0, 90, 90),
                new("windpark2", 0, 21.6m, 21.6m),
                new("gasfiredbig1", 100, 460, 460),
                new("gasfiredbig2", 100, 460, 338.4m),
                new("gasfiredsomewhatsmaller", 40, 210, 0),
                new("tj1", 0, 16, 0),
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