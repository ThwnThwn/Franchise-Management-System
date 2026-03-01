using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Users.Queries.GetByEmail
{
    public class GetUsersByEmailHandler : IRequestHandler<GetUsersByEmailQuery, List<User>>
    {
        private readonly IUserRepository _userRepository;   
        public GetUsersByEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }   
        public async Task<List<User>> Handle(GetUsersByEmailQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUsersByEmailAsync(request.Email);

            if(users == null || !users.Any())
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");

            return users;
        }
    }
}
