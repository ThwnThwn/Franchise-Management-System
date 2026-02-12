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
        /// Dùng cho nhân viên checkin checkout.
        /// </summary>
        Task<ShiftAssignment?> GetActiveShiftAssignmentAsync(Guid userId, DateTime now, int bufferInMin, int bufferOutHour);

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

        /// <summary>
        /// Lấy danh sách phân công của NHIỀU nhân viên trong khoảng thời gian.
        /// Dùng cho GetAvailableStaffs để tránh N+1 Query.
        /// </summary>
        Task<IEnumerable<ShiftAssignment>> GetAssignmentsByUsersAsync(IEnumerable<Guid> userIds, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy TẤT CẢ assignment của 1 ca, BAO GỒM CẢ SOFT-DELETED.
        /// Dùng cho Sync Logic: khi user bị xóa rồi gán lại, cần restore row cũ
        /// thay vì INSERT mới (tránh vi phạm UNIQUE constraint shift_id + user_id).
        /// </summary>
        Task<List<ShiftAssignment>> GetAllAssignmentsIncludingDeletedAsync(Guid shiftId, CancellationToken cancellationToken);

        /// <summary>
        /// Khôi phục assignment đã bị soft-delete.
        /// Dùng khi user bị xóa rồi gán lại cùng ca.
        /// Bypass Global Query Filter để tìm và update entity.
        /// </summary>
        Task RestoreAssignmentAsync(ShiftAssignment assignment, CancellationToken cancellationToken);
    }
}