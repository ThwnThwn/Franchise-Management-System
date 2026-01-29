using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;
using IdentityService.Application.Features.Users.Commands.ChangePassword;
using IdentityService.Application.Features.Users.Commands.CustomerCreateUser;
using IdentityService.Application.Features.Users.Commands.DeleteUser;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Features.Users.Queries.GetAllUser;
using IdentityService.Application.Features.Users.Queries.GetManager;
using IdentityService.Application.Features.Users.Queries.GetUserById;
using IdentityService.Grpc.Protos.Role;
using IdentityService.Grpc.Protos.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Org.BouncyCastle.Ocsp;
using Sprache;
using System.Globalization;

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
        var dob = DateTime.ParseExact(
                request.DateOfBirth,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );
        var rep = await _mediator.Send(new CustomerCreateUserCommand(new()
        {
            Email = request.Email,
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
            FirstName = request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
            Gender = request.Gender,
            DateOfBirth = dob,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            RoleSlug = request.RoleSlug
        }));

        return MapUser(rep);
    }

    public override async Task<UserResponse> AdminCreate(AdminCreateUserRequest request, ServerCallContext context)
    {
        var dob = DateTime.ParseExact(
                request.DateOfBirth,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );
        var rep = await _mediator.Send(new AdminCreateUserCommand(new()
        {
            Email = request.Email,
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
            FirstName = request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
            Gender = request.Gender,
            DateOfBirth = dob,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            RoleSlug = request.RoleSlug
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
    public override async Task<GetAllUserOrWithRoleResponse> GetAll(GetAllUserOrWithRoleRequest request, ServerCallContext context)
    {
        var pagedResult = await _mediator.Send(
            new GetAllUsersQuery()
            {
                RoleSlug = request.RoleSlug,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize
            }
        );

        var res = new GetAllUserOrWithRoleResponse
        {
            TotalCount = (int)pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };

        res.Users.AddRange(pagedResult.Items.Select(u => new UserResponse
        {
            Id = u.Id.ToString(),
            Email = u.Email ?? "",
            Username = u.Username ?? "",
            Phone = u.Phone ?? "",
            Address = u.Address ?? "",
            IsBlocked = u.IsBlocked ?? false,

            FirstName = u.FirstName ?? "",
            LastName = u.LastName ?? "",
            Gender = u.Gender,

            DateOfBirth = u.DateOfBirth.HasValue
        ? Timestamp.FromDateTime(u.DateOfBirth.Value.ToUniversalTime())
        : null,

            Role = u.Role == null
            ? null
            : new RoleUserResponse
            {
                Id = u.Role.Id.ToString(),
                Slug = u.Role.Slug,
                Name = u.Role.Name
            },

            CreatedAt = Timestamp.FromDateTime(u.CreatedAt.UtcDateTime),
            UpdatedAt = Timestamp.FromDateTime(u.UpdatedAt.UtcDateTime),
        }));
        return res;
    }

    public override async Task<UserResponse> GetManager(GetManagerRequest request,ServerCallContext context)
    {
        var manager = await _mediator.Send(
            new GetManagerQuery(
                new GetManagerReq
                {
                    StoreId = Guid.Parse(request.Id)
                }
            )
        );
        if (manager == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Manager not found"));

        return MapUser(manager);
    }

    public override async Task<UserResponse> Update(UpdateUserRequest request, ServerCallContext context)
    {
        string[] formats = {
            "yyyy-MM-dd",
            "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
            "yyyy-MM-dd'T'HH:mm:ss'Z'"
        };

        var dob = DateTime.ParseExact(
            request.DateOfBirth,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind
        );

        _ = Guid.TryParse(request.Id, out var customerId);
        var rep = await _mediator.Send(new UpdateUserCommand(
            Id: customerId,
            Request: new()
            {
                Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                FirstName = request.FirstName,
                LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName,
                Gender = request.Gender,
                DateOfBirth = dob,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
                Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
                IsBlocked = request.IsBlocked,
                RoleSlug = request.RoleSlug
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
            Gender = user.Gender,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            DateOfBirth = user.DateOfBirth.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(user.DateOfBirth.Value, DateTimeKind.Utc))
                : null,
            CreatedAt = Timestamp.FromDateTime(user.CreatedAt.UtcDateTime),
            UpdatedAt = Timestamp.FromDateTime(user.UpdatedAt.UtcDateTime),
            Role = user.Role == null
                ? new Grpc.Protos.Role.RoleUserResponse()
                : new Grpc.Protos.Role.RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? "",
                    Slug = user.Role.Slug ?? ""
                }
        };
    }
}