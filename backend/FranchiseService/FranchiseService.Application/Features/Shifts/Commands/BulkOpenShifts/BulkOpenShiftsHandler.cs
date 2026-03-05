using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkOpenShifts;

/// <summary>
/// Handler chuyen nhieu ca tu Draft sang Open (mo Cho Ca).
/// Chi cho phep mo cac ca: Draft, chua du nguoi, ngay chua qua.
/// Dung batch fetch de tranh N+1 query.
/// </summary>
public class BulkOpenShiftsHandler : IRequestHandler<BulkOpenShiftsCommand, BulkOpenShiftsResult>
{
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;

    public BulkOpenShiftsHandler(
        IGenericRepository<Shift> shiftRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository)
    {
        _shiftRepository = shiftRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<BulkOpenShiftsResult> Handle(
        BulkOpenShiftsCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkOpenShiftsResult();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Batch fetch: lay tat ca shifts cua Store trong 1 query duy nhat
        var allShifts = await _shiftRepository.GetAllAsync(
            predicate: s => request.ShiftIds.Contains(s.Id)
                         && s.StoreId == request.StoreId);
        var shiftMap = allShifts.ToDictionary(s => s.Id);

        // Batch fetch: lay tat ca assignments cua cac shift nay trong 1 query
        var allAssignments = await _assignmentRepository.GetAllAsync(
            predicate: a => request.ShiftIds.Contains(a.ShiftId));
        var assignmentCounts = allAssignments
            .GroupBy(a => a.ShiftId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var shiftId in request.ShiftIds)
        {
            // Bo qua neu: khong ton tai hoac khong thuoc Store nay
            if (!shiftMap.TryGetValue(shiftId, out var shift))
            {
                result.SkippedIds.Add(shiftId);
                continue;
            }

            // Chi cho phep mo ca dang o trang thai Draft
            if (shift.Status != (int)ShiftStatus.Draft)
            {
                result.SkippedIds.Add(shiftId);
                continue;
            }

            // Khong cho mo ca da qua ngay (tranh tao ca "ma")
            if (shift.ShiftDate < today)
            {
                result.SkippedIds.Add(shiftId);
                continue;
            }

            // Bo qua neu ca da du nguoi (khong can mo Cho)
            assignmentCounts.TryGetValue(shiftId, out int currentStaff);
            if (currentStaff >= shift.RequiredStaffCount)
            {
                result.SkippedIds.Add(shiftId);
                continue;
            }

            // Chuyen trang thai sang Open (mo cho nhan vien dang ky)
            shift.Status = (int)ShiftStatus.Open;
            shift.UpdatedAt = DateTimeOffset.UtcNow;
            await _shiftRepository.UpdateAsync(shift);
            result.OpenedCount++;
        }

        return result;
    }
}
