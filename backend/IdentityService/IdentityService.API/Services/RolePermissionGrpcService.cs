using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IdentityService.Application.Features.Permission.Commands.UpdateInfo;
using IdentityService.Application.Features.Permission.Queries.GetAllGrouped;
using IdentityService.Application.Features.Roles.Commands.UpdateRolePermissions;
using IdentityService.Application.Features.Roles.Queries.GetPermissionsByRoleId;
using IdentityService.Application.Features.Roles.Queries.GetPermissionsBySlug;
using IdentityService.Grpc.Protos.RolePermission;
using MediatR;

namespace IdentityService.API.Services
{
    public class RolePermissionGrpcService : RolePermissionGrpc.RolePermissionGrpcBase
    {
        private readonly IMediator _mediator;

        public RolePermissionGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Get Permissions by Role Id
        public override async Task<GetPermissionsByRoleIdResponse> GetPermissionsByRoleId(
            GetPermissionsByRoleIdRequest request,
            ServerCallContext context)
        {
            // Validate Guid
            if (!Guid.TryParse(request.Id, out var roleId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID không hợp lệ"));
            }
            var query = new GetPermissionsByRoleIdQuery
            {
                Id = roleId
            };
            var groupedPermissions = await _mediator.Send(query);
            var response = new GetPermissionsByRoleIdResponse();
            foreach (var group in groupedPermissions)
            {
                var protoGroup = new RolePermissionGroupRes
                {
                    Resource = group.Resource
                };
                foreach (var perm in group.Permissions)
                {
                    protoGroup.Permissions.Add(new RolePermissionRes
                    {
                        Id = perm.Id.ToString(),
                        Slug = perm.Slug,
                        Name = perm.Name ?? string.Empty,
                        Description = perm.Description ?? string.Empty,
                        IsActived = perm.IsActived
                    });
                }
                response.Groups.Add(protoGroup);
            }
            return response;
        }

        // 1.5. Get Cache (Lazy Loading)
        public override async Task<GetPermissionsBySlugResponse> GetPermissionsBySlug(
            GetPermissionsBySlugRequest request,
            ServerCallContext context)
        {
            var query = new GetPermissionsBySlugQuery
            {
                RoleSlug = request.RoleSlug
            };

            var permissionSlugs = await _mediator.Send(query);

            var response = new GetPermissionsBySlugResponse();
            response.Permissions.AddRange(permissionSlugs);

            return response;
        }

        // 2. Update Info (Name, Description)
        public override async Task<Empty> UpdatePermissionInfo(
            UpdatePermissionInfoRequest request,
            ServerCallContext context)
        {
            // Validate Guid
            if (!Guid.TryParse(request.Id, out var permissionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID không hợp lệ"));
            }

            var command = new UpdatePermissionInfoCommand
            {
                Id = permissionId,
                Name = request.HasName ? request.Name : null,
                Description = request.HasDescription ? request.Description : null
            };

            await _mediator.Send(command);

            return new Empty();
        }

        // 3. Update Permissions cho Role
        public override async Task<Empty> UpdateRolePermissions(
            UpdateRolePermissionsRequest request,
            ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var roleId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID không hợp lệ"));

            var command = new UpdateRolePermissionsCommand
            {
                Id = roleId,
                PermissionsToGrant = request.PermissionsToGrant.ToList(),
                PermissionsToRevoke = request.PermissionsToRevoke.ToList()
            };

            await _mediator.Send(command);

            return new Empty();
        }

        // 4. Get All Grouped (Cho UI Matrix)
        public override async Task<GetAllPermissionsGroupedResponse> GetAllPermissionsGrouped(
            Empty request,
            ServerCallContext context)
        {
            // Gọi Query lấy List<PermissionGroupDto>
            var groupsDto = await _mediator.Send(new GetPermissionsByIdQuery());

            var response = new GetAllPermissionsGroupedResponse();

            foreach (var groupDto in groupsDto)
            {
                var protoGroup = new PermissionGroup
                {
                    Resource = groupDto.Resource
                };

                foreach (var permDto in groupDto.Permissions)
                {
                    protoGroup.Permissions.Add(new PermissionDetail
                    {
                        Id = permDto.Id.ToString(),
                        Slug = permDto.Slug,
                        Name = permDto.Name ?? string.Empty,
                        Description = permDto.Description ?? string.Empty
                    });
                }

                response.Groups.Add(protoGroup);
            }

            return response;
        }
    }
}