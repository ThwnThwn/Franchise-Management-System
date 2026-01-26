using AutoMapper;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordComand, UserRep?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public ChangePasswordCommandHandler(IUserRepository userRepository, IMapper mapper, ICurrentUserService currentUser, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _currentUser = currentUser;
            _roleRepository = roleRepository;
        }

        public async Task<UserRep?> Handle(ChangePasswordComand command, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
                throw new UnauthorizedAccessException("Unauthenticated");
            var currentUser = await _userRepository.GetByIdAsync(currentUserId.Value);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Unauthenticated");
            var roleAdmin = await _roleRepository.GetByNameAsync("Admin");
            var request = command.Request;

            //Only allow update own profile
            if (currentUserId.Value != command.Id && currentUser.Role != roleAdmin)
                throw new ForbidenException("You can only update your own profile");

            //Check old password
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, currentUser.Password))
                throw new UnauthorizedAccessException("Invalid wrong current password");

            if(request.NewPassword != request.ConfirmNewPassword)
                throw new UnauthorizedAccessException("New password and Confirm password are not the same");

            //Set new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.ConfirmNewPassword);
            currentUser.Password = hashedPassword;

            _mapper.Map(request, currentUser);
            currentUser.UpdatedAt = DateTimeOffset.UtcNow;

            await _userRepository.UpdateAsync(currentUser);

            return _mapper.Map<UserRep>(currentUser);
        }
    }
}
