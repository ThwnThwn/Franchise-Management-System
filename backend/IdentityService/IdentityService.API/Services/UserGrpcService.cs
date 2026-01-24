using Grpc.Core;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Grpc.Protos.User;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Grpc.Services;

[Authorize]
public class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IUserService _userService;

    public UserGrpcService(IUserService userService)
    {
        _userService = userService;
    }

    public override async Task<UserResponse> CustomerCreate(CustomerCreateUserRequest request, ServerCallContext context)
    {
        var rep = await _userService.CustomerCreateAsync(new CustomerCreateUserReq
        {
            Email = request.Email,
            Password = request.Password,
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address
        });

        return MapUser(rep);
    }

    public override async Task<UserResponse> AdminCreate(AdminCreateUserRequest request, ServerCallContext context)
    {
        var rep = await _userService.AdminCreateAsync(new CreateUserReq
        {
            Email = request.Email,
            Password = request.Password,
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            RoleId = string.IsNullOrWhiteSpace(request.RoleId) ? null : Guid.Parse(request.RoleId)
        });

        return MapUser(rep);
    }

    public override async Task<UserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        var rep = await _userService.GetByIdAsync(Guid.Parse(request.Id));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }

    public override async Task<UserListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var users = await _userService.GetAllAsync();

        var res = new UserListResponse();
        res.Users.AddRange(users.Select(MapUser));

        return res;
    }

    public override async Task<UserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        var rep = await _userService.UpdateAsync(Guid.Parse(request.Id), new UpdateUserReq
        {
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            IsBlocked = request.IsBlocked, //Cannot be null
            RoleId = string.IsNullOrWhiteSpace(request.RoleId) ? null : Guid.Parse(request.RoleId)
        });

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }

    public override async Task<DeleteUserResponse> Delete(DeleteUserRequest request, ServerCallContext context)
    {
        var ok = await _userService.DeleteAsync(Guid.Parse(request.Id));

        return new DeleteUserResponse
        {
            Success = ok
        };
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
            CreatedAt = user.CreatedAt.ToString("O"),
            UpdatedAt = user.UpdatedAt.ToString("O"),
            Role = user.Role == null
                ? new IdentityService.Grpc.Protos.Role.RoleUserResponse()
                : new IdentityService.Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? ""
                }
        };
    }
}
