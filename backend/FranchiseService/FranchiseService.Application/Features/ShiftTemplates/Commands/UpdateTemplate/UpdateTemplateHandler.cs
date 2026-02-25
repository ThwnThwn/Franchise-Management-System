using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.UpdateTemplate
{
    /// <summary>
    /// Handler cập nhật mẫu ca (Partial Update).
    /// Chỉ update các field có giá trị (not null).
    /// </summary>
    public class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, Unit>
    {
        private readonly IGenericRepository<ShiftTemplate> _templateRepo;

        public UpdateTemplateHandler(IGenericRepository<ShiftTemplate> templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<Unit> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
        {
            // 1. Load entity
            var template = await _templateRepo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy mẫu ca {request.Id}");

            // 2. Partial Update — chỉ cập nhật field có giá trị
            if (request.Code != null)
                template.Code = request.Code;

            if (request.Name != null)
                template.Name = request.Name;

            if (request.StartTime.HasValue)
                template.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                template.EndTime = request.EndTime.Value;

            if (request.BreakDurationMinutes.HasValue)
                template.BreakDurationMinutes = request.BreakDurationMinutes.Value;

            if (request.IsActive.HasValue)
                template.IsActive = request.IsActive.Value;

            // 3. Cập nhật timestamp
            template.UpdatedAt = DateTimeOffset.UtcNow;

            await _templateRepo.UpdateAsync(template);

            return Unit.Value;
        }
    }
}
