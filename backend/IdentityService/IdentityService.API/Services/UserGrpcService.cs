using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;
using IdentityService.Application.Features.Users.Commands.ChangePassword;
using IdentityService.Application.Features.Users.Commands.CustomerCreateUser;
using IdentityService.Application.Features.Users.Commands.DeleteUser;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Features.Users.Queries.GetAllUser;
using IdentityService.Application.Features.Users.Queries.GetUserById;
using IdentityService.Grpc.Protos.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace IdentityService.API.Services;
public class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IMediator _mediator;

    public UserGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<UserResponse> CustomerCreate(CustomerCreateUserRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new CustomerCreateUserCommand(new()
        {
            Email = request.Email,
            Password = request.Password,
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
            FirstName = request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth.ToDateTime(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address
        }));

        return MapUser(rep);
    }

    public override async Task<UserResponse> AdminCreate(AdminCreateUserRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new AdminCreateUserCommand(new()
        {
            Email = request.Email,
            Password = request.Password,
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
            FirstName = request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth.ToDateTime(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            RoleId = string.IsNullOrWhiteSpace(request.RoleId) ? null : Guid.Parse(request.RoleId)
        }));

        return MapUser(rep);
    }

    public override async Task<UserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new GetUserByIdQuery(Guid.Parse(request.Id)));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }

    public override async Task<UserListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var users = await _mediator.Send(new GetAllUsersQuery());

        var res = new UserListResponse();
        res.Users.AddRange(users.Select(MapUser));
        return res;
    }

    public override async Task<UserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new UpdateUserCommand(
            Id: Guid.Parse(request.Id),
            Request: new()
            {
                Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                FirstName = request.FirstName,
                LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth.ToDateTime(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
                Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
                IsBlocked = request.IsBlocked,
                RoleId = string.IsNullOrWhiteSpace(request.RoleId) ? null : Guid.Parse(request.RoleId)
            }
        ));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }

    public override async Task<UserResponse> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
    {
        var rep = await _mediator.Send(new ChangePasswordComand(
            Id: Guid.Parse(request.Id),
            Request: new()
            {
                OldPassword = request.OldPassword,
                NewPassword = request.NewPassword,
                ConfirmNewPassword = request.ConfirmNewPassword,
            }
            ));
        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }


    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        var ok = await _mediator.Send(new DeleteUserCommand(Guid.Parse(request.Id)));

        return new DeleteUserResponse { Success = ok };
    }

    private static UserResponse MapUser(UserRep user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            Username = user.Username ?? "",
            Phone = user.Phone ?? "",
            Address = user.Address ?? "",
            IsBlocked = user.IsBlocked ?? false,
            CreatedAt = Timestamp.FromDateTime(user.CreatedAt.UtcDateTime),
            UpdatedAt = Timestamp.FromDateTime(user.UpdatedAt.UtcDateTime),
            Role = user.Role == null
                ? new Grpc.Protos.Role.RoleUserResponse()
                : new Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? ""
                }
        };
    }
}