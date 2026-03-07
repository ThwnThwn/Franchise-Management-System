using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.AutoFillShifts;

/// <summary>
/// Lenh tu dong xep ca cho cua hang trong khoang ngay.
/// Manager chi can chon ngay bat dau va ket thuc, he thong tu tim
/// nhan vien phu hop va gan vao cac ca Draft chua du nguoi.
/// </summary>
public class AutoFillShiftsCommand : IRequest<AutoFillResult>
{
    /// <summary>
    /// Ma cua hang can tu dong xep ca
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// Ngay bat dau khoang can xep
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Ngay ket thuc khoang can xep
    /// </summary>
    public DateOnly EndDate { get; set; }
}

/// <summary>
/// Ket qua tra ve sau khi chay tu dong xep ca.
/// </summary>
public class AutoFillResult
{
    /// <summary>
    /// Tong so slot da dien duoc nhan vien
    /// </summary>
    public int FilledCount { get; set; }

    /// <summary>
    /// Tong so slot can dien (bao gom chua dien va da dien)
    /// </summary>
    public int TotalSlots { get; set; }

    /// <summary>
    /// Danh sach ma ca khong dien du nguoi.
    /// Manager co the xu ly thu cong hoac day ra Cho Ca.
    /// </summary>
    public List<Guid> UnfilledShiftIds { get; set; } = new();
}
