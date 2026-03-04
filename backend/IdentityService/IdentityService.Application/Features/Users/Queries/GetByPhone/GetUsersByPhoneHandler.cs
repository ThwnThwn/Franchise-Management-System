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

        public async Task<List<User>> Handle(GetUsersByPhoneQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUsersByPhoneAsync(request.PhoneNumber);

            if(users == null || !users.Any())
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");

            return users;
        }
    }
}
