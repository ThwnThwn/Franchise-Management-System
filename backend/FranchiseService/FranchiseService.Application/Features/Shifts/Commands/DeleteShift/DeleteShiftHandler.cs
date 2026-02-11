using System.Text.Json;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Events;

namespace FranchiseService.Application.Features.Shifts.Commands.DeleteShift
{
    /// <summary>
    /// Handler xử lý xóa ca làm việc (Soft Delete).
    /// Bao gồm: kiểm tra trạng thái, cascade soft-delete assignments, set status = Cancelled.
    /// </summary>
    public class DeleteShiftHandler : IRequestHandler<DeleteShiftCommand, Unit>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;

        public DeleteShiftHandler(
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

        public async Task<Unit> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm ca làm việc
            var shift = await _shiftRepository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy Ca làm việc {request.Id}");

            // 2. Kiểm tra trạng thái (Không cho xóa ca đã Completed)
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Không thể xóa ca đã hoàn thành.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Ca này đã được hủy trước đó.");

            // 3. Cascade soft-delete các ShiftAssignment liên quan 
            var assignments = await _assignmentRepository.GetAllAsync(
                predicate: a => a.ShiftId == shift.Id);

            foreach (var assignment in assignments)
            {
                await _assignmentRepository.DeleteAsync(assignment.Id);
            }

            // 4. Set trạng thái Cancelled trước khi soft delete 
            shift.Status = (int)ShiftStatus.Cancelled;
            shift.UpdatedAt = DateTimeOffset.UtcNow;
            await _shiftRepository.UpdateAsync(shift);

            // 5. Soft Delete ca
            await _shiftRepository.DeleteAsync(shift.Id);

            // 6. Tạo Outbox Message
            var @event = new ShiftDeletedEvent
            {
                ShiftId = shift.Id,
                StoreId = shift.StoreId,
                ShiftDate = shift.ShiftDate
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ShiftDeleted",
                Content = JsonSerializer.Serialize(@event),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _outboxRepository.Add(outboxMessage);

            // 7. Đánh thức OutboxWorker
            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}
