using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.DeleteTemplate
{
    /// <summary>
    /// Handler xóa mẫu ca (Soft Delete qua IGenericRepository).
    /// </summary>
    public class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, Unit>
    {
        private readonly IGenericRepository<ShiftTemplate> _templateRepo;

        public DeleteTemplateHandler(IGenericRepository<ShiftTemplate> templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<Unit> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra mẫu ca tồn tại
            var template = await _templateRepo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy mẫu ca {request.Id}");

            // Soft Delete (IGenericRepository.DeleteAsync sẽ set IsDeleted = true)
            await _templateRepo.DeleteAsync(template.Id);

            return Unit.Value;
        }
    }
}
