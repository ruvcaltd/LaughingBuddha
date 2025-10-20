using System;
using System.Collections.Generic;
using System.Linq;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Services.Mappers
{
    public static class CollateralTypeMapper
    {
        public static CollateralTypeDto ToDto(CollateralType entity)
        {
            if (entity == null) return null;

            return new CollateralTypeDto
            {
                Id = entity.Id,
                Name = entity.AssetType,
                Description = entity.CollateralType1 ?? "",
                StandardHaircut = null,
                IsActive = true,
                CreatedDate = entity.CreatedAt.DateTime,
                CreatedBy = entity.CreatedBy?.ToString() ?? "System",
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedBy?.ToString()
            };
        }

        public static List<CollateralTypeDto> ToDtoList(IEnumerable<CollateralType> entities)
        {
            return entities?.Select(ToDto).ToList() ?? new List<CollateralTypeDto>();
        }

        public static CollateralType ToEntity(CreateCollateralTypeDto dto)
        {
            if (dto == null) return null;

            return new CollateralType
            {
                AssetType = dto.Name,
                CollateralType1 = dto.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = dto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedBy = dto.CreatedByUserId
            };
        }

        public static void UpdateEntity(CollateralType entity, UpdateCollateralTypeDto dto)
        {
            if (entity == null || dto == null) return;

            entity.CollateralType1 = dto.Description;
            entity.ModifiedAt = DateTimeOffset.UtcNow;
            entity.ModifiedDate = DateTimeOffset.UtcNow;
            entity.ModifiedBy = dto.ModifiedByUserId;
        }
    }
}