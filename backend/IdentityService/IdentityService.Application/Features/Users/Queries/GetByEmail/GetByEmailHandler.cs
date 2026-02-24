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
    public class GetByEmailHandler : IRequestHandler<GetByEmailQuery, User>
    {
        private readonly IUserRepository _userRepository;   
        public GetByEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }   
        public async Task<User> Handle(GetByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if(user == null)
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");

            return user;
        }
    }
}
