using AutoMapper;
using IdentityService.Application.Dtos.Loyalty.Response;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserRep?>
{
    private readonly IUserRepository _userRepository;
    private readonly IEngagementServiceClient _engagementServiceClient;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper, IEngagementServiceClient engagementServiceClient)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _engagementServiceClient = engagementServiceClient;
    }

    public async Task<UserRep?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        var loyalty = await _engagementServiceClient.GetLoyaltyByEmail(user.Email);


        return new UserRep
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Address = user.Address,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Gender = user.Gender,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            DateOfBirth = user.DateOfBirth,
            IsBlocked = user.IsBlocked,
            HomeStoreId = user.HomeStoreId,
            CurrentHourlyWage = user.CurrentHourlyWage,
            Role = user.Role == null
                        ? null
                        : _mapper.Map<RoleUserRep>(user.Role),
            AvatarUrl = user.AvatarUrl,
            LoyaltyTier = loyalty == null ? null : new LoyaltyRes
            {
                AvailablePoints = loyalty.AvailablePoints,
                TotalSpent = loyalty.TotalSpent,
                CurrentTier = loyalty.CurrentTier,
            }
        };
    }
}
