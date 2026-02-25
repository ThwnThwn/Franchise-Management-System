namespace FranchiseService.Application.Features.ShiftTemplates.DTOs
{
    /// <summary>
    /// DTO trả về thông tin mẫu ca làm việc.
    /// </summary>
    public class ShiftTemplateResponse
    {
        public Guid Id { get; set; }
        public Guid? StoreId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int BreakDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
