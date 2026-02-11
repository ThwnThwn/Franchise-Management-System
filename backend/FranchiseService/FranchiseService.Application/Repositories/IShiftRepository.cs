using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Repositories
{
    /// <summary>
    /// Repository chuyên biệt cho Shift.
    /// Cung cấp các query custom ngoài GenericRepository.
    /// </summary>
    public interface IShiftRepository
    {
        /// <summary>
        /// Lấy ca làm việc đã Published của nhân viên trong ngày.
        /// Dùng cho nhân viên xem "Lịch của tôi".
        /// </summary>
        Task<Shift?> GetPublishedShiftAsync(Guid userId, DateTime date, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy danh sách ca làm việc của Store trong khoảng thời gian.
        /// Dùng cho Manager xem lịch tổng hoặc kiểm tra trùng lặp.
        /// </summary>
        Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(Guid storeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy danh sách ca làm việc kèm danh sách phân công (Include Assignments).
        /// Hỗ trợ filter theo status, dùng cho API GetAll trả về FE.
        /// </summary>
        Task<IEnumerable<Shift>> GetShiftsWithAssignmentsAsync(Guid storeId, DateOnly fromDate, DateOnly toDate, int? status, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy chi tiết 1 ca kèm danh sách phân công.
        /// Dùng cho API GetById.
        /// </summary>
        Task<Shift?> GetShiftByIdWithAssignmentsAsync(Guid shiftId, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy danh sách phân công của nhân viên trong khoảng thời gian.
        /// Dùng để check trùng ca và tính tổng giờ/tuần.
        /// </summary>
        Task<IEnumerable<ShiftAssignment>> GetAssignmentsByUserAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);
    }
}