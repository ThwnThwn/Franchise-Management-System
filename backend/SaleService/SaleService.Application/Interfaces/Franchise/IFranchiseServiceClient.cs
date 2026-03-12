namespace SalesService.Application.Interfaces.Franchise;

public interface IFranchiseServiceClient
{
    Task<UserShiftInfo?> GetUserCurrentShiftAsync(Guid userId);
    Task<List<Guid>> GetActiveStaffsAtStoreAsync(Guid storeId);
    Task<StoreDetailInfo?> GetStoreDetailAsync(Guid storeId);
}

public class UserShiftInfo
{
    public bool HasShiftToday { get; set; }
    public string? ShiftId { get; set; }
    public string? StoreId { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public bool CanCheckIn { get; set; }
    public bool CanCheckOut { get; set; }
    public string? CheckInAt { get; set; }
    public string? CheckOutAt { get; set; }
}
public class StoreDetailInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsOpenNow { get; set; }
}
