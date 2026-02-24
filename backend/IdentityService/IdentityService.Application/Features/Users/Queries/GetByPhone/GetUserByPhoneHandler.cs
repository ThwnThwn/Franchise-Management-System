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
    public class GetUserByPhoneHandler : IRequestHandler<GetUserByPhoneQuery, User>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByPhoneHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }   

        public async Task<User> Handle(GetUserByPhoneQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByPhoneAsync(request.PhoneNumber);
            
            if(user == null)
                throw new DirectoryNotFoundException("Không tìm thấy người dùng");

            return user;
        }
    }
}
