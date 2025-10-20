using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class FundMapper
    {
        public static FundDto ToDto(Fund entity)
        {
            if (entity == null) return null;

            return new FundDto
            {
                Id = entity.Id,
                FundCode = entity.FundCode,
                FundName = entity.FundName,
                CurrencyCode = entity.CurrencyCode,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedAt.DateTime,
                CreatedBy = entity.CreatedBy?.ToString(),
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedBy?.ToString()
            };
        }

        public static List<FundDto> ToDtoList(IEnumerable<Fund> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<FundDto>();
        }

        public static Fund ToEntity(CreateFundDto dto)
        {
            if (dto == null) return null;

            return new Fund
            {
                FundCode = dto.FundCode,
                FundName = dto.FundName,
                CurrencyCode = dto.CurrencyCode,
                IsActive = dto.IsActive,
                CreatedBy = dto.CreatedByUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedBy = dto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(Fund entity, UpdateFundDto dto)
        {
            if (entity == null || dto == null) return;

            entity.FundName = dto.FundName;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = dto.ModifiedByUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;
        }

        public static FundBalanceDto ToBalanceDto(VFundBalance entity)
        {
            if (entity == null) return null;

            return new FundBalanceDto
            {
                FundId = entity.FundId,
                FundCode = entity.FundCode ?? string.Empty,
                FundName = entity.FundName ?? string.Empty,
                CurrencyCode = entity.CurrencyCode ?? string.Empty,
                AvailableCash = entity.AvailableBalance,
                OpeningBalance = 0, // Not available in view
                AsOfDate = DateTime.UtcNow // Use current date as fallback
            };
        }

        public static List<FundBalanceDto> ToBalanceDtoList(IEnumerable<VFundBalance> entities)
        {
            return entities?.Select(ToBalanceDto).ToList() ?? new List<FundBalanceDto>();
        }
    }
}