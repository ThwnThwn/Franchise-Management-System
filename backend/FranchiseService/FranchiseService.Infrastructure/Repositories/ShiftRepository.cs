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
    }
}