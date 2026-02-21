using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.UploadAvatar
{
    public record UploadAvatarCommand(Guid UserId, byte[] ImageData) : IRequest<UserRep>;
}
