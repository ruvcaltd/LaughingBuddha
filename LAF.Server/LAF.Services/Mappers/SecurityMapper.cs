using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class SecurityMapper
    {
        public static SecurityDto? ToDto(Security entity)
        {
            if (entity == null)
                return null;

            return new SecurityDto
            {
                Id = entity.Id,
                Isin = entity.Isin,
                Description = entity.Description,
                AssetType = entity.AssetType,
                Issuer = entity.Issuer,
                Currency = entity.Currency,
                MaturityDate = entity.MaturityDate,
                CreatedDate = entity.CreatedAt.Date,
                ModifiedDate = entity.ModifiedAt?.Date,
                CreatedByUserId = entity.CreatedBy ?? 1,
                ModifiedByUserId = entity.ModifiedBy ?? 1
            };
        }

        public static IEnumerable<SecurityDto> ToDtoList(IEnumerable<Security> entities)
        {
            if (entities == null)
                return Enumerable.Empty<SecurityDto>();

            return entities.Select(ToDto);
        }

        public static Security? ToEntity(CreateSecurityDto dto)
        {
            if (dto == null)
                return null;

            return new Security
            {
                Isin = dto.Isin,
                Description = dto.Description,
                AssetType = dto.AssetType,
                Issuer = dto.Issuer,
                Currency = dto.Currency,
                MaturityDate = dto.MaturityDate,
                CreatedAt = DateTime.Today,
                ModifiedAt = DateTime.Today,
                CreatedBy = dto.CreatedByUserId,
                ModifiedBy = dto.CreatedByUserId
            };
        }

        public static void UpdateEntity(Security entity, UpdateSecurityDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.Description = dto.Description;
            entity.AssetType = dto.AssetType;
            entity.Issuer = dto.Issuer;
            entity.Currency = dto.Currency;
            entity.MaturityDate = dto.MaturityDate;
            entity.ModifiedBy = dto.ModifiedByUserId;
            entity.ModifiedAt = DateTime.UtcNow;
        }
    }
}