using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services;

/// <summary>
/// Validate nhan vien cho ca lam viec. Tra ve danh sach canh bao (rong = hop le).
/// Dung chung cho: AssignStaff, GetCandidates, AutoFill, ApplyForShift.
/// </summary>
public interface IShiftValidationService
{
    /// <summary>
    /// Validate bang cach query DB. Dung cho AssignStaff, ApplyForShift.
    /// </summary>
    Task<List<ValidationWarning>> ValidateAssignment(Shift shift, Guid userId, int minHoursBreak, int maxHoursPerWeek, CancellationToken ct);

    /// <summary>
    /// Validate bang du lieu in-memory (khong query DB them).
    /// Dung cho GetAvailableStaffs va AutoFill de tranh N+1.
    /// </summary>
    CandidateValidation ValidateCandidate(Shift shift, List<ShiftAssignment> userAssignments, int minHoursBreak, int maxHoursPerWeek);
}
