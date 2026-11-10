using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkDelete
{
    // Xoa mem nhieu ca cung luc
    // Chi cho phep xoa ca dang o trang thai Draft
    public class BulkDeleteCommand : IRequest<int>
    {
        public List<Guid> ShiftIds { get; set; } = new();
    }
}
