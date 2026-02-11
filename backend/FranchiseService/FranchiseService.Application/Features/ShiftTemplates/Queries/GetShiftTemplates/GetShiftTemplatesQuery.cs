using FranchiseService.Application.Features.ShiftTemplates.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplates
{
    public class GetShiftTemplatesQuery : IRequest<List<ShiftTemplateResponse>>
    {
        public Guid? StoreId { get; set; }
        public bool? IsActive { get; set; }
        public string? Keyword { get; set; }

        public GetShiftTemplatesQuery(Guid? storeId, bool? isActive, string? keyword)
        {
            StoreId = storeId;
            IsActive = isActive;
            Keyword = keyword;
        }
    }
}
