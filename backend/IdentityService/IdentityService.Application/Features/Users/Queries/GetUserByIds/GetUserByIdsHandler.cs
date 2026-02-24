using AutoMapper;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Queries.GetUserByIds;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetUserByIds;

public class GetUserByIdsQueryHandler
    : IRequestHandler<GetUserByIdsQuery, List<UserRep?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdsQueryHandler(IUserRepository userRepository,IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserRep?>> Handle(GetUserByIdsQuery request,CancellationToken cancellationToken)
    {
        if (request.UserIds == null || request.UserIds.Count == 0)
            return new List<UserRep?>();

        var users = await _userRepository.GetByIds(request.UserIds);

        return users
            .Where(u => u != null)
            .Select(u => _mapper.Map<UserRep>(u!))
            .Cast<UserRep?>()
            .ToList();
    }
}
