using AutoMapper;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Queries.GetUserById;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetManager
{
    public class GetManagerQueryHandler : IRequestHandler<GetManagerQuery, UserRep?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetManagerQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserRep?> Handle(GetManagerQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetManagerByStoreIdAsync(request.Request.StoreId);
            return user == null ? null : _mapper.Map<UserRep>(user);
        }
    }
}
