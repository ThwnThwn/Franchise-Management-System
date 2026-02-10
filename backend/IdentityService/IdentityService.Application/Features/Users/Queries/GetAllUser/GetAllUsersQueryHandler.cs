using AutoMapper;
using BuildingBlocks.Core.Pagination;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserRep>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserRep>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var pagedUsers = await _userRepository.GetPagedUsersAsync(
            query.Keyword ?? "",
            query.Id,
            query.RoleSlug,
            query.Page,
            query.PageSize,
            cancellationToken
        );
        var role = await _roleRepository.GetBySlugAsync(query.RoleSlug);
        var roleResponse = _mapper.Map<RoleUserRep>(role);

        var dtos = pagedUsers.Items.Select(u => new UserRep
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
            Role = u.Role == null
                ? null
                : _mapper.Map<RoleUserRep>(u.Role)
        }).ToList();

        return new PagedResult<UserRep>(dtos, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PageSize);
    }
}