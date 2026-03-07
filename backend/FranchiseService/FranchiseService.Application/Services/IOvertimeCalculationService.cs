using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services;

/// <summary>
/// Interface dich vu tinh toan Overtime.
/// Tach rieng de tai su dung o ca CheckOutHandler va UpdateTimeLogHandler.
/// </summary>
public interface IOvertimeCalculationService
{
    /// <summary>
    /// Tinh toan phan bo so phut lam viec thanh Regular, Daily OT, Weekly OT.
    /// </summary>
    Task<OvertimeResult> CalculateAsync(TimeLog timeLog, Shift shift, ShiftRule? rule, Guid userId);
}
