using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Repositories
{
    public interface IShiftRepository
    {
        // Lấy lịch làm việc đã Public của nhân viên trong ngày
        Task<Shift?> GetPublishedShiftAsync(Guid userId, DateTime date, CancellationToken cancellationToken);
    }
}