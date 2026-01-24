using Grpc.Core;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Grpc.Protos.Role;

namespace IdentityService.Grpc.Services;

public class RoleGrpcService : RoleGrpc.RoleGrpcBase
{
    private readonly IRoleService _roleService;

    public RoleGrpcService(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override async Task<RoleResponse> Create(CreateRoleRequest request, ServerCallContext context)
    {
        var rep = await _roleService.CreateAsync(new CreateRoleReq
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description
        });

        return MapRole(rep);
    }

    public override async Task<RoleResponse> GetById(GetRoleByIdRequest request, ServerCallContext context)
    {
        var rep = await _roleService.GetByIdAsync(Guid.Parse(request.Id));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        return MapRole(rep);
    }

    public override async Task<RoleListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var roles = await _roleService.GetAllAsync();

        var res = new RoleListResponse();
        res.Roles.AddRange(roles.Select(MapRole));

        return res;
    }

    public override async Task<RoleResponse> Update(UpdateRoleRequest request, ServerCallContext context)
    {
        var rep = await _roleService.UpdateAsync(Guid.Parse(request.Id), new UpdateRoleReq
        {
            Name = request.Name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description
        });

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        return MapRole(rep);
    }

    public override async Task<DeleteRoleResponse> Delete(DeleteRoleRequest request, ServerCallContext context)
    {
        var ok = await _roleService.DeleteAsync(Guid.Parse(request.Id));

        return new DeleteRoleResponse
        {
            Success = ok
        };
    }

    private static RoleResponse MapRole(RoleRep role)
    {
        return new RoleResponse
        {
            Id = role.Id.ToString(),
            Name = role.Name ?? "",
            Slug = role.Slug ?? "",
            Description = role.Description ?? "",
            CreatedAt = role.CreatedAt.ToString("O"),
            UpdatedAt = role.UpdatedAt.ToString("O"),
        };
    }
}
