using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate
{
    /// <summary>
    /// Command tạo mẫu ca làm việc mới.
    /// FE gửi: POST /shift-templates { code, name, startTime, endTime, breakDurationMinutes, storeId? }
    /// </summary>
    public record CreateTemplateCommand : IRequest<Guid>
    {
        public Guid? StoreId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int BreakDurationMinutes { get; set; }
    }
}
