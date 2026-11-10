using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetByEmail
{
    public class GetUsersByEmailHandler : IRequestHandler<GetUsersByEmailQuery, List<User>>
    {
        private readonly IUserRepository _userRepository;
        public GetUsersByEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        /// <exception cref="DirectoryNotFoundException">Không tìm thấy người dùng</exception>
        public async Task<List<User>> Handle(GetUsersByEmailQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUsersByEmailAsync(request.Email);

            if (users == null || !users.Any())
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");
            return users;
        }
    }
}
