using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Services;

/// <summary>
/// Tự động từ chối tất cả đơn Pending của ca khi ca đã đủ người.
/// </summary>
public class ShiftApplicationAutoRejectService : IShiftApplicationAutoRejectService
{
    private readonly IGenericRepository<ShiftApplication> _applicationRepository;

    public ShiftApplicationAutoRejectService(
        IGenericRepository<ShiftApplication> applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    /// <inheritdoc />
    public async Task RejectPendingApplicationsAsync(
        Guid shiftId,
        Guid? excludeApplicationId,
        Guid reviewedBy,
        CancellationToken cancellationToken = default)
    {
        var remainingApps = await _applicationRepository.GetAllAsync(
            predicate: a => a.ShiftId == shiftId
                         && a.Status == (int)ShiftApplicationStatus.Pending
                         && (!excludeApplicationId.HasValue || a.Id != excludeApplicationId.Value));

        foreach (var app in remainingApps)
        {
            app.Status = (int)ShiftApplicationStatus.Rejected;
            app.ReviewedAt = DateTimeOffset.UtcNow;
            app.ReviewedBy = reviewedBy;
            app.ReviewNote = "Tu dong tu choi: ca da du nguoi.";
            app.UpdatedAt = DateTimeOffset.UtcNow;
            await _applicationRepository.UpdateAsync(app);
        }
    }
}
