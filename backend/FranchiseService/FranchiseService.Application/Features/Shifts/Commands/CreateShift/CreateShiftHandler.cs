using System.Text.Json;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;
using SharedKernel.Events;

namespace FranchiseService.Application.Features.Shifts.Commands.CreateShift
{
    /// <summary>
    /// Handler xử lý tạo ca làm việc mới.
    /// Hỗ trợ tạo từ Template hoặc nhập tay, kiểm tra trùng lặp.
    /// </summary>
    public class CreateShiftHandler : IRequestHandler<CreateShiftCommand, Guid>
    {
        private readonly IGenericRepository<Shift> _genericRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IGenericRepository<ShiftTemplate> _shiftTemplateRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;

        public CreateShiftHandler(
            IGenericRepository<Shift> genericRepository,
            IShiftRepository shiftRepository,
            IOutboxRepository outboxRepository,
            IGenericRepository<ShiftTemplate> shiftTemplateRepository,
            Application.Signals.IOutboxSignal outboxSignal)
        {
            _genericRepository = genericRepository;
            _shiftRepository = shiftRepository;
            _outboxRepository = outboxRepository;
            _shiftTemplateRepository = shiftTemplateRepository;
            _outboxSignal = outboxSignal;
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Xác định thời gian làm việc 
            // Ưu tiên: Request > Template > Lỗi
            TimeOnly finalStartTime = request.StartTime ?? default;
            TimeOnly finalEndTime = request.EndTime ?? default;
            int finalBreakDuration = request.BreakDurationMinutes ?? 0;

            if (request.ShiftTemplateId.HasValue)
            {
                // Fetch Template và áp dụng giờ giấc (nếu Request không gửi kèm)
                var template = await _shiftTemplateRepository.GetByIdAsync(request.ShiftTemplateId.Value)
                    ?? throw new KeyNotFoundException($"Không tìm thấy Mẫu ca làm việc với ID: {request.ShiftTemplateId}");

                // Ưu tiên Request > Template
                if (!request.StartTime.HasValue) finalStartTime = template.StartTime;
                if (!request.EndTime.HasValue) finalEndTime = template.EndTime;
                if (!request.BreakDurationMinutes.HasValue) finalBreakDuration = template.BreakDurationMinutes ?? 0;
            }

            // Kiểm tra bắt buộc có thời gian làm việc
            if (finalStartTime == default && finalEndTime == default)
            {
                throw new FluentValidation.ValidationException(
                    "Không thể xác định thời gian làm việc. Vui lòng nhập Giờ bắt đầu/Kết thúc HOẶC chọn Mẫu ca hợp lệ.");
            }

            // 2. Kiểm tra trùng lặp
            // Cùng Store + Ngày + Giờ bắt đầu + Giờ kết thúc → Không cho tạo ca mới
            var existingShifts = await _shiftRepository.GetShiftsByDateRangeAsync(
                request.StoreId, request.ShiftDate, request.ShiftDate, cancellationToken);

            if (existingShifts.Any(s => s.StartTime == finalStartTime && s.EndTime == finalEndTime))
            {
                throw new InvalidOperationException(
                    $"Ca làm việc {finalStartTime:HH\\:mm}-{finalEndTime:HH\\:mm} ngày {request.ShiftDate} đã tồn tại cho cửa hàng này. " +
                    "Hãy cập nhật số lượng nhân viên (RequiredStaffCount) thay vì tạo ca mới.");
            }

            // 3. Tạo entity Shift
            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                StoreId = request.StoreId,
                ShiftTemplateId = request.ShiftTemplateId,
                ShiftDate = request.ShiftDate,
                StartTime = finalStartTime,
                EndTime = finalEndTime,
                RequiredStaffCount = request.RequiredStaffCount,
                Note = request.Note,
                BreakDurationMinutes = finalBreakDuration,
                Status = 0, // Draft — Manager phải Publish sau
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LockVersion = 1
            };

            await _genericRepository.AddAsync(shift);

            // 4. Tạo Outbox Message (ShiftCreated)
            var @event = new ShiftCreatedEvent
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
                Type = "ShiftCreated",
                Content = JsonSerializer.Serialize(@event),
                OccurredOn = DateTimeOffset.UtcNow,
                ProcessedOn = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _outboxRepository.Add(outboxMessage);

            // 5. Đánh thức OutboxWorker
            _outboxSignal.Pulse();

            return shift.Id;
        }
    }
}
