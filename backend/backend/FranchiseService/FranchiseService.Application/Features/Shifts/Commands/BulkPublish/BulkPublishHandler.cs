using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkPublish
{
    // Loc chi nhung ca dang Draft/Open, chuyen sang Published.
    // Chi cho phep chot khi moi ca deu da du so nhan vien (assignments >= RequiredStaffCount).
    public class BulkPublishHandler : IRequestHandler<BulkPublishCommand, int>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;

        public BulkPublishHandler(
            IGenericRepository<Shift> shiftRepository,
            IGenericRepository<ShiftAssignment> assignmentRepository)
        {
            _shiftRepository = shiftRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task<int> Handle(
            BulkPublishCommand request, CancellationToken cancellationToken)
        {
            // Validate: phai chon it nhat 1 ca
            if (!request.ShiftIds.Any())
                throw new ArgumentException("Phai chon it nhat 1 ca de publish.");

            // Lay tat ca shift theo ids (dung GetByIdsAsync cho hieu qua)
            var shifts = await _shiftRepository.GetByIdsAsync(request.ShiftIds.ToArray());

            // Chi publish nhung ca dang Draft, bo qua trang thai khac
            var canPublishShifts = shifts
                .Where(s => s.Status == (int)ShiftStatus.Draft || s.Status == (int)ShiftStatus.Open)
                .ToList();

            // Validate: phai co it nhat 1 ca Draft trong danh sach
            if (!canPublishShifts.Any())
                throw new InvalidOperationException(
                    "Khong co ca nao o trang thai Draft de publish.");

            // Batch: lay so luong assignment cua tat ca ca can publish (tranh N+1)
            var shiftIdsToCheck = canPublishShifts.Select(s => s.Id).ToList();
            var allAssignments = await _assignmentRepository.GetAllAsync(
                predicate: a => shiftIdsToCheck.Contains(a.ShiftId));
            var assignmentCounts = allAssignments
                .GroupBy(a => a.ShiftId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Chi cho chot khi moi ca deu du nguoi
            foreach (var shift in canPublishShifts)
            {
                var currentCount = assignmentCounts.GetValueOrDefault(shift.Id, 0);
                if (currentCount < shift.RequiredStaffCount)
                    throw new InvalidOperationException(
                        $"Khong the chot ca: ca chua du nhan vien (can {shift.RequiredStaffCount}, hien co {currentCount}). " +
                        $"ShiftId: {shift.Id}, ngay: {shift.ShiftDate}.");
            }

            // Chuyen trang thai tung ca tu Draft sang Published
            foreach (var shift in canPublishShifts)
            {
                shift.Status = (int)ShiftStatus.Published;
                shift.UpdatedAt = DateTimeOffset.UtcNow;
                await _shiftRepository.UpdateAsync(shift);
            }

            // Tra ve so ca thuc te duoc publish
            return canPublishShifts.Count;
        }
    }
}