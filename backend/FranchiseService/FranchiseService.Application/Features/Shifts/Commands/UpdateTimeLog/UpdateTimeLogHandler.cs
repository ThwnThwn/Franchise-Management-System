using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateTimeLog;

/// <summary>
/// Handler cho Manager duyệt công.
/// Cập nhật trạng thái chấm công, lý do, và tính lại tổng số phút làm việc nếu chuyển về Valid.
/// </summary>
public class UpdateTimeLogHandler : IRequestHandler<UpdateTimeLogCommand, bool>
{
    private readonly IGenericRepository<TimeLog> _timeLogRepository;

    public UpdateTimeLogHandler(IGenericRepository<TimeLog> timeLogRepository)
    {
        _timeLogRepository = timeLogRepository;
    }

    public async Task<bool> Handle(UpdateTimeLogCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trạng thái truyền vào phải là giá trị hợp lệ trong enum
        if (!Enum.IsDefined(typeof(TimeLogStatus), request.Status))
            throw new ArgumentException($"Trạng thái không hợp lệ. Chỉ chấp nhận các giá trị từ 0 đến 3.");

        // Bắt buộc quản lý phải nhập lý do khi từ chối bản ghi chấm công
        if (request.Status == (int)TimeLogStatus.Invalid && string.IsNullOrWhiteSpace(request.ManagerNote))
            throw new ArgumentException("Bắt buộc phải nhập lý do khi từ chối bản ghi chấm công.");

        var timeLog = await _timeLogRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Không tìm thấy bản ghi chấm công: {request.Id}");

        // Cập nhật trạng thái và ghi chú lý do bù công hoặc từ chối do quản lý nhập
        timeLog.Status = request.Status;
        timeLog.ManagerNote = request.ManagerNote;
        timeLog.UpdatedAt = DateTimeOffset.UtcNow;

        // Tính lại tổng phút nếu sửa qua trạng thái Valid
        if (request.Status == (int)TimeLogStatus.Valid
            && timeLog.CheckInAt.HasValue
            && timeLog.CheckOutAt.HasValue)
        {
            timeLog.TotalWorkMinutes = (int)(timeLog.CheckOutAt.Value - timeLog.CheckInAt.Value).TotalMinutes;
        }

        await _timeLogRepository.UpdateAsync(timeLog);
        return true;
    }
}
