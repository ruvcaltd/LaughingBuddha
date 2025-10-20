using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class CounterpartyMapper
    {
        public static CounterpartyDto ToDto(Counterparty entity)
        {
            if (entity == null) return null;

            return new CounterpartyDto
            {
                Id = entity.Id,
                Name = entity.CounterpartyName,
                ShortName = entity.CounterpartyCode,
                Country = entity.CountryCode,
                CreditRating = entity.CreditRating,
                Sector = entity.CounterpartyType,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedDate.ToString(), // No CreatedBy field, using date as placeholder
                ModifiedDate = entity.ModifiedDate,
                ModifiedBy = entity.ModifiedDate.ToString() // No ModifiedBy field, using date as placeholder
            };
        }

        public static List<CounterpartyDto> ToDtoList(IEnumerable<Counterparty> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<CounterpartyDto>();
        }

        public static Counterparty ToEntity(CreateCounterpartyDto dto)
        {
            if (dto == null) return null;

            return new Counterparty
            {
                CounterpartyName = dto.Name,
                CounterpartyCode = dto.ShortName,
                CountryCode = dto.Country,
                CreditRating = dto.CreditRating,
                CounterpartyType = dto.Sector,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
                // Note: No CreatedBy/ModifiedBy fields in entity
            };
        }

        public static void UpdateEntity(Counterparty entity, UpdateCounterpartyDto dto)
        {
            if (entity == null || dto == null) return;

            entity.CounterpartyCode = dto.ShortName;
            entity.CountryCode = dto.Country;
            entity.CreditRating = dto.CreditRating;
            entity.CounterpartyType = dto.Sector;
            entity.IsActive = dto.IsActive;
            entity.ModifiedDate = DateTime.UtcNow;
        }

        public static CounterpartyExposureDto ToExposureDto(VCounterpartyExposure entity)
        {
            if (entity == null) return null;

            return new CounterpartyExposureDto
            {
                CounterpartyId = entity.CounterpartyId,
                CounterpartyName = entity.CounterpartyName,
                TradeDate = entity.TradeDate,
                CurrentExposure = entity.CurrentExposure ?? 0,
                TargetCircle = entity.TargetCircle ?? 0,
                AvailableLimit = entity.AvailableLimit ?? 0,
                UtilizationPercentage = entity.UtilizationPercentage ?? 0,
                IsLimitBreached = entity.IsLimitBreached ?? false
            };
        }

        public static List<CounterpartyExposureDto> ToExposureDtoList(IEnumerable<VCounterpartyExposure> entities)
        {
            return entities?.Select(ToExposureDto).ToList() ?? new List<CounterpartyExposureDto>();
        }
    }
}