using IdentityService.Application.AppExceptions;
using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.NewPassword
{
    public class NewPasswordCommandHandler
        : IRequestHandler<NewPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public NewPasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            NewPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var email = command.Email;
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new ForbidenException($"User with email '{email}' not found");
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Request.NewPassword);
            user.Password = hashedPassword;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(user);

            return Unit.Value;
        }
    }
}
