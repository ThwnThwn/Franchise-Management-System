using FranchiseService.Application.Features.ShiftTemplates.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplateById
{
    public class GetShiftTemplateByIdQuery : IRequest<ShiftTemplateResponse>
    {
        public Guid Id { get; set; }

        public GetShiftTemplateByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
