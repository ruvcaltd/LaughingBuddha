using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class RepoRateMapper
    {
        public static RepoRateDto ToDto(RepoRate entity)
        {
            if (entity == null) return null;

            return new RepoRateDto
            {
                Id = (int)entity.Id,
                CounterpartyId = entity.CounterpartyId,
                CounterpartyName = entity.Counterparty?.CounterpartyName,
                CollateralTypeId = entity.CollateralTypeId,
                CollateralTypeName = entity.CollateralType?.CollateralType1,
                RepoDate = entity.EffectiveDate,
                RepoRate = entity.RepoRate1,
                TargetCircle = entity.TargetCircle,
                FinalCircle = entity.FinalCircle,
                CreatedDate = entity.CreatedAt.DateTime,
                ModifiedDate = entity.ModifiedAt?.DateTime
            };
        }

        public static List<RepoRateDto> ToDtoList(IEnumerable<RepoRate> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<RepoRateDto>();
        }

        public static RepoRate ToEntity(CreateRepoRateDto dto)
        {
            if (dto == null) return null;

            return new RepoRate
            {
                CounterpartyId = dto.CounterpartyId,
                CollateralTypeId = (short)dto.CollateralTypeId,
                EffectiveDate = dto.RepoDate,
                RepoRate1 = dto.RepoRate,
                TargetCircle = dto.TargetCircle,
                FinalCircle = dto.FinalCircle ?? 0,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(RepoRate entity, UpdateRepoRateDto dto)
        {
            if (entity == null || dto == null) return;

            entity.RepoRate1 = dto.RepoRate;
            entity.TargetCircle = dto.TargetCircle;
            entity.FinalCircle = dto.FinalCircle ?? 0;
            entity.ModifiedAt = DateTimeOffset.UtcNow;
        }

        public static TargetCircleValidationDto ToValidationDto(int counterpartyId, string counterpartyName, 
            DateTime tradeDate, decimal currentExposure, decimal proposedNotional, decimal targetCircle)
        {
            var newTotalExposure = currentExposure + proposedNotional;
            var isWithinLimit = newTotalExposure <= targetCircle * 1000000; // TargetCircle is in millions
            var utilizationPercentage = targetCircle > 0 ? (newTotalExposure / (targetCircle * 1000000)) * 100 : 0;

            return new TargetCircleValidationDto
            {
                CounterpartyId = counterpartyId,
                CounterpartyName = counterpartyName,
                TradeDate = tradeDate,
                CurrentExposure = currentExposure,
                ProposedNotional = proposedNotional,
                TargetCircle = targetCircle,
                NewTotalExposure = newTotalExposure,
                IsWithinLimit = isWithinLimit,
                LimitUtilizationPercentage = utilizationPercentage,
                ValidationMessage = isWithinLimit 
                    ? "Trade is within TargetCircle limit"
                    : $"Trade exceeds TargetCircle limit of {targetCircle}M by {newTotalExposure - (targetCircle * 1000000):C}"
            };
        }
    }
}