namespace FranchiseService.Application.Services;

/// <summary>
/// Tự động từ chối tất cả đơn Pending của ca khi ca đã đủ người.
/// Dùng chung cho: ReviewApplication (sau khi duyệt 1 đơn) và UpdateShiftAssignment (sau khi gán tay).
/// </summary>
public interface IShiftApplicationAutoRejectService
{
    /// <summary>
    /// Từ chối tất cả đơn đăng ký Pending của ca, trừ đơn excludeApplicationId (nếu có).
    /// </summary>
    /// <param name="shiftId">ID ca làm việc</param>
    /// <param name="excludeApplicationId">Đơn không reject (vd. đơn vừa được Approve); null = reject hết</param>
    /// <param name="reviewedBy">ID người thực hiện (Manager hoặc hệ thống)</param>
    Task RejectPendingApplicationsAsync(
        Guid shiftId,
        Guid? excludeApplicationId,
        Guid reviewedBy,
        CancellationToken cancellationToken = default);
}
