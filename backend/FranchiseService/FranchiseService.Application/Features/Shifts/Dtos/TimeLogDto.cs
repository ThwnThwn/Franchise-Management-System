namespace FranchiseService.Application.Features.Shifts.DTOs
{
    public class TimeLogDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset? CheckInAt { get; set; }
        public DateTimeOffset? CheckOutAt { get; set; }
        public int Status { get; set; }
    }
}
