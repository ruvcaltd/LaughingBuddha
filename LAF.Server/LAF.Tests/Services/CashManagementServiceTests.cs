using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using LAF.DataAccess.Models;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Services.Services;

namespace LAF.Tests.Services
{
    [TestFixture]
    public class CashManagementServiceTests
    {
        private Mock<ICashAccountRepository> _mockCashAccountRepository;
        private Mock<ICashflowRepository> _mockCashflowRepository;
        private Mock<IFundRepository> _mockFundRepository;
        private Mock<IRepoTradeRepository> _mockRepoTradeRepository;
        private Mock<ILogger<CashManagementService>> _mockLogger;
        private CashManagementService _cashManagementService;

        [SetUp]
        public void Setup()
        {
            _mockCashAccountRepository = new Mock<ICashAccountRepository>();
            _mockCashflowRepository = new Mock<ICashflowRepository>();
            _mockFundRepository = new Mock<IFundRepository>();
            _mockRepoTradeRepository = new Mock<IRepoTradeRepository>();
            _mockLogger = new Mock<ILogger<CashManagementService>>();

            _cashManagementService = new CashManagementService(
                _mockCashAccountRepository.Object,
                _mockCashflowRepository.Object,
                _mockFundRepository.Object,
                _mockRepoTradeRepository.Object,
                null, // DbContext will be mocked
                _mockLogger.Object);
        }

        [Test]
        public async Task GetCashAccountBalanceAsync_ReturnsCorrectBalance()
        {
            // Arrange
            var cashAccountId = 1;
            var asOfDate = DateTime.Today;
            var expectedBalance = 1000000m; // 1M

            var cashAccount = new CashAccount
            {
                Id = cashAccountId,
                AccountName = "ACC001",
                FundId = 1,
                CurrencyCode = "USD",
                Fund = new Fund
                {
                    Id = 1,
                    FundCode = "FUND1",
                    FundName = "Test Fund",
                    CurrencyCode = "USD"
                }
            };

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(cashAccountId))
                .ReturnsAsync(cashAccount);

            _mockCashflowRepository
                .Setup(x => x.GetNetCashflowByAccountAsync(cashAccountId, asOfDate))
                .ReturnsAsync(expectedBalance);

            // Act
            var result = await _cashManagementService.GetCashAccountBalanceAsync(cashAccountId, asOfDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cashAccountId, result.CashAccountId);
            Assert.AreEqual("ACC001", result.AccountName);
            Assert.AreEqual(1, result.FundId);
            Assert.AreEqual("FUND1", result.FundCode);
            Assert.AreEqual("Test Fund", result.FundName);
            Assert.AreEqual("USD", result.CurrencyCode);
            Assert.AreEqual(expectedBalance, result.CurrentBalance);
            Assert.AreEqual(asOfDate, result.AsOfDate);
        }

        [Test]
        public async Task GetFundBalanceAsync_ReturnsCorrectFundBalance()
        {
            // Arrange
            var fundId = 1;
            var asOfDate = DateTime.Today;
            var expectedTotalBalance = 2500000m; // 2.5M

            var fund = new Fund
            {
                Id = fundId,
                FundCode = "FUND1",
                FundName = "Test Fund",
                CurrencyCode = "USD"
            };

            var cashAccounts = new List<CashAccount>
            {
                new CashAccount { Id = 1, FundId = fundId, CurrencyCode = "USD", AccountName = "ACC001" },
                new CashAccount { Id = 2, FundId = fundId, CurrencyCode = "USD", AccountName = "ACC002" }
            };

            _mockFundRepository
                .Setup(x => x.GetByIdAsync(fundId))
                .ReturnsAsync(fund);

            _mockCashAccountRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CashAccount, bool>>>()))
                .ReturnsAsync(cashAccounts);

            // Mock individual account balances
            _mockCashflowRepository
                .Setup(x => x.GetNetCashflowByAccountAsync(1, asOfDate))
                .ReturnsAsync(1000000m); // 1M

            _mockCashflowRepository
                .Setup(x => x.GetNetCashflowByAccountAsync(2, asOfDate))
                .ReturnsAsync(1500000m); // 1.5M

            // Mock individual cash account lookups
            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(cashAccounts[0]);

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(cashAccounts[1]);

            // Act
            var result = await _cashManagementService.GetFundBalanceAsync(fundId, asOfDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fundId, result.FundId);
            Assert.AreEqual("FUND1", result.FundCode);
            Assert.AreEqual("Test Fund", result.FundName);
            Assert.AreEqual("USD", result.CurrencyCode);
            Assert.AreEqual(expectedTotalBalance, result.AvailableCash);
            Assert.AreEqual(asOfDate, result.AsOfDate);
        }

        [Test]
        public async Task CheckFundFlatnessAsync_WhenFundIsFlat_ReturnsCorrectResult()
        {
            // Arrange
            var fundId = 1;
            var checkDate = DateTime.Today;

            var fund = new Fund
            {
                Id = fundId,
                FundCode = "FUND1",
                FundName = "Test Fund",
                CurrencyCode = "USD"
            };

            _mockFundRepository
                .Setup(x => x.GetByIdAsync(fundId))
                .ReturnsAsync(fund);

            // Mock fund balance as flat (0)
            var cashAccounts = new List<CashAccount>
            {
                new CashAccount { Id = 1, FundId = fundId, CurrencyCode = "USD", AccountName = "ACC001" }
            };

            _mockCashAccountRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CashAccount, bool>>>()))
                .ReturnsAsync(cashAccounts);


            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(cashAccounts[0]);

            _mockCashflowRepository
                .Setup(x => x.GetNetCashflowByAccountAsync(1, checkDate))
                .ReturnsAsync(0m);

            // Act
            var result = await _cashManagementService.CheckFundFlatnessAsync(fundId, checkDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fundId, result.FundId);
            Assert.AreEqual("FUND1", result.FundCode);
            Assert.AreEqual("Test Fund", result.FundName);
            Assert.AreEqual("USD", result.Currency);
            Assert.AreEqual(0m, result.CurrentBalance);
            Assert.IsTrue(result.IsFlat);
            Assert.AreEqual(0m, result.RequiredAdjustment);
            Assert.IsNull(result.AdjustmentType);
        }

        [Test]
        public async Task CheckFundFlatnessAsync_WhenFundIsNotFlat_ReturnsCorrectResult()
        {
            // Arrange
            var fundId = 1;
            var checkDate = DateTime.Today;
            var currentBalance = 500000m; // 500K

            var fund = new Fund
            {
                Id = fundId,
                FundCode = "FUND1",
                FundName = "Test Fund",
                CurrencyCode = "USD"
            };

            _mockFundRepository
                .Setup(x => x.GetByIdAsync(fundId))
                .ReturnsAsync(fund);

            var cashAccounts = new List<CashAccount>
            {
                new CashAccount { Id = 1, FundId = fundId, CurrencyCode = "USD", AccountName = "ACC001" }
            };

            _mockCashAccountRepository
                .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CashAccount, bool>>>()))
                .ReturnsAsync(cashAccounts);

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(cashAccounts[0]);

            _mockCashflowRepository
                .Setup(x => x.GetNetCashflowByAccountAsync(1, checkDate))
                .ReturnsAsync(currentBalance);

            // Act
            var result = await _cashManagementService.CheckFundFlatnessAsync(fundId, checkDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fundId, result.FundId);
            Assert.AreEqual(currentBalance, result.CurrentBalance);
            Assert.IsFalse(result.IsFlat);
            Assert.AreEqual(-currentBalance, result.RequiredAdjustment); // Negative adjustment needed
            Assert.AreEqual("Repo", result.AdjustmentType); // Positive balance means invest in repo
        }

        [Test]
        public async Task CreateCashflowAsync_WithValidData_CreatesCashflowSuccessfully()
        {
            // Arrange
            var createDto = new CreateCashflowDto
            {
                CashAccountId = 1,
                FundId = 1,
                Amount = 100000m, // 100K
                CurrencyCode = "USD",
                CashflowDate = DateTime.Today,
                Description = "Test cashflow",
                Source = "Manual",
                CreatedByUserId = 1
            };

            var cashAccount = new CashAccount
            {
                Id = 1,
                FundId = 1,
                CurrencyCode = "USD"
            };

            var fund = new Fund
            {
                Id = 1,
                CurrencyCode = "USD"
            };

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(createDto.CashAccountId))
                .ReturnsAsync(cashAccount);

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(cashAccount);


            _mockFundRepository
                .Setup(x => x.GetByIdAsync(createDto.FundId))
                .ReturnsAsync(fund);

            var expectedCashflow = new Cashflow
            {
                Id = 1,
                CashAccountId = createDto.CashAccountId,
                FundId = createDto.FundId,
                Amount = createDto.Amount,
                CurrencyCode = createDto.CurrencyCode,
                CashflowDate = createDto.CashflowDate,
                Description = createDto.Description,
                CashflowType = createDto.Source,
                CreatedBy = createDto.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = createDto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            _mockCashflowRepository
                .Setup(x => x.AddAsync(It.IsAny<Cashflow>()))
                .ReturnsAsync(expectedCashflow);

            // Act
            var result = await _cashManagementService.CreateCashflowAsync(createDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createDto.CashAccountId, result.CashAccountId);
            Assert.AreEqual(createDto.FundId, result.FundId);
            Assert.AreEqual(createDto.Amount, result.Amount);
            Assert.AreEqual(createDto.CurrencyCode, result.CurrencyCode);
            Assert.AreEqual(createDto.CashflowDate, result.EffectiveDate);
            Assert.AreEqual(createDto.Description, result.Description);
            Assert.AreEqual(createDto.Source, result.Source);
        }

        [Test]
        public void CreateCashflowAsync_WhenCashAccountNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var createDto = new CreateCashflowDto
            {
                CashAccountId = 999, // Non-existent
                FundId = 1,
                Amount = 100000m,
                CurrencyCode = "USD",
                CashflowDate = DateTime.Today,
                Description = "Test cashflow",
                Source = "Manual",
                CreatedByUserId = 1
            };

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(createDto.CashAccountId))
                .ReturnsAsync((CashAccount)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _cashManagementService.CreateCashflowAsync(createDto));

            StringAssert.Contains("Cash account with ID 999 not found", ex.Message);
        }

        [Test]
        public void CreateCashflowAsync_WhenCurrencyMismatch_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateCashflowDto
            {
                CashAccountId = 1,
                FundId = 1,
                Amount = 100000m,
                CurrencyCode = "EUR", // Different from fund currency
                CashflowDate = DateTime.Today,
                Description = "Test cashflow",
                Source = "Manual",
                CreatedByUserId = 1
            };

            var cashAccount = new CashAccount
            {
                Id = 1,
                FundId = 1,
                CurrencyCode = "USD"
            };

            var fund = new Fund
            {
                Id = 1,
                CurrencyCode = "USD"
            };

            _mockCashAccountRepository
                .Setup(x => x.GetByIdAsync(createDto.CashAccountId))
                .ReturnsAsync(cashAccount);

            _mockFundRepository
                .Setup(x => x.GetByIdAsync(createDto.FundId))
                .ReturnsAsync(fund);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _cashManagementService.CreateCashflowAsync(createDto));

            StringAssert.Contains("Cashflow currency EUR does not match fund currency USD", ex.Message);
        }

        [Test]
        public async Task GetFundCashflowSummaryAsync_ReturnsCorrectSummary()
        {
            // Arrange
            var fundId = 1;
            var fromDate = DateTime.Today.AddDays(-30);
            var toDate = DateTime.Today;

            var fund = new Fund
            {
                Id = fundId,
                FundCode = "FUND1",
                FundName = "Test Fund",
                CurrencyCode = "USD"
            };

            var cashflows = new List<Cashflow>
            {
                new Cashflow { Amount = 100000m, CashflowDate = DateTime.Today.AddDays(-15) }, // Inflow
                new Cashflow { Amount = -50000m, CashflowDate = DateTime.Today.AddDays(-10) }, // Outflow
                new Cashflow { Amount = 200000m, CashflowDate = DateTime.Today.AddDays(-5) },  // Inflow
                new Cashflow { Amount = -75000m, CashflowDate = DateTime.Today.AddDays(-2) }   // Outflow
            };

            _mockFundRepository
                .Setup(x => x.GetByIdAsync(fundId))
                .ReturnsAsync(fund);

            _mockCashflowRepository
                .Setup(x => x.GetCashflowsByFundAsync(fundId))
                .ReturnsAsync(cashflows);

            // Act
            var result = await _cashManagementService.GetFundCashflowSummaryAsync(fundId, fromDate, toDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fundId, result.FundId);
            Assert.AreEqual("FUND1", result.FundCode);
            Assert.AreEqual("Test Fund", result.FundName);
            Assert.AreEqual("USD", result.CurrencyCode);
            Assert.AreEqual(300000m, result.TotalInflows); // 100K + 200K
            Assert.AreEqual(125000m, result.TotalOutflows); // 50K + 75K
            Assert.AreEqual(175000m, result.NetCashflow); // 300K - 125K
            Assert.AreEqual(fromDate, result.DateFrom);
            Assert.AreEqual(toDate, result.DateTo);
        }
    }
}