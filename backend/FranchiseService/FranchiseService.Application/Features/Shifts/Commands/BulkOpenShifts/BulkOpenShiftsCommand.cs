using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkOpenShifts;

/// <summary>
/// Command mo nhieu ca ra Cho Ca (Draft -> Open).
/// Manager chon cac ca thieu nguoi va day ra cho de nhan vien dang ky.
/// </summary>
public class BulkOpenShiftsCommand : IRequest<BulkOpenShiftsResult>
{
    /// <summary>
    /// Danh sach ID cac ca can mo ra Cho Ca
    /// </summary>
    public List<Guid> ShiftIds { get; set; } = new();

    /// <summary>
    /// ID cua hang de validate quyen
    /// </summary>
    public Guid StoreId { get; set; }
}

/// <summary>
/// Ket qua tra ve sau khi mo ca ra Cho
/// </summary>
public class BulkOpenShiftsResult
{
    /// <summary>
    /// So ca da mo thanh cong
    /// </summary>
    public int OpenedCount { get; set; }

    /// <summary>
    /// Danh sach ID cac ca khong the mo (khong phai Draft, da du nguoi, hoac da qua ngay)
    /// </summary>
    public List<Guid> SkippedIds { get; set; } = new();
}
