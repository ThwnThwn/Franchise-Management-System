using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Users.Queries.GetByPhone
{
    public class GetUsersByPhoneHandler : IRequestHandler<GetUsersByPhoneQuery, List<User>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersByPhoneHandler(IUserRepository userRepository)
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
        public async Task<List<User>> Handle(GetUsersByPhoneQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUsersByPhoneAsync(request.PhoneNumber);

            if(users == null || !users.Any())
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");

            return users;
        }
    }
}
