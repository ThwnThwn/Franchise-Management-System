using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.UpdateTemplate
{
    /// <summary>
    /// Command cập nhật mẫu ca (Partial Update / PATCH).
    /// FE gửi: PATCH /shift-templates/{id} { code?, name?, startTime?, endTime?, breakDurationMinutes?, isActive? }
    /// </summary>
    public record UpdateTemplateCommand : IRequest<Unit>
    {
        /// <summary>
        /// ID mẫu ca (set từ route param bởi Controller)
        /// </summary>
        public Guid Id { get; set; }

        public string? Code { get; set; }
        public string? Name { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int? BreakDurationMinutes { get; set; }
        public bool? IsActive { get; set; }
    }
}
