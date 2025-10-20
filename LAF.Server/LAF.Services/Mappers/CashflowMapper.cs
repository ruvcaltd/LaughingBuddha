using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class CashflowMapper
    {
        public static CashflowDto ToDto(Cashflow entity)
        {
            if (entity == null) return null;

            return new CashflowDto
            {
                Id = entity.Id,
                CashAccountId = entity.CashAccountId,
                AccountNumber = entity.CashAccount?.AccountName ?? string.Empty,
                FundId = entity.FundId ?? 0,
                FundCode = entity.Fund?.FundCode,
                FundName = entity.Fund?.FundName,
                RepoTradeId = entity.TradeId,
                TradeReference = entity.Trade?.Id.ToString() ?? string.Empty,
                Amount = entity.Amount,
                CurrencyCode = entity.CurrencyCode,
                EffectiveDate = entity.CashflowDate.UtcDateTime,
                Description = entity.Description ?? string.Empty,
                Source = entity.CashflowType ?? string.Empty,
                CreatedDate = entity.CreatedAt ?? DateTime.MinValue,
                CreatedBy = entity.CreatedBy?.ToString(),
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedBy?.ToString()
            };
        }

        public static List<CashflowDto> ToDtoList(IEnumerable<Cashflow> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<CashflowDto>();
        }

        public static Cashflow ToEntity(CreateCashflowDto dto)
        {
            if (dto == null) return null;

            return new Cashflow
            {
                CashAccountId = dto.CashAccountId,
                FundId = dto.FundId,
                TradeId = dto.RepoTradeId,
                Amount = dto.Amount,
                CurrencyCode = dto.CurrencyCode,
                CashflowDate = dto.CashflowDate,
                Description = dto.Description,
                CashflowType = dto.Source,
                CreatedBy = dto.CreatedByUserId,
                CreatedAt = dto.CashflowDate,
                ModifiedBy = dto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow,
                SettlementDate = dto.CashflowDate // Map effective date to settlement date
            };
        }

        public static void UpdateEntity(Cashflow entity, UpdateCashflowDto dto)
        {
            if (entity == null || dto == null) return;

            entity.Amount = dto.Amount;
            entity.Description = dto.Description;
            entity.ModifiedBy = dto.ModifiedByUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;
        }

        public static FundCashflowSummaryDto ToSummaryDto(int fundId, string fundCode, string fundName,
            string currencyCode, DateTime fromDate, DateTime toDate, IEnumerable<Cashflow> cashflows)
        {
            if (cashflows == null) cashflows = new List<Cashflow>();

            var totalInflows = cashflows.Where(cf => cf.Amount > 0).Sum(cf => cf.Amount);
            var totalOutflows = cashflows.Where(cf => cf.Amount < 0).Sum(cf => cf.Amount);

            return new FundCashflowSummaryDto
            {
                FundId = fundId,
                FundCode = fundCode,
                FundName = fundName,
                CurrencyCode = currencyCode,
                TotalInflows = totalInflows,
                TotalOutflows = Math.Abs(totalOutflows),
                NetCashflow = totalInflows + totalOutflows,
                DateFrom = fromDate,
                DateTo = toDate
            };
        }
    }
}