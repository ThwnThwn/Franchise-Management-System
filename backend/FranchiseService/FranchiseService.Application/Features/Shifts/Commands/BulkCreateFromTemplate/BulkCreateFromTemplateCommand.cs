using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkCreateFromTemplate
{
    // Tao nhieu ca tu Template co san (Ca Sang/Chieu/Toi)
    // Lay StartTime, EndTime, BreakDuration tu Template
    // Quet tat ca cac ngay tu startDate den endDate
    public class BulkCreateFromTemplateCommand : IRequest<int>
    {
        public Guid StoreId { get; set; }
        public Guid TemplateId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int RequiredStaffCount { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
