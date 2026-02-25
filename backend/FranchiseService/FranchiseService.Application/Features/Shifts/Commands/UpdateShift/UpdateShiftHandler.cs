using System.Text.Json;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;
using SharedKernel.Events;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShift
{
    /// <summary>
    /// Handler xử lý cập nhật ca làm việc.
    /// Hỗ trợ Partial Update, thay đổi Template, Optimistic Concurrency, và kiểm tra trùng lặp.
    /// </summary>
    public class UpdateShiftHandler : IRequestHandler<UpdateShiftCommand, Unit>
    {
        private readonly IGenericRepository<Shift> _shiftRepository;
        private readonly IShiftRepository _shiftQueryRepository;
        private readonly IGenericRepository<ShiftTemplate> _shiftTemplateRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;

        public UpdateShiftHandler(
            IGenericRepository<Shift> shiftRepository,
            IShiftRepository shiftQueryRepository,
            IGenericRepository<ShiftTemplate> shiftTemplateRepository,
            IOutboxRepository outboxRepository,
            Application.Signals.IOutboxSignal outboxSignal)
        {
            _shiftRepository = shiftRepository;
            _shiftQueryRepository = shiftQueryRepository;
            _shiftTemplateRepository = shiftTemplateRepository;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
        }

        public async Task<Unit> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm ca làm việc
            var shift = await _shiftRepository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Không tìm thấy Ca làm việc {request.Id}");

            // 2. Kiểm tra trạng thái (Không cho sửa ca đã Completed/Cancelled)
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Không thể chỉnh sửa ca đã hoàn thành.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Không thể chỉnh sửa ca đã hủy.");

            // 3. Optimistic Concurrency Check
            //if (shift.LockVersion != request.LockVersion)
            //{
            //    throw new InvalidOperationException("Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
            //}

            // 4. Nếu đổi ShiftTemplateId -> Fetch template mới và áp dụng giờ giấc
            if (request.ShiftTemplateId.HasValue && request.ShiftTemplateId.Value != shift.ShiftTemplateId)
            {
                var template = await _shiftTemplateRepository.GetByIdAsync(request.ShiftTemplateId.Value)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Mẫu ca làm việc với ID: {request.ShiftTemplateId}");

                shift.ShiftTemplateId = template.Id;
                // Áp dụng giờ từ Template, trừ khi Request gửi kèm giờ thủ công (Override)
                shift.StartTime = request.StartTime ?? template.StartTime;
                shift.EndTime = request.EndTime ?? template.EndTime;
                shift.BreakDurationMinutes = request.BreakDurationMinutes ?? template.BreakDurationMinutes ?? shift.BreakDurationMinutes;
            }
            else
            {
                // Không đổi Template -> Partial Update bình thường
                if (request.StartTime.HasValue) shift.StartTime = request.StartTime.Value;
                if (request.EndTime.HasValue) shift.EndTime = request.EndTime.Value;
                if (request.BreakDurationMinutes.HasValue) shift.BreakDurationMinutes = request.BreakDurationMinutes.Value;
            }

            // 5. Update các trường còn lại (Partial Update)
            if (request.StoreId.HasValue) shift.StoreId = request.StoreId.Value;
            if (request.ShiftDate.HasValue) shift.ShiftDate = request.ShiftDate.Value;
            if (request.RequiredStaffCount.HasValue) shift.RequiredStaffCount = request.RequiredStaffCount.Value;
            if (request.Note != null) shift.Note = request.Note;
            if (request.Status.HasValue) shift.Status = request.Status.Value;

            // 6. Kiểm tra trùng lặp sau khi áp dụng thay đổi
            // Chỉ check nếu giờ hoặc ngày có thay đổi
            bool timeOrDateChanged = request.StartTime.HasValue || request.EndTime.HasValue
                                  || request.ShiftDate.HasValue || request.ShiftTemplateId.HasValue;

            if (timeOrDateChanged)
            {
                var existingShifts = await _shiftQueryRepository.GetShiftsByDateRangeAsync(
                    shift.StoreId, shift.ShiftDate, shift.ShiftDate, cancellationToken);

                // Exclude chính shift đang sửa (vì nó sẽ match với chính nó)
                var duplicate = existingShifts.FirstOrDefault(s =>
                    s.Id != shift.Id
                    && s.StartTime == shift.StartTime
                    && s.EndTime == shift.EndTime);

                if (duplicate != null)
                {
                    throw new InvalidOperationException(
                        $"Ca làm việc {shift.StartTime:HH\\:mm}-{shift.EndTime:HH\\:mm} ngày {shift.ShiftDate} " +
                        "đã tồn tại cho cửa hàng này.");
                }
            }

            shift.UpdatedAt = DateTimeOffset.UtcNow;
            //shift.LockVersion++; // Increment Version

            // 7. Save to DB
            await _shiftRepository.UpdateAsync(shift);

            // 4. Add Outbox Message (ShiftUpdated)
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
                Type = "ShiftUpdated",
                Content = JsonSerializer.Serialize(@event),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _outboxRepository.Add(outboxMessage);

            _outboxSignal.Pulse();

            return Unit.Value;
        }
    }
}