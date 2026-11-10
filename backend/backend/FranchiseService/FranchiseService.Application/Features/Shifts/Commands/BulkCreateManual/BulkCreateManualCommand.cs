using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkCreateManual
{
    // Tao nhieu ca thu cong theo khung gio va cac ngay trong tuan
    // Quet tu startDate den endDate, ngay nao trung daysOfWeek thi tao 1 ca Draft
    public class BulkCreateManualCommand : IRequest<int>
    {
        public Guid StoreId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public List<int> DaysOfWeek { get; set; } = new();
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int RequiredStaffCount { get; set; }
        public int BreakDurationMinutes { get; set; }
        public string? Note { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
