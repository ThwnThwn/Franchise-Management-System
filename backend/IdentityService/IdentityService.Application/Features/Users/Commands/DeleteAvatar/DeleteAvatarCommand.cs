using MediatR;

namespace IdentityService.Application.Features.Users.Commands.DeleteAvatar
{
    public record DeleteAvatarCommand(Guid UserId) : IRequest<string>;
}
