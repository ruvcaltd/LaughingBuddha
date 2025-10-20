using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class CashAccountMapper
    {
        public static CashAccountDto ToDto(CashAccount entity)
        {
            if (entity == null) return null;

            return new CashAccountDto
            {
                Id = entity.Id,
                FundId = entity.FundId ?? 0,
                FundCode = entity.Fund?.FundCode,
                FundName = entity.Fund?.FundName,
                AccountNumber = entity.AccountName ?? string.Empty,
                CurrencyCode = entity.CurrencyCode,
                AccountType = entity.OwnerType ?? string.Empty,
                IsActive = true, // No IsActive field in entity, assume active
                CreatedDate = entity.CreatedAt.DateTime,
                CreatedBy = entity.CreatedAt.ToString(), // No CreatedBy field, using timestamp
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedAt?.ToString() // No ModifiedBy field, using timestamp
            };
        }

        public static List<CashAccountDto> ToDtoList(IEnumerable<CashAccount> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<CashAccountDto>();
        }

        public static CashAccount ToEntity(CreateCashAccountDto dto)
        {
            if (dto == null) return null;

            return new CashAccount
            {
                FundId = dto.FundId,
                AccountName = dto.AccountNumber,
                CurrencyCode = dto.CurrencyCode,
                OwnerType = dto.AccountType,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
                // Note: No CreatedBy/ModifiedBy fields in actual entity
            };
        }

        public static void UpdateEntity(CashAccount entity, UpdateCashAccountDto dto)
        {
            if (entity == null || dto == null) return;

            // Note: CashAccount doesn't have IsActive field, so we only update ModifiedAt
            entity.ModifiedAt = DateTimeOffset.UtcNow;
        }

        public static CashAccountBalanceDto ToBalanceDto(VCashAccountBalance entity)
        {
            if (entity == null) return null;

            return new CashAccountBalanceDto
            {
                CashAccountId = entity.CashAccountId,
                AccountName = entity.AccountName ?? string.Empty,
                FundId = entity.FundId ?? 0,
                FundCode = entity.FundCode ?? string.Empty,
                FundName = entity.FundName ?? string.Empty,
                CurrencyCode = entity.CurrencyCode,
                CurrentBalance = entity.Balance ?? 0,
                AsOfDate = DateTime.UtcNow // Use current date as fallback
            };
        }

        public static List<CashAccountBalanceDto> ToBalanceDtoList(IEnumerable<VCashAccountBalance> entities)
        {
            return entities?.Select(ToBalanceDto).ToList() ?? new List<CashAccountBalanceDto>();
        }
    }
}