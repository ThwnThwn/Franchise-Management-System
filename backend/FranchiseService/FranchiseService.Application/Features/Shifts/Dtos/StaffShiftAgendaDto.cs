namespace FranchiseService.Application.Features.Shifts.DTOs;

/// <summary>
/// DTO cho 1 muc trong lich ca nhan cua nhan vien
/// </summary>
public class StaffShiftAgendaDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AgendaStoreDto Store { get; set; } = new();
    public List<AgendaCoWorkerDto> CoWorkers { get; set; } = new();
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
}

/// <summary>
/// Thong tin cua hang (khop voi FE StoreDetailDto)
/// </summary>
public class AgendaStoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsOpenNow { get; set; }
}

/// <summary>
/// Thong tin dong nghiep cung ca
/// </summary>
public class AgendaCoWorkerDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
}
