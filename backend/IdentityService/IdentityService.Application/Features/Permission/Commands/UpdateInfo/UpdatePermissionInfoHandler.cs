using IdentityService.Application.Persistences.Repositories;
using MediatR;

namespace IdentityService.Application.Features.Permission.Commands.UpdateInfo
{
    public class UpdatePermissionInfoHandler : IRequestHandler<UpdatePermissionInfoCommand>
    {
        private readonly IPermissionRepository _permRepo;

        public UpdatePermissionInfoHandler(IPermissionRepository permRepo)
        {
            _permRepo = permRepo;
        }

        public async Task<Unit> Handle(UpdatePermissionInfoCommand request, CancellationToken cancellationToken)
        {
            var perm = await _permRepo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy perrmission này");

            // Update Info
            perm.Name = !string.IsNullOrEmpty(request.Name) ? request.Name : perm.Name;
            perm.Description = !string.IsNullOrEmpty(request.Description) ? request.Description : perm.Description;

            await _permRepo.UpdateAsync(perm);

            return Unit.Value;
        }
    }
}