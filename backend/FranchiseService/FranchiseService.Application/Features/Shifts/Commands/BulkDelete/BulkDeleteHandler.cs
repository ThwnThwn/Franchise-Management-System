using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkDelete
{
    // Chi xoa ca Draft, cascade soft-delete assignments lien quan
    // Ca Published, Completed, Cancelled se bi bo qua (khong throw loi)
    public class BulkDeleteHandler : IRequestHandler<BulkDeleteCommand, int>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;

        public BulkDeleteHandler(
            IGenericRepository<Shift> shiftRepository,
            IGenericRepository<ShiftAssignment> assignmentRepository)
        {
            _shiftRepository = shiftRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task<int> Handle(
            BulkDeleteCommand request, CancellationToken cancellationToken)
        {
            // Validate: phai chon it nhat 1 ca
            if (!request.ShiftIds.Any())
                throw new ArgumentException("Phai chon it nhat 1 ca de xoa.");

            // Lay tat ca shift theo ids (dung GetByIdsAsync cho hieu qua)
            var shifts = await _shiftRepository.GetByIdsAsync(request.ShiftIds.ToArray());

            // Chi xoa nhung ca dang Draft (FE cung chi cho chon ca Draft)
            var draftShifts = shifts
                .Where(s => s.Status == (int)ShiftStatus.Draft)
                .ToList();

            // Validate: phai co it nhat 1 ca Draft trong danh sach
            if (!draftShifts.Any())
                throw new InvalidOperationException(
                    "Khong co ca nao o trang thai Draft de xoa.");

            foreach (var shift in draftShifts)
            {
                // Cascade soft-delete tat ca assignments (phan cong nhan vien) cua ca nay truoc
                var assignments = await _assignmentRepository.GetAllAsync(
                    predicate: a => a.ShiftId == shift.Id);

                foreach (var assignment in assignments)
                {
                    await _assignmentRepository.DeleteAsync(assignment.Id);
                }

                // Chuyen trang thai sang Cancelled truoc khi soft-delete (giong DeleteShiftHandler don le)
                shift.Status = (int)ShiftStatus.Cancelled;
                shift.UpdatedAt = DateTimeOffset.UtcNow;
                await _shiftRepository.UpdateAsync(shift);

                // Soft-delete ca lam viec
                await _shiftRepository.DeleteAsync(shift.Id);
            }

            // Tra ve so ca thuc te duoc xoa
            return draftShifts.Count;
        }
    }
}
