using AutoMapper;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Repositories;
using MediatR;
using System.Collections.Generic;

namespace IdentityService.Application.Features.Users.Commands.UploadAvatar
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UserRep>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;

        public UploadAvatarCommandHandler(
            IUserRepository userRepository,
            ICloudinaryService cloudinaryService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<UserRep> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(user.AvatarPublicId);
            }

            // Upload new avatar
            var uploadResult = await _cloudinaryService.UploadImageFromBytesAsync(
                request.ImageData
            );

            // Update user
            user.AvatarUrl = uploadResult.Url;
            user.AvatarPublicId = uploadResult.PublicId;

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserRep>(user);
        }
    }
}
