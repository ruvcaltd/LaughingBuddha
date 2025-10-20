using LAF.DataAccess.Models;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LAF.Tests.Services
{
    [TestFixture]
    public class TargetCircleServiceTests
    {
        private Mock<IRepoTradeRepository> _mockRepoTradeRepository;
        private Mock<IRepoRateRepository> _mockRepoRateRepository;
        private Mock<ICounterpartyRepository> _mockCounterpartyRepository;
        private Mock<ILogger<TargetCircleService>> _mockLogger;
        private TargetCircleService _targetCircleService;

        [SetUp]
        public void Setup()
        {
            _mockRepoTradeRepository = new Mock<IRepoTradeRepository>();
            _mockRepoRateRepository = new Mock<IRepoRateRepository>();
            _mockCounterpartyRepository = new Mock<ICounterpartyRepository>();
            _mockLogger = new Mock<ILogger<TargetCircleService>>();

            _targetCircleService = new TargetCircleService(
                _mockRepoTradeRepository.Object,
                _mockRepoRateRepository.Object,
                _mockCounterpartyRepository.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task ValidateTradeAgainstTargetCircleAsync_WhenTradeWithinLimit_ReturnsValidResult()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var proposedNotional = 1000000m; // 1M
            var currentExposure = 5000000m; // 5M
            var targetCircle = 10m; // 10M

            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate))
                .ReturnsAsync(currentExposure);

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate
                {
                    CounterpartyId = counterpartyId,
                    EffectiveDate = tradeDate,
                    TargetCircle = targetCircle
                }
            };

            _mockRepoRateRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RepoRate, bool>>>()))
                .ReturnsAsync(repoRates);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(counterpartyId))
                .ReturnsAsync(new DataAccess.Models.Counterparty { Id = counterpartyId, CounterpartyName = "Test Counterparty" });

            // Act
            var result = await _targetCircleService.ValidateTradeAgainstTargetCircleAsync(counterpartyId, tradeDate, proposedNotional);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(counterpartyId, result.CounterpartyId);
            Assert.AreEqual("Test Counterparty", result.CounterpartyName);
            Assert.AreEqual(currentExposure, result.CurrentExposure);
            Assert.AreEqual(proposedNotional, result.ProposedNotional);
            Assert.AreEqual(targetCircle, result.TargetCircle);
            Assert.AreEqual(currentExposure + proposedNotional, result.NewTotalExposure);
            Assert.IsTrue(result.IsWithinLimit);
            Assert.AreEqual(60m, result.LimitUtilizationPercentage); // (6M/10M)*100
            Assert.AreEqual("Trade is within TargetCircle limit", result.ValidationMessage);
        }

        [Test]
        public async Task ValidateTradeAgainstTargetCircleAsync_WhenTradeExceedsLimit_ReturnsInvalidResult()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var proposedNotional = 6000000m; // 6M
            var currentExposure = 5000000m; // 5M
            var targetCircle = 10m; // 10M

            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate))
                .ReturnsAsync(currentExposure);

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate
                {
                    CounterpartyId = counterpartyId,
                    EffectiveDate = tradeDate,
                    TargetCircle = targetCircle
                }
            };

            _mockRepoRateRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataAccess.Models.RepoRate, bool>>>()))
                .ReturnsAsync(repoRates);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(counterpartyId))
                .ReturnsAsync(new DataAccess.Models.Counterparty { Id = counterpartyId, CounterpartyName = "Test Counterparty" });

            // Act
            var result = await _targetCircleService.ValidateTradeAgainstTargetCircleAsync(counterpartyId, tradeDate, proposedNotional);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsWithinLimit);
            Assert.AreEqual(110m, result.LimitUtilizationPercentage); // (11M/10M)*100
            StringAssert.Contains("exceeds TargetCircle limit", result.ValidationMessage);
        }

        [Test]
        public async Task GetCurrentExposureAsync_ReturnsCorrectExposure()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var expectedExposure = 3000000m; // 3M

            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate))
                .ReturnsAsync(expectedExposure);

            // Act
            var result = await _targetCircleService.GetCurrentExposureAsync(counterpartyId, tradeDate);

            // Assert
            Assert.AreEqual(expectedExposure, result);
        }

        [Test]
        public async Task GetTargetCircleAsync_ReturnsCorrectTargetCircle()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var expectedTargetCircle = 15m; // 15M

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate
                {
                    CounterpartyId = counterpartyId,
                    EffectiveDate = tradeDate,
                    TargetCircle = expectedTargetCircle
                }
            };

            _mockRepoRateRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataAccess.Models.RepoRate, bool>>>()))
                .ReturnsAsync(repoRates);

            // Act
            var result = await _targetCircleService.GetTargetCircleAsync(counterpartyId, tradeDate);

            // Assert
            Assert.AreEqual(expectedTargetCircle, result);
        }

        [Test]
        public async Task GetCounterpartyExposureAsync_ReturnsCorrectExposureData()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var currentExposure = 4000000m; // 4M
            var targetCircle = 10m; // 10M

            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate))
                .ReturnsAsync(currentExposure);

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate
                {
                    CounterpartyId = counterpartyId,
                    EffectiveDate = tradeDate,
                    TargetCircle = targetCircle
                }
            };

            _mockRepoRateRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataAccess.Models.RepoRate, bool>>>()))
                .ReturnsAsync(repoRates);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(counterpartyId))
                .ReturnsAsync(new DataAccess.Models.Counterparty { Id = counterpartyId, CounterpartyName = "Test Counterparty" });

            // Act
            var result = await _targetCircleService.GetCounterpartyExposureAsync(counterpartyId, tradeDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(counterpartyId, result.CounterpartyId);
            Assert.AreEqual("Test Counterparty", result.CounterpartyName);
            Assert.AreEqual(currentExposure, result.CurrentExposure);
            Assert.AreEqual(targetCircle, result.TargetCircle);
            Assert.AreEqual(6000000m, result.AvailableLimit); // 10M - 4M
            Assert.AreEqual(40m, result.UtilizationPercentage); // (4M/10M)*100
            Assert.IsFalse(result.IsLimitBreached);
        }

        [Test]
        public async Task IsTradeWithinTargetCircleAsync_WhenWithinLimit_ReturnsTrue()
        {
            // Arrange
            var counterpartyId = 1;
            var tradeDate = DateTime.Today;
            var proposedNotional = 1000000m; // 1M

            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate))
                .ReturnsAsync(5000000m); // 5M current exposure

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate
                {
                    CounterpartyId = counterpartyId,
                    EffectiveDate = tradeDate,
                    TargetCircle = 10m // 10M
                }
            };

            _mockRepoRateRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataAccess.Models.RepoRate, bool>>>()))
                .ReturnsAsync(repoRates);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(counterpartyId))
                .ReturnsAsync(new DataAccess.Models.Counterparty { Id = counterpartyId, CounterpartyName = "Test Counterparty" });

            // Act
            var result = await _targetCircleService.IsTradeWithinTargetCircleAsync(counterpartyId, tradeDate, proposedNotional);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetLimitBreachesAsync_ReturnsOnlyBreachedCounterparties()
        {
            // Arrange
            var tradeDate = DateTime.Today;
            var activeCounterparties = new List<DataAccess.Models.Counterparty>
            {
                new DataAccess.Models.Counterparty { Id = 1, CounterpartyName = "Counterparty 1", IsActive = true },
                new DataAccess.Models.Counterparty { Id = 2, CounterpartyName = "Counterparty 2", IsActive = true }
            };

            _mockCounterpartyRepository
                .Setup(x => x.GetActiveCounterpartiesAsync())
                .ReturnsAsync(activeCounterparties);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(activeCounterparties[0]);

            _mockCounterpartyRepository
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(activeCounterparties[1]);

            // Counterparty 1: within limit (5M exposure, 10M target)
            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(1, tradeDate))
                .ReturnsAsync(5000000m);

            // Counterparty 2: exceeds limit (12M exposure, 10M target)
            _mockRepoTradeRepository
                .Setup(x => x.GetTotalNotionalByCounterpartyAndDateAsync(2, tradeDate))
                .ReturnsAsync(12000000m);

            var repoRates = new List<DataAccess.Models.RepoRate>
            {
                new DataAccess.Models.RepoRate { CounterpartyId = 1, EffectiveDate = tradeDate, TargetCircle = 10m },
                new DataAccess.Models.RepoRate { CounterpartyId = 2, EffectiveDate = tradeDate, TargetCircle = 10m }
            };

            _mockRepoRateRepository
                .SetupSequence(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataAccess.Models.RepoRate, bool>>>()))
                .ReturnsAsync(repoRates.Where(rr => rr.CounterpartyId == 1).ToList())
                .ReturnsAsync(repoRates.Where(rr => rr.CounterpartyId == 2).ToList());

            // Act
            var result = await _targetCircleService.GetLimitBreachesAsync(tradeDate);

            // Assert
            Assert.IsNotNull(result);
            var breaches = result.ToList();
            Assert.AreEqual(1, breaches.Count);
            Assert.AreEqual(2, breaches[0].CounterpartyId); // Only counterparty 2 should be in breach
            Assert.IsTrue(breaches[0].IsLimitBreached);
        }
    }
}
