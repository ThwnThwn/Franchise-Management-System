using AutoMapper;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordComand, UserRep?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenBlacklist _tokenBlacklist;
        private readonly JwtHelper _jwtHelper;
        private readonly IMapper _mapper;

        public ChangePasswordCommandHandler(IUserRepository userRepository, IMapper mapper, ICurrentUserService currentUser, IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenBlacklist tokenBlacklist,
            JwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _currentUser = currentUser;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenBlacklist = tokenBlacklist;
            _jwtHelper = jwtHelper;
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

            //Update new password
            currentUser.UpdatedAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(currentUser);

            if (request.ForgetAll == true)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(currentUser.Id);
                var expiry = _jwtHelper.GetTokenExpiry(request.AccessToken);
                if (expiry != null)
                {
                    await _tokenBlacklist.AddToBlacklistAsync(
                        request.AccessToken,
                        expiry.Value
                    );
                }
            }

            _mapper.Map(request, currentUser);
            return _mapper.Map<UserRep>(currentUser);
        }
    }
}
