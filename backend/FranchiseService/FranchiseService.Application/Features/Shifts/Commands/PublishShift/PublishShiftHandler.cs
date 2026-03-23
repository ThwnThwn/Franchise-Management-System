using System.Text.Json;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Events;

namespace FranchiseService.Application.Features.Shifts.Commands.PublishShift
{
    /// <summary>
    /// Handler chuyển trạng thái ca từ Draft → Published.
    /// Chỉ cho phép chốt khi ca đã đủ số nhân viên (assignments >= RequiredStaffCount).
    /// </summary>
    public class PublishShiftHandler : IRequestHandler<PublishShiftCommand, Unit>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;

        public PublishShiftHandler(
            IGenericRepository<Shift> shiftRepository,
            IGenericRepository<ShiftAssignment> assignmentRepository,
            IOutboxRepository outboxRepository,
            Application.Signals.IOutboxSignal outboxSignal)
        {
            _shiftRepository = shiftRepository;
            _assignmentRepository = assignmentRepository;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
        }

        public async Task<Unit> Handle(PublishShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm ca làm việc
            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Không tìm thấy Ca làm việc {request.ShiftId}");

            // 2. Kiểm tra trạng thái — chỉ cho phép Draft → Published
            if (shift.Status == (int)ShiftStatus.Published)
                throw new InvalidOperationException("Ca này đã được publish rồi.");
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Không thể publish ca đã hoàn thành.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Không thể publish ca đã hủy.");

            // 2.5. Chỉ cho chốt ca khi đã đủ nhân viên
            var assignments = await _assignmentRepository.GetAllAsync(
                predicate: a => a.ShiftId == shift.Id);
            var assignedCount = assignments.Count();
            if (assignedCount < shift.RequiredStaffCount)
                throw new InvalidOperationException(
                    $"Khong the chot ca: can du {shift.RequiredStaffCount} nhan vien, hien co {assignedCount}.");

            // 3. Chuyển trạng thái
            shift.Status = (int)ShiftStatus.Published;
            shift.UpdatedAt = DateTimeOffset.UtcNow;

            await _shiftRepository.UpdateAsync(shift);

            // 4. Tạo Outbox Message (ShiftUpdated — thay đổi status)
            var @event = new ShiftUpdatedEvent
            {
                ShiftId = shift.Id,
                StoreId = shift.StoreId,
                ShiftDate = shift.ShiftDate,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                RequiredStaffCount = shift.RequiredStaffCount,
                BreakDurationMinutes = shift.BreakDurationMinutes
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ShiftPublished",
                Content = JsonSerializer.Serialize(@event),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _outboxRepository.Add(outboxMessage);

            // 5. Đánh thức OutboxWorker
            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}
