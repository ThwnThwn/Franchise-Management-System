using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository chuyên biệt cho Shift, kế thừa GenericRepository.
    /// Các query custom phải tự filter IsDeleted.
    /// </summary>
    public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
    {
        public ShiftRepository(IFranchiseDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Lấy ca làm việc đã Published của nhân viên trong ngày.
        /// Dùng cho nhân viên xem "Lịch của tôi".
        /// </summary>
        public async Task<ShiftAssignment?> GetActiveShiftAssignmentAsync(
            Guid userId, DateTime now, int bufferInMin, int bufferOutHour
        )
        {
            var today = DateOnly.FromDateTime(now);
            var yesterday = today.AddDays(-1);

            return await _dbContext.ShiftAssignments
                .Include(sa => sa.Shift).ThenInclude(s => s.Store)
                .Include(sa => sa.TimeLogs.Where(tl => !tl.IsDeleted))
                .AsNoTracking()
                .Where(sa => sa.UserId == userId
                          && !sa.IsDeleted
                          && sa.Shift.Status == (int)ShiftStatus.Published
                          && (sa.Shift.ShiftDate == today || sa.Shift.ShiftDate == yesterday))
                .Select(sa => new
                {
                    Assignment = sa,
                    ActualStart = sa.Shift.ShiftDate.ToDateTime(sa.Shift.StartTime),
                    ActualEnd = sa.Shift.StartTime < sa.Shift.EndTime
                        ? sa.Shift.ShiftDate.ToDateTime(sa.Shift.EndTime)
                        : sa.Shift.ShiftDate.AddDays(1).ToDateTime(sa.Shift.EndTime)
                })
                .Where(x => now >= x.ActualStart.AddMinutes(-bufferInMin)
                         && now <= x.ActualEnd.AddHours(bufferOutHour))
                // --- LOGIC GIẢI QUYẾT XUNG ĐỘT ---
                .OrderByDescending(x => x.Assignment.TimeLogs.Any(tl => tl.CheckInAt != null && tl.CheckOutAt == null)) // 1. Ưu tiên ca đang In-Progress
                .ThenByDescending(x => !x.Assignment.TimeLogs.Any()) // 2. Ưu tiên ca chưa chấm công tí nào (so với ca đã hoàn thành nhưng vẫn trong buffer)
                .ThenBy(x => x.ActualStart) // 3. Ưu tiên ca nào bắt đầu sớm hơn
                .Select(x => x.Assignment)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách ca làm việc (không Include Assignment).
        /// Dùng để kiểm tra trùng lặp khi tạo/cập nhật ca.
        /// </summary>
        public async Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(Guid storeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
        {
            return await _dbContext.Shifts
                .AsNoTracking()
                .Where(s => s.StoreId == storeId
                         && s.ShiftDate >= fromDate
                         && s.ShiftDate <= toDate
                         && s.IsDeleted == false)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách ca làm việc kèm Assignments (Include).
        /// Hỗ trợ filter theo status. Dùng cho API GetAll trả về FE.
        /// </summary>
        public async Task<IEnumerable<Shift>> GetShiftsWithAssignmentsAsync(
            Guid storeId, DateOnly fromDate, DateOnly toDate, int? status, CancellationToken cancellationToken)
        {
            var query = _dbContext.Shifts
                .AsNoTracking()
                .Include(s => s.ShiftAssignments.Where(a => a.IsDeleted == false))
                .Where(s => s.StoreId == storeId
                         && s.ShiftDate >= fromDate
                         && s.ShiftDate <= toDate
                         && s.IsDeleted == false);

            // Filter theo trạng thái nếu có
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy chi tiết 1 ca kèm Assignments.
        /// Dùng cho API GetById.
        /// </summary>
        public async Task<Shift?> GetShiftByIdWithAssignmentsAsync(Guid shiftId, CancellationToken cancellationToken)
        {
            return await _dbContext.Shifts
                .AsNoTracking()
                .Include(s => s.ShiftAssignments.Where(a => a.IsDeleted == false))
                .Where(s => s.Id == shiftId && s.IsDeleted == false)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách phân công của nhân viên trong khoảng thời gian.
        /// Dùng để check trùng ca và tính tổng giờ/tuần.
        /// </summary>
        public async Task<IEnumerable<ShiftAssignment>> GetAssignmentsByUserAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
        {
            return await _dbContext.ShiftAssignments
                .AsNoTracking()
                .Include(a => a.Shift)
                .Where(a => a.UserId == userId
                         && a.Shift.ShiftDate >= fromDate
                         && a.Shift.ShiftDate <= toDate
                         && a.IsDeleted == false
                         && a.Shift.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách phân công của NHIỀU nhân viên trong khoảng thời gian.
        /// Dùng cho GetAvailableStaffs để tránh N+1 Query.
        /// </summary>
        public async Task<IEnumerable<ShiftAssignment>> GetAssignmentsByUsersAsync(
            IEnumerable<Guid> userIds, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
        {
            return await _dbContext.ShiftAssignments
                .AsNoTracking()
                .Include(a => a.Shift)
                .Where(a => userIds.Contains(a.UserId)
                         && a.Shift.ShiftDate >= fromDate
                         && a.Shift.ShiftDate <= toDate
                         && a.IsDeleted == false
                         && a.Shift.IsDeleted == false)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy TẤT CẢ assignment của 1 ca, BAO GỒM CẢ SOFT-DELETED.
        /// KHÔNG filter IsDeleted để Sync Logic có thể tìm row cũ và restore.
        /// </summary>
        public async Task<List<ShiftAssignment>> GetAllAssignmentsIncludingDeletedAsync(
            Guid shiftId, CancellationToken cancellationToken)
        {
            return await _dbContext.ShiftAssignments
                .IgnoreQueryFilters()  // Bypass Global Query Filter (IsDeleted == false)
                .Where(a => a.ShiftId == shiftId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Khôi phục assignment đã bị soft-delete.
        /// Bypass Global Query Filter để tìm entity và update.
        /// </summary>
        public async Task RestoreAssignmentAsync(ShiftAssignment assignment, CancellationToken cancellationToken)
        {
            // Tim entity bao gom ca soft-deleted (bypass Global Query Filter)
            var entity = await _dbContext.ShiftAssignments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == assignment.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Khong tim thay ShiftAssignment {assignment.Id}");

            // Cap nhat tu entity truyen vao
            entity.IsDeleted = assignment.IsDeleted;
            entity.Status = assignment.Status;
            entity.HourlyWage = assignment.HourlyWage;
            entity.UpdatedAt = assignment.UpdatedAt;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách lịch sử chấm công theo bộ lọc và phân trang.
        /// Cần Include ShiftAssignment và Shift để lấy thông tin ngày tháng bắt đầu ca.
        /// </summary>
        public async Task<(IEnumerable<TimeLog> Items, int TotalCount)> GetTimeLogsAsync(
            Guid? storeId,
            Guid? staffId,
            int? status,
            DateOnly fromDate,
            DateOnly toDate,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            // Query TimeLogs thay vì Shift
            var query = _dbContext.Set<TimeLog>()
                .Include(tl => tl.ShiftAssignment)
                    .ThenInclude(sa => sa.Shift)
                .AsNoTracking()
                .Where(tl => !tl.IsDeleted && !tl.ShiftAssignment.IsDeleted && !tl.ShiftAssignment.Shift.IsDeleted);

            // 1. Lọc theo khoảng ngày (dựa trên thông tin ca làm việc)
            query = query.Where(tl => 
                tl.ShiftAssignment.Shift.ShiftDate >= fromDate && 
                tl.ShiftAssignment.Shift.ShiftDate <= toDate);

            // 2. Lọc theo StoreId (Dành cho Manager)
            if (storeId.HasValue)
            {
                query = query.Where(tl => tl.ShiftAssignment.Shift.StoreId == storeId.Value);
            }

            // 3. Lọc theo StaffId (Dành cho Manager lọc riêng hoặc Staff xem của mình)
            if (staffId.HasValue)
            {
                query = query.Where(tl => tl.ShiftAssignment.UserId == staffId.Value);
            }

            // 4. Lọc theo Status
            if (status.HasValue)
            {
                query = query.Where(tl => tl.Status == status.Value);
            }

            // 5. Đếm tổng số bản ghi (trước khi phân trang)
            var totalCount = await query.CountAsync(cancellationToken);

            // 6. Phân trang và sắp xếp (ưu tiên ca làm việc mới nhất)
            var items = await query
                .OrderByDescending(tl => tl.ShiftAssignment.Shift.ShiftDate)
                .ThenByDescending(tl => tl.ShiftAssignment.Shift.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}