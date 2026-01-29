using AutoMapper;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserRep>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserRep>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        
        var users = await _userRepository.GetUsersByRoleSlugAsync(query.RoleSlug);
        return _mapper.Map<IEnumerable<UserRep>>(users);
    }
}
