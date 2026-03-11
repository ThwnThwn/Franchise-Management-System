using AutoMapper;
using BuildingBlocks.Core.Pagination;
using IdentityService.Application.Dtos.Loyalty.Response;
using IdentityService.Application.Dtos.Order.Response;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserRep>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEngagementServiceClient _engagementServiceClient;
    private readonly ISaleServiceClient _saleServiceClient;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper, IEngagementServiceClient engagementServiceClient, ISaleServiceClient saleServiceClient)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
        _engagementServiceClient = engagementServiceClient;
        _saleServiceClient = saleServiceClient;
    }

    /// <summary>
    /// Handles the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<PagedResult<UserRep>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        List<Guid>? filteredUserIds = null;
        IEnumerable<LoyaltyRes> loyaltyResponse = null;

        var role = await _roleRepository.GetBySlugAsync(query.RoleSlug);
        var roleResponse = _mapper.Map<RoleUserRep>(role);
        // Lấy all User theo Field tier + range money 
        if (query.TierId != null || query.StartAt != null || query.EndAt != null)
        {
            loyaltyResponse = await _engagementServiceClient.GetByFilter(query.TierId, query.StartAt, query.EndAt);
            filteredUserIds = loyaltyResponse.Select(x => x.Id).ToList();
        }
        //Console.WriteLine("IDs sent to Users table:");
        //foreach (var id in loyaltyResponse.Select(x => x.Id))
        //{
        //    Console.WriteLine(id);
        //}
        // Lấy all OrderCountUser 
        var orderCounts = await _saleServiceClient.GetCountOrderByUserIds(filteredUserIds ?? new List<Guid>());
        if (query.OrderCount.HasValue)
        {
            //Filter theo OrderCount
            var matchedUsers = orderCounts
                .Where(x => x.OrderCount == query.OrderCount.Value)
                .Select(x => x.UserId)
                .ToList();

            filteredUserIds = matchedUsers;
        }
        // Lưu map key Id - value loyaltyResponse

        var loyaltyDict = loyaltyResponse?.ToDictionary(x => x.Id)?? new Dictionary<Guid, LoyaltyRes>();

        // Lưu Map key Id - value CountOrder
        var UserCountOrdersDict = orderCounts.ToDictionary(x => x.UserId, x => x.OrderCount);

        // Paging theo Id đã tìm được 
        var pagedUsers = await _userRepository.GetPagedUsersAsync(
            query.Keyword ?? "",
            filteredUserIds,
            query.StoreId,
            query.HasHourlyWage,
            query.RoleSlug,
            query.Page,
            query.PageSize,
            cancellationToken
        );
        var dtos = pagedUsers.Items.Select(u =>
        {
            loyaltyDict.TryGetValue(u.Id, out var loyalty);
            UserCountOrdersDict.TryGetValue(u.Id, out var userCountOrders);
            return new UserRep
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                Address = u.Address,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                Gender = u.Gender,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DateOfBirth = u.DateOfBirth,
                IsBlocked = u.IsBlocked,
                HomeStoreId = u.HomeStoreId,
                CurrentHourlyWage = u.CurrentHourlyWage,
                Role = u.Role == null
                        ? null
                        : _mapper.Map<RoleUserRep>(u.Role),
                AvatarUrl = u.AvatarUrl,
                LoyaltyTier = loyalty == null ? null : new LoyaltyRes
                {
                    AvailablePoints = loyalty.AvailablePoints,
                    TotalSpent = loyalty.TotalSpent,
                    CurrentTier = loyalty.CurrentTier,
                },
                OrderCount = userCountOrders
            };
        });

        return new PagedResult<UserRep>(
            dtos.ToList(),
            pagedUsers.TotalCount,
            pagedUsers.Page,
            pagedUsers.PageSize
        );
    }
}