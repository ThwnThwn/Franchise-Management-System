using Grpc.Core;
using IdentityService.Application.Features.Roles.Commands.CreateRole;
using IdentityService.Application.Features.Roles.Commands.UpdateRole;
using IdentityService.Application.Features.Roles.Commands.DeleteRole;
using IdentityService.Application.Features.Roles.Queries.GetRoleById;
using IdentityService.Application.Features.Roles.Queries.GetAllRoles;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Grpc.Protos.Role;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Google.Protobuf.WellKnownTypes;

namespace IdentityService.API.Services;

[Authorize]
public class RoleGrpcService : RoleGrpc.RoleGrpcBase
{
    private readonly IMediator _mediator;

    public RoleGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<RoleResponse> Create(CreateRoleRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new CreateRoleCommand(new()
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description
        }));

        return MapRole(rep);
    }

    public override async Task<RoleResponse> GetById(GetRoleByIdRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new GetRoleByIdQuery(Guid.Parse(request.Id)));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        return MapRole(rep);
    }

    public override async Task<RoleListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var roles = await _mediator.Send(new GetAllRolesQuery());

        var res = new RoleListResponse();
        res.Roles.AddRange(roles.Select(MapRole));
        return res;
    }

    public override async Task<RoleResponse> Update(UpdateRoleRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new UpdateRoleCommand(
            Id: Guid.Parse(request.Id),
            Request: new()
            {
                Name = request.Name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description
            }
        ));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        return MapRole(rep);
    }

    public override async Task<DeleteRoleResponse> Delete(DeleteRoleRequest request, ServerCallContext context)
    {
        var ok = await _mediator.Send(new DeleteRoleCommand(Guid.Parse(request.Id)));

        return new DeleteRoleResponse { Success = ok };
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