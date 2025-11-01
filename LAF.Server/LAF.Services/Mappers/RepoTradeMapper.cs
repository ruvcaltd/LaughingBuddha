using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class RepoTradeMapper
    {
        public static RepoTradeDto ToDto(RepoTrade entity)
        {
            if (entity == null) return null;

            return new RepoTradeDto
            {
                Id = entity.Id,
                TradeReference = $"TRD{entity.Id:D6}", // Generate reference from ID
                FundId = entity.FundId ?? 0,
                FundCode = entity.Fund?.FundCode,
                FundName = entity.Fund?.FundName,
                CounterpartyId = entity.CounterpartyId ?? 0,
                CounterpartyName = entity.Counterparty?.CounterpartyName,
                SecurityId = (int)entity.SecurityId,
                SecurityIsin = entity.Security?.Isin,
                SecurityName = entity.Security?.Description,
                CollateralTypeId = entity.CollateralTypeId ?? 0,
                CollateralTypeName = entity.CollateralType?.CollateralType1,
                Direction = entity.Direction ?? string.Empty,
                Notional = entity.Notional,
                Rate = entity.Rate,
                StartDate = entity.StartDate?.DateTime ?? DateTime.MinValue,
                EndDate = entity.MaturityDate,
                SettlementDate = entity.TradeDate, // Using TradeDate as SettlementDate
                Status = entity.Status ?? "Unknown",
                Currency = entity.Fund?.CurrencyCode ?? "USD", // Get from fund currency
                Haircut = null, // No Haircut field in entity
                CollateralValue = null, // No CollateralValue field in entity
                CreatedDate = entity.CreatedAt.DateTime,
                CreatedBy = entity.CreatedBy?.ToString(),
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedBy?.ToString(),
                Security = entity.Security != null ? SecurityMapper.ToDto(entity.Security) : null
            };
        }

        public static List<RepoTradeDto> ToDtoList(IEnumerable<RepoTrade> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<RepoTradeDto>();
        }

        public static RepoTrade ToEntity(CreateRepoTradeDto dto)
        {
            if (dto == null) return null;

            return new RepoTrade
            {
                FundId = dto.FundId,
                CounterpartyId = dto.CounterpartyId,
                SecurityId = dto.SecurityId,
                CollateralTypeId = (short)dto.CollateralTypeId,
                Direction = dto.Direction,
                Notional = dto.Notional,
                Rate = dto.Rate,
                StartDate = dto.StartDate,
                MaturityDate = dto.EndDate, // Map EndDate to MaturityDate
                TradeDate = dto.StartDate, // Map SettlementDate to TradeDate
                Status = "Pending",
                CreatedBy = dto.CreatedByUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedBy = dto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(RepoTrade entity, UpdateRepoTradeDto dto)
        {
            if (entity == null || dto == null) return;

            entity.Notional = dto.Notional;
            entity.Rate = dto.Rate;
            entity.StartDate = dto.StartDate;
            entity.MaturityDate = dto.EndDate; // Map EndDate to MaturityDate
            // Note: Cannot update TradeDate as it represents original trade date
            entity.ModifiedBy = dto.ModifiedByUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;
        }
    }
}