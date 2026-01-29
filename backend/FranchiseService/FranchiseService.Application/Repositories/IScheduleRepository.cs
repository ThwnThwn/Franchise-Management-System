using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Repositories
{
    public interface IScheduleRepository
    {
        // Lấy lịch làm việc đã Public của nhân viên trong ngày
        Task<Schedule?> GetPublishedShiftAsync(Guid userId, DateTime date, CancellationToken cancellationToken);
    }
}