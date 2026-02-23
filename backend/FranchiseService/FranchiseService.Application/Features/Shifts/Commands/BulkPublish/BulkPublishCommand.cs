using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkPublish
{
    // Publish nhieu ca cung luc (Draft -> Published)
    // Chi chuyen trang thai nhung ca dang o Draft
    public class BulkPublishCommand : IRequest<int>
    {
        public List<Guid> ShiftIds { get; set; } = new();
    }
}
