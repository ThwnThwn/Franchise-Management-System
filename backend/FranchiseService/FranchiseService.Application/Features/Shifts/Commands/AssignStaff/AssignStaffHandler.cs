using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using SharedKernel.Events;
using MediatR;
using System.Text.Json;

namespace FranchiseService.Application.Features.Shifts.Commands.AssignStaff
{
    /// <summary>
    /// Handler xử lý gán nhân viên vào ca làm việc (batch mode).
    /// Hỗ trợ gán nhiều nhân viên cùng lúc.
    /// Kiểm tra cho TỪNG user: trùng ca, nghỉ tối thiểu, giới hạn giờ tuần, số lượng tối đa.
    /// </summary>
    public class AssignStaffHandler : IRequestHandler<AssignStaffCommand, Unit>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IGenericRepository<Shift> _shiftGenericRepo;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
        private readonly IGenericRepository<ShiftRule> _ruleRepo;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;
        private readonly IIdentityServiceClient _identityClient;

        public AssignStaffHandler(
            IShiftRepository shiftRepository,
            IGenericRepository<Shift> shiftGenericRepo,
            IGenericRepository<ShiftAssignment> assignmentRepo,
            IGenericRepository<ShiftRule> ruleRepo,
            IOutboxRepository outboxRepository,
            Application.Signals.IOutboxSignal outboxSignal,
            IIdentityServiceClient identityClient)
        {
            _shiftRepository = shiftRepository;
            _shiftGenericRepo = shiftGenericRepo;
            _assignmentRepo = assignmentRepo;
            _ruleRepo = ruleRepo;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _identityClient = identityClient;
        }

        public async Task<Unit> Handle(AssignStaffCommand request, CancellationToken cancellationToken)
        {
            //  1. Load Shift 
            var shift = await _shiftGenericRepo.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Không tìm thấy Ca làm việc {request.ShiftId}");

            // 2. Kiểm tra trạng thái ca 
            // Chỉ cho phép gán khi Draft hoặc Published
            ValidateShiftStatus(shift);

            // 3. Load quy tắc Store
            var rules = await _ruleRepo.GetAllAsync(predicate: r => r.StoreId == shift.StoreId);
            var storeRule = rules.FirstOrDefault();

            // Giá trị mặc định nếu chưa cấu hình
            int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;
            int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;

            // 4. Đếm assignment hiện tại để kiểm tra capacity 
            var currentAssignments = await _assignmentRepo.GetAllAsync(
                predicate: a => a.ShiftId == request.ShiftId);
            int currentCount = currentAssignments.Count();
            int availableSlots = shift.RequiredStaffCount - currentCount;

            if (availableSlots <= 0)
            {
                throw new InvalidOperationException(
                    $"Ca này đã đủ {shift.RequiredStaffCount} nhân viên (hiện có {currentCount}).");
            }

            if (request.UserIds.Count > availableSlots)
            {
                throw new InvalidOperationException(
                    $"Không đủ chỗ: cần gán {request.UserIds.Count} nhân viên " +
                    $"nhưng chỉ còn {availableSlots} slot trống " +
                    $"(tối đa {shift.RequiredStaffCount}, hiện có {currentCount}).");
            }

            // 5. Validate và tạo assignment cho từng user
            var newAssignments = new List<ShiftAssignment>();
            var outboxMessages = new List<OutboxMessage>();

            foreach (var userId in request.UserIds)
            {
                // Validate user tồn tại qua gRPC → IdentityService
                var userInfo = await _identityClient.GetUserByIdAsync(userId)
                    ?? throw new KeyNotFoundException(
                        $"Nhân viên {userId} không tồn tại trong hệ thống.");

                // Lấy danh sách assignment hiện tại của user trong cùng ngày
                var userDailyAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                    userId, shift.ShiftDate, shift.ShiftDate, cancellationToken);

                // Kiểm tra trùng assignment (user đã được gán vào ca này chưa)
                if (userDailyAssignments.Any(a => a.ShiftId == request.ShiftId))
                {
                    throw new InvalidOperationException(
                        $"Nhân viên {userInfo.Username} ({userId}) đã được gán vào ca này rồi.");
                }

                //  Kiểm tra trùng giờ (Overlap) với các ca khác trong ngày
                foreach (var existing in userDailyAssignments)
                {
                    if (IsOverlap(shift.StartTime, shift.EndTime,
                        existing.Shift.StartTime, existing.Shift.EndTime))
                    {
                        throw new InvalidOperationException(
                            $"Nhân viên {userInfo.Username} ({userId}) có ca trùng giờ: " +
                            $"{existing.Shift.StartTime:HH\\:mm} - {existing.Shift.EndTime:HH\\:mm}.");
                    }
                }

                //  Kiểm tra khoảng nghỉ tối thiểu giữa 2 ca
                // Mở rộng phạm vi: ngày trước + ngày sau (xử lý ca qua đêm)
                var extendedAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                    userId, shift.ShiftDate.AddDays(-1), shift.ShiftDate.AddDays(1), cancellationToken);

                ValidateMinHoursBetweenShifts(shift, extendedAssignments, minHoursBreak, userId);

                //  Kiểm tra giới hạn giờ làm/tuần
                await ValidateMaxHoursPerWeek(shift, userId, maxHoursPerWeek, cancellationToken);

                //  Tạo Assignment mới
                var assignment = new ShiftAssignment
                {
                    Id = Guid.NewGuid(),
                    ShiftId = shift.Id,
                    UserId = userId,
                    HourlyWage = userInfo.CurrentHourlyWage, // Snapshot lương hiện tại
                    Status = 0, // Assigned
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                newAssignments.Add(assignment);

                //  Tạo Outbox Message cho từng assignment
                var @event = new ShiftAssignedEvent
                {
                    AssignmentId = assignment.Id,
                    ShiftId = shift.Id,
                    UserId = userId,
                    StoreId = shift.StoreId,
                    HourlyWage = assignment.HourlyWage ?? 0
                };

                outboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "ShiftAssigned",
                    Content = JsonSerializer.Serialize(@event),
                    OccurredOn = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            // 6. Lưu tất cả assignments 
            foreach (var assignment in newAssignments)
            {
                await _assignmentRepo.AddAsync(assignment);
            }

            // 7. Lưu tất cả outbox messages
            foreach (var message in outboxMessages)
            {
                _outboxRepository.Add(message);
            }

            // 8. Đánh thức OutboxWorker
            _outboxSignal.Pulse();

            return Unit.Value;
        }

        //PRIVATE METHODS

        /// <summary>
        /// Kiểm tra trạng thái ca: chỉ cho phép gán khi Draft hoặc Published.
        /// </summary>
        private static void ValidateShiftStatus(Shift shift)
        {
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Không thể gán nhân viên vào ca đã hoàn thành.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Không thể gán nhân viên vào ca đã hủy.");
        }

        /// <summary>
        /// Kiểm tra khoảng nghỉ tối thiểu giữa ca mới và các ca hiện tại.
        /// So sánh thời gian kết thúc ca trước với thời gian bắt đầu ca sau.
        /// </summary>
        private static void ValidateMinHoursBetweenShifts(
            Shift newShift,
            IEnumerable<ShiftAssignment> existingAssignments,
            int minHoursBreak,
            Guid userId)
        {
            // Chuyển ca mới thành DateTime tuyệt đối để so sánh chính xác
            var newShiftStart = newShift.ShiftDate.ToDateTime(newShift.StartTime);
            var newShiftEnd = newShift.ShiftDate.ToDateTime(newShift.EndTime);

            // Nếu ca qua đêm (EndTime <= StartTime) → kết thúc ngày hôm sau
            if (newShift.EndTime <= newShift.StartTime)
                newShiftEnd = newShiftEnd.AddDays(1);

            foreach (var existing in existingAssignments)
            {
                var existStart = existing.Shift.ShiftDate.ToDateTime(existing.Shift.StartTime);
                var existEnd = existing.Shift.ShiftDate.ToDateTime(existing.Shift.EndTime);

                if (existing.Shift.EndTime <= existing.Shift.StartTime)
                    existEnd = existEnd.AddDays(1);

                // Tính khoảng cách giữa 2 ca (lấy gap nhỏ nhất)
                double gapHours;
                if (newShiftStart >= existEnd)
                {
                    // Ca mới nằm SAU ca hiện tại
                    gapHours = (newShiftStart - existEnd).TotalHours;
                }
                else if (existStart >= newShiftEnd)
                {
                    // Ca mới nằm TRƯỚC ca hiện tại
                    gapHours = (existStart - newShiftEnd).TotalHours;
                }
                else
                {
                    // 2 ca chồng chéo → gap = 0 (sẽ bị bắt bởi overlap check trước đó)
                    continue;
                }

                if (gapHours < minHoursBreak)
                {
                    throw new InvalidOperationException(
                        $"Nhân viên {userId}: Vi phạm quy tắc nghỉ tối thiểu {minHoursBreak}h giữa 2 ca. " +
                        $"Khoảng cách hiện tại: {gapHours:F1}h " +
                        $"(Ca {existing.Shift.StartTime:HH\\:mm}-{existing.Shift.EndTime:HH\\:mm} " +
                        $"ngày {existing.Shift.ShiftDate}).");
                }
            }
        }

        /// <summary>
        /// Kiểm tra tổng giờ làm trong tuần không vượt quá giới hạn.
        /// Tuần tính từ Thứ Hai → Chủ Nhật.
        /// Có trừ BreakDurationMinutes khi tính giờ.
        /// </summary>
        private async Task ValidateMaxHoursPerWeek(
            Shift newShift, Guid userId, int maxHoursPerWeek, CancellationToken cancellationToken)
        {
            // Tìm ngày đầu tuần (Thứ Hai)
            int daysSinceMonday = (int)newShift.ShiftDate.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7; // Chủ Nhật → +6

            var startOfWeek = newShift.ShiftDate.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);

            var weeklyAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                userId, startOfWeek, endOfWeek, cancellationToken);

            // Tính tổng giờ làm (đã trừ break)
            double totalHoursWorked = weeklyAssignments.Sum(a =>
                CalculateNetHours(a.Shift.StartTime, a.Shift.EndTime, a.Shift.BreakDurationMinutes));

            double newShiftNetHours = CalculateNetHours(
                newShift.StartTime, newShift.EndTime, newShift.BreakDurationMinutes);

            if (totalHoursWorked + newShiftNetHours > maxHoursPerWeek)
            {
                throw new InvalidOperationException(
                    $"Nhân viên {userId}: Sẽ vượt quá {maxHoursPerWeek}h/tuần " +
                    $"(Hiện tại: {totalHoursWorked:F1}h + Ca mới: {newShiftNetHours:F1}h = " +
                    $"{totalHoursWorked + newShiftNetHours:F1}h).");
            }
        }

        /// <summary>
        /// Kiểm tra 2 khoảng thời gian có trùng nhau không.
        /// Hỗ trợ ca qua đêm (StartTime > EndTime).
        /// </summary>
        private static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
        {
            // Chuyển sang phút tuyệt đối, xử lý ca qua đêm bằng cách cộng 1440 (24h)
            int start1 = s1.Hour * 60 + s1.Minute;
            int end1 = e1.Hour * 60 + e1.Minute;
            if (end1 <= start1) end1 += 1440;

            int start2 = s2.Hour * 60 + s2.Minute;
            int end2 = e2.Hour * 60 + e2.Minute;
            if (end2 <= start2) end2 += 1440;

            return start1 < end2 && end1 > start2;
        }

        /// <summary>
        /// Tính số giờ làm thực tế (đã trừ thời gian nghỉ).
        /// Hỗ trợ ca qua đêm.
        /// </summary>
        private static double CalculateNetHours(TimeOnly start, TimeOnly end, int breakDurationMinutes)
        {
            double grossHours;
            if (end <= start) // Ca qua đêm
            {
                grossHours = (1440 - (start.Hour * 60 + start.Minute) + (end.Hour * 60 + end.Minute)) / 60.0;
            }
            else
            {
                grossHours = (end.Hour * 60 + end.Minute - start.Hour * 60 - start.Minute) / 60.0;
            }

            // Trừ thời gian nghỉ
            return Math.Max(0, grossHours - (breakDurationMinutes / 60.0));
        }
    }
}
