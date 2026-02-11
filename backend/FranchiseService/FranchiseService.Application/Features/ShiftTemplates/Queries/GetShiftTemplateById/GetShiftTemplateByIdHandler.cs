using FranchiseService.Application.Features.ShiftTemplates.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplateById
{
    public class GetShiftTemplateByIdHandler : IRequestHandler<GetShiftTemplateByIdQuery, ShiftTemplateResponse>
    {
        private readonly IGenericRepository<ShiftTemplate> _templateRepo;

        public GetShiftTemplateByIdHandler(IGenericRepository<ShiftTemplate> templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<ShiftTemplateResponse> Handle(GetShiftTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await _templateRepo.GetByIdAsync(request.Id);
            
            if (template == null)
            {
               throw new KeyNotFoundException($"Không tìm thấy mẫu ca {request.Id}");
            }

            return new ShiftTemplateResponse
            {
                Id = template.Id,
                StoreId = template.StoreId,
                Code = template.Code,
                Name = template.Name,
                StartTime = template.StartTime,
                EndTime = template.EndTime,
                BreakDurationMinutes = template.BreakDurationMinutes ?? 0,
                IsActive = template.IsActive ?? true,
                CreatedAt = template.CreatedAt
            };
        }
    }
}
