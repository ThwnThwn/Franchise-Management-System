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
using IdentityService.Application.Features.Users.Queries.GetByEmail;
using IdentityService.Application.Features.Users.Queries.GetByPhone;
using IdentityService.Application.Features.Users.Queries.GetManager;
using IdentityService.Application.Features.Users.Queries.GetUserById;
using IdentityService.Application.Features.Users.Queries.GetUserByIds;
using IdentityService.Application.Features.Users.Commands.UploadAvatar;
using IdentityService.Application.Features.Users.Commands.DeleteAvatar;
using IdentityService.Grpc.Protos.Role;
using IdentityService.Grpc.Protos.User;
using MediatR;
using Sprache;
using System.Globalization;
using Google.Protobuf;

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
            RoleSlug = request.RoleSlug,
            CurrentHourlyWage = request.HasCurrentHourlyWage? (decimal?)request.CurrentHourlyWage: null,
            HomeStoreId = Guid.Parse(string.IsNullOrWhiteSpace(request.HomeStoreId) ? null : request.HomeStoreId)

        }));

        return MapUser(rep);
    }

    public override async Task<UserResponse> GetById(GetUserByIdRequest request, ServerCallContext context)
    {
        try
        {
            var rep = await _mediator.Send(new GetUserByIdQuery(Guid.Parse(request.Id)));

            if (rep == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

            return MapUser(rep);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetById Failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Server Error: {ex.Message}"));
        }
    }

    public override async Task<GetUserByIdsResponse> GetByIds(GetUserByIdsRequest request, ServerCallContext context)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return new GetUserByIdsResponse();
        }

        var userIds = request.Ids
            .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        if (userIds.Count == 0)
        {
            return new GetUserByIdsResponse();
        }

        var users = await _mediator.Send(new GetUserByIdsQuery(userIds), context.CancellationToken);
        var response = new GetUserByIdsResponse();
        response.Users.AddRange(users.Select(MapUser));

        return response;
    }

    public override async Task<GetAllUserOrWithRoleResponse> GetAll(GetAllUserWithFilterRequest request, ServerCallContext context)
    {
        var userEntry = context.RequestHeaders.FirstOrDefault(h =>
           h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
        if (userEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        var query = new GetAllUsersQuery()
        {
            RoleSlug = request.RoleSlug,
            StoreId = Guid.TryParse(request.HomeStoreId, out var storeId)? storeId: null,
            HasHourlyWage = request.HasHourlyWage,
            Keyword = request.Keyword,
            OrderCount = request.HasOrderCount ? request.OrderCount : null,
            TierId = Guid.TryParse(request.TierId, out var parsed)? parsed: null,
<<<<<<< HEAD
            StartAt = (decimal)request.StartAt,
            EndAt = (decimal)request.EndAt,
=======
            StartAt = request.HasStartAt ? (decimal)request.StartAt : null,
            EndAt = request.HasEndAt? (decimal)request.EndAt : null,
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
            Page = request.Page,
            PageSize = request.PageSize
        };
        var pagedResult = await _mediator.Send(query);

        var res = new GetAllUserOrWithRoleResponse
        {
            TotalCount = (int)pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };

        res.Users.AddRange(pagedResult.Items.Select(u => MapUser(u)));
        return res;
    }

    public override async Task<UserResponse> GetManager(GetManagerRequest request, ServerCallContext context)
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

        DateTime? dob = null;
        if (!string.IsNullOrEmpty(request.DateOfBirth))
        {
            dob = DateTime.ParseExact(
                request.DateOfBirth,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind
            );
        }

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
                RoleSlug = request.RoleSlug,
                CurrentHourlyWage = request.HasCurrentHourlyWage ? (decimal?)request.CurrentHourlyWage : null,
                HomeStoreId = Guid.Parse(string.IsNullOrWhiteSpace(request.HomeStoreId) ? null : request.HomeStoreId)
            }
        ));

        if (rep == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapUser(rep);
    }

    public override async Task<UserResponse> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
    {
        var userEntry = context.RequestHeaders.FirstOrDefault(h =>
           h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
        if (userEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        var rep = await _mediator.Send(new ChangePasswordComand(
            Id: Guid.Parse(userEntry.Value),
            Request: new()
            {
                OldPassword = request.OldPassword,
                NewPassword = request.NewPassword,
                ConfirmNewPassword = request.ConfirmNewPassword,
                AccessToken = request.AccessToken,
                ForgetAll = request.ForgetAll
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

    public override async Task<UserResponse> UploadAvatar(UploadAvatarRequest request, ServerCallContext context)
    {
        var userEntry = context.RequestHeaders.FirstOrDefault(h =>
           h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
        if (userEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        var rep = await _mediator.Send(new UploadAvatarCommand(
            UserId: Guid.Parse(userEntry.Value),
            ImageData: request.ImageData.ToByteArray()
        ));

        return MapUser(rep);
    }

    public override async Task<DeleteAvatarResponse> DeleteAvatar(Empty request, ServerCallContext context)
    {
        var userEntry = context.RequestHeaders.FirstOrDefault(h =>
           h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
        if (userEntry == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
        }

        var message = await _mediator.Send(new DeleteAvatarCommand(Guid.Parse(userEntry.Value)));

        return new DeleteAvatarResponse { Message = message };
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
            CurrentHourlyWage = (double)(user.CurrentHourlyWage ?? 0),
            HomeStoreId = user.HomeStoreId.HasValue ? user.HomeStoreId.Value.ToString() : "",
            Role = user.Role == null
                ? new RoleUserResponse()
                : new RoleUserResponse
                {
                    Id = user.Role.Id.ToString(),
                    Name = user.Role.Name ?? "",
                    Slug = user.Role.Slug ?? ""
                },
            AvatarUrl = user.AvatarUrl ?? "",
            OrderCount = user.OrderCount ?? 0,
            Loyalty = user.LoyaltyTier == null
            ? null
            : new LoyaltyResponse
            {
                Id = user.LoyaltyTier.Id.ToString(),
                AvailablePoints = user.LoyaltyTier.AvailablePoints.ToString(),
                TotalSpent = user.LoyaltyTier.TotalSpent.ToString(),
                CurrentTier = user.LoyaltyTier.CurrentTier == null
                    ? null
                    : new MembershipTier
                    {
                        Id = user.LoyaltyTier.CurrentTier.Id.ToString(),
                        Name = user.LoyaltyTier.CurrentTier.Name ?? "",
                        MinSpendRequired = user.LoyaltyTier.CurrentTier.MinSpendRequired.ToString(),
                        DiscountRate = user.LoyaltyTier.CurrentTier.DiscountRate,
                        AmountPerPoint = user.LoyaltyTier.CurrentTier.AmountPerPoint.ToString(),
                        ValuePerPoint = user.LoyaltyTier.CurrentTier.ValuePerPoint.ToString()
                    }
            }
        };
    }
    public override async Task<GetSimpleUsersRespone> GetUserIdByPhone(GetUserIdByPhoneRequest request, ServerCallContext context)
    {
        var req = new GetUsersByPhoneQuery(PhoneNumber: request.Phone);
        var users = await _mediator.Send(req);
        var userRep = new GetSimpleUsersRespone();
        userRep.UserRes.AddRange(users.Select(u => new UserIdNameResponse
        {
            Id = u.Id.ToString(),
            Name = string.IsNullOrWhiteSpace(u.LastName) ? u.FirstName : u.LastName + " " + u.FirstName
        }));
        return userRep;
    }

    public override async Task<GetSimpleUsersRespone> GetUserIdByEmail(GetUserIdByEmailRequest request, ServerCallContext context)
    {
        var req = new GetUsersByEmailQuery(Email: request.Email);
        var users = await _mediator.Send(req);
        var userRep = new GetSimpleUsersRespone();
        userRep.UserRes.AddRange(users.Select(u => new UserIdNameResponse
        {
            Id = u.Id.ToString(),
            Name = string.IsNullOrWhiteSpace(u.LastName) ? u.FirstName : u.LastName + " " + u.FirstName
        }));
        return userRep;
    }
}