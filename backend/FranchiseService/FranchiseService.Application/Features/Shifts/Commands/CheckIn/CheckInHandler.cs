using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckIn;

/// <summary>
/// Handler xử lý Check-in.
/// Tận dụng GenericRepository để thao tác với TimeLog và ShiftAssignment.
/// </summary>
public class CheckInHandler : IRequestHandler<CheckInCommand, Guid>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;

    public CheckInHandler(
        IGenericRepository<TimeLog> timeLogRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository,
        IGenericRepository<Shift> shiftRepository)
    {
        _timeLogRepository = timeLogRepository;
        _assignmentRepository = assignmentRepository;
        _shiftRepository = shiftRepository;
    }

    public async Task<Guid> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate phân công tồn tại và thuộc về user
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy phân công ca làm việc: {request.AssignmentId}");

        if (assignment.UserId != request.UserId)
            throw new UnauthorizedAccessException("Bạn không có quyền chấm công cho ca này.");

        // Kiểm tra ca làm việc phải ở trạng thái đã xuất bản mới cho phép check in
        var shift = await _shiftRepository.GetByIdAsync(assignment.ShiftId)
            ?? throw new KeyNotFoundException("Không tìm thấy ca làm việc.");

        if (shift.Status != (int)ShiftStatus.Published)
            throw new InvalidOperationException("Chỉ có thể check in khi ca làm việc đã được phát hành.");

        // 2. Kiểm tra chưa check-in trước đó để tránh tạo trùng TimeLog
        var existingLogs = await _timeLogRepository.GetAllAsync(
            predicate: tl => tl.ShiftAssignmentId == request.AssignmentId);

        if (existingLogs.Any(tl => tl.CheckOutAt == null))
            throw new InvalidOperationException("Bạn đã check-in rồi. Hãy check-out trước.");

        // 3. Tạo TimeLog mới
        var timeLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = request.AssignmentId,
            CheckInAt = DateTime.UtcNow, // Lưu UTC time để đồng bộ FE
            CheckInLat = request.Latitude,
            CheckInLong = request.Longitude,
            Status = (int)TimeLogStatus.Processing,
            ManagerNote = request.Note, // Lưu note nhân viên/hệ thống tạm thời vào đây theo Cách 1
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Hàm AddAsync từ GenericRepository đã tự động gọi SaveChangesAsync
        await _timeLogRepository.AddAsync(timeLog);

        return timeLog.Id;
    }
}
