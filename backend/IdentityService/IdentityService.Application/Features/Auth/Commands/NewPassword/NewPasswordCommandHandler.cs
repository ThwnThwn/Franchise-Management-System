using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.NewPassword
{
    public class NewPasswordCommandHandler
        : IRequestHandler<NewPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public NewPasswordCommandHandler(IUserRepository userRepository,IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Unit> Handle(NewPasswordCommand command, CancellationToken cancellationToken)
        {
            var email = command.Email;
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new UnauthorizedAccessException($"User with email '{email}' not found");

            //Revoke all user token
            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Request.NewPassword);
            user.Password = hashedPassword;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(user);

            return Unit.Value;
        }
    }
}
