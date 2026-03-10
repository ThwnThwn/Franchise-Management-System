using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkPublish
{
    // Loc chi nhung ca dang Draft, chuyen sang Published
    // Nhung ca o trang thai khac (Published, Completed, Cancelled) se bi bo qua
    public class BulkPublishHandler : IRequestHandler<BulkPublishCommand, int>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;

        public BulkPublishHandler(IGenericRepository<Shift> shiftRepository)
        {
            _shiftRepository = shiftRepository;
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