using System.Text.Json;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.RemoveStaff
{
    /// <summary>
    /// Handler xóa nhân viên khỏi ca làm việc (Soft Delete Assignment).
    /// </summary>
    public class RemoveStaffHandler : IRequestHandler<RemoveStaffCommand, Unit>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;

        public RemoveStaffHandler(
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

        public async Task<Unit> Handle(RemoveStaffCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm ca làm việc
            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Không tìm thấy Ca làm việc {request.ShiftId}");

            // 2. Kiểm tra trạng thái — không cho xóa assignment từ ca Completed/Cancelled
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Không thể xóa nhân viên khỏi ca đã hoàn thành.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Không thể xóa nhân viên khỏi ca đã hủy.");

            // 3. Tìm assignment của user trong ca này
            var assignments = await _assignmentRepository.GetAllAsync(
                predicate: a => a.ShiftId == request.ShiftId && a.UserId == request.UserId);

            var assignment = assignments.FirstOrDefault()
                ?? throw new KeyNotFoundException(
                    $"Nhân viên {request.UserId} không được gán vào ca {request.ShiftId}.");

            // 4. Soft Delete assignment
            await _assignmentRepository.DeleteAsync(assignment.Id);

            // 5. Tạo Outbox Message (StaffRemoved)
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "StaffRemovedFromShift",
                Content = System.Text.Json.JsonSerializer.Serialize(new
                {
                    ShiftId = shift.Id,
                    UserId = request.UserId,
                    StoreId = shift.StoreId,
                    ShiftDate = shift.ShiftDate
                }),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _outboxRepository.Add(outboxMessage);

            // 6. Đánh thức OutboxWorker
            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}
