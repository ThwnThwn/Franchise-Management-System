using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.CheckOut;

/// <summary>
/// Handler xử lý Check-out.
/// Cập nhật TimeLog đã tạo khi Check-in: thêm thời gian checkout, tọa độ, và tính tổng số phút làm việc.
/// </summary>
public class CheckOutHandler : IRequestHandler<CheckOutCommand, bool>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepository;

    public CheckOutHandler(
        IGenericRepository<TimeLog> timeLogRepository,
        IGenericRepository<ShiftAssignment> assignmentRepository)
    {
        _timeLogRepository = timeLogRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<bool> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm bản ghi TimeLog
        var timeLog = await _timeLogRepository.GetByIdAsync(request.LogId)
             ?? throw new KeyNotFoundException($"Không tìm thấy bản ghi chấm công: {request.LogId}");

        // 2. Validate quyền sở hữu qua bản ghi ShiftAssignment
        var assignment = await _assignmentRepository.GetByIdAsync(timeLog.ShiftAssignmentId)
             ?? throw new KeyNotFoundException("Không tìm thấy phân công ca.");

        if (assignment.UserId != request.UserId)
            throw new UnauthorizedAccessException("Bạn không có quyền check-out ca này.");

        // 3. Kiểm tra xem người dùng đã check-out chưa
        if (timeLog.CheckOutAt.HasValue)
            throw new InvalidOperationException("Bạn đã check-out rồi.");

        // 4. Cập nhật thông tin check-out
        timeLog.CheckOutAt = DateTime.UtcNow;
        timeLog.CheckOutLat = request.Latitude;
        timeLog.CheckOutLong = request.Longitude;
        timeLog.UpdatedAt = DateTimeOffset.UtcNow;

        // 5. Thêm note check-out nếu có vào cột ManagerNote
        if (!string.IsNullOrEmpty(request.Note))
        {
            timeLog.ManagerNote = string.IsNullOrEmpty(timeLog.ManagerNote)
                ? request.Note
                : $"{timeLog.ManagerNote} | Note checkout: {request.Note}";
        }

        // 6. Tính tổng số phút làm việc dựa trên thời gian check-in và check-out
        if (timeLog.CheckInAt.HasValue)
        {
            timeLog.TotalWorkMinutes = (int)(timeLog.CheckOutAt.Value - timeLog.CheckInAt.Value).TotalMinutes;
        }

        // 7. Chuyển trạng thái sang Valid sau khi hoàn tất check-out
        timeLog.Status = (int)TimeLogStatus.Valid;

        // Hàm UpdateAsync tự động gọi SaveChangesAsync thiết lập sẵn trong GenericRepository
        await _timeLogRepository.UpdateAsync(timeLog);

        return true;
    }
}
