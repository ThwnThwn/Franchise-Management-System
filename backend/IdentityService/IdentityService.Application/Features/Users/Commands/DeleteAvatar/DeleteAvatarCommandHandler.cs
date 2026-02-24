using Grpc.Core;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using System.Collections.Generic;

namespace IdentityService.Application.Features.Users.Commands.DeleteAvatar
{
    public class DeleteAvatarCommandHandler : IRequestHandler<DeleteAvatarCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public DeleteAvatarCommandHandler(
            IUserRepository userRepository,
            ICloudinaryService cloudinaryService)
        {
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<string> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            if (string.IsNullOrEmpty(user.AvatarUrl) || string.IsNullOrEmpty(user.AvatarPublicId))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User does not have an avatar to delete"));
            }
            else
            {
                await _cloudinaryService.DeleteImageAsync(user.AvatarPublicId);
            }

            user.AvatarUrl = null;
            user.AvatarPublicId = null;

            await _userRepository.UpdateAsync(user);

            return "Delete avatar successfully";
        }
    }
}
