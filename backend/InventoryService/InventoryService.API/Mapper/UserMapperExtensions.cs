using Google.Protobuf.WellKnownTypes;
using IdentityService.Grpc.Protos.User;
using InventoryService.Application.Dtos.User;

namespace InventoryService.API.Mapper
{
    public static class UserMapperExtensions
    {
        public static UserResponse ToProto(this UserRep entity)
        {
            if (entity == null) return null;

            var response = new UserResponse
            {
                Id = entity.Id.ToString(),
                Email = entity.Email,
                Username = entity.Username,
                FirstName = entity.FirstName,
                LastName = entity.LastName ?? "", // Proto không nhận null string
                Phone = entity.Phone ?? "",
                Address = entity.Address ?? "",
                Gender = entity.Gender,
                IsBlocked = entity.IsBlocked ?? false,

                // Chuyển đổi DateTime? (Nullable) sang Timestamp
                DateOfBirth = entity.DateOfBirth.HasValue
                    ? Timestamp.FromDateTime(DateTime.SpecifyKind(entity.DateOfBirth.Value, DateTimeKind.Utc))
                    : null,

                // Chuyển đổi DateTimeOffset sang Timestamp
                CreatedAt = Timestamp.FromDateTimeOffset(entity.CreatedAt),
                UpdatedAt = Timestamp.FromDateTimeOffset(entity.UpdatedAt)
            };

            // Map Object lồng nhau (Role)
            if (entity.Role != null)
            {
                response.Role = entity.Role.ToProto();
            }

            return response;
        }

        public static RoleUserResponse ToProto(this RoleUserRep entity)
        {
            if (entity == null) return null;

            return new RoleUserResponse
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Slug = entity.Slug
            };
        }

        public static UserRep ToEntity(this UserResponse proto)
        {
            if (proto == null) return null;

            return new UserRep
            {
                // Chuyển string sang Guid
                Id = Guid.TryParse(proto.Id, out var userId) ? userId : Guid.Empty,

                Email = proto.Email,
                Username = proto.Username,
                FirstName = proto.FirstName,
                LastName = string.IsNullOrEmpty(proto.LastName) ? null : proto.LastName,
                Phone = string.IsNullOrEmpty(proto.Phone) ? null : proto.Phone,
                Address = string.IsNullOrEmpty(proto.Address) ? null : proto.Address,
                Gender = proto.Gender,

                // Chuyển Timestamp sang DateTime? (Nullable)
                DateOfBirth = proto.DateOfBirth?.ToDateTime(),

                // Chuyển Timestamp sang DateTimeOffset
                CreatedAt = proto.CreatedAt?.ToDateTimeOffset() ?? DateTimeOffset.UtcNow,
                UpdatedAt = proto.UpdatedAt?.ToDateTimeOffset() ?? DateTimeOffset.UtcNow,

                IsBlocked = proto.IsBlocked,

                // Map Object lồng nhau
                Role = proto.Role?.ToEntity()
            };
        }

        public static RoleUserRep ToEntity(this RoleUserResponse proto)
        {
            if (proto == null) return null;

            return new RoleUserRep
            {
                Id = Guid.TryParse(proto.Id, out var roleId) ? roleId : Guid.Empty,
                Name = proto.Name,
                Slug = proto.Slug
            };
        }
    }
}
