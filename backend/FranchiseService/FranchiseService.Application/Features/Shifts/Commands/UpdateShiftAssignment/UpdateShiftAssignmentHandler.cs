using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using SharedKernel.Events;
using MediatR;
using System.Text.Json;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment
{
    // Handler xu ly dong bo danh sach nhan vien trong ca lam viec (Sync Logic)
    // So sanh mang userIds tu request voi DB de tim ra: ai can them, ai can xoa, ai giu nguyen
    public class UpdateShiftAssignmentHandler : IRequestHandler<UpdateShiftAssignmentCommand, Unit>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IGenericRepository<Shift> _shiftGenericRepo;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
        private readonly IGenericRepository<ShiftRule> _ruleRepo;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;
        private readonly IIdentityServiceClient _identityClient;

        public UpdateShiftAssignmentHandler(
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

        public async Task<Unit> Handle(UpdateShiftAssignmentCommand request, CancellationToken cancellationToken)
        {
            // Load ca lam viec 
            var shift = await _shiftGenericRepo.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Khong tim thay Ca lam viec {request.ShiftId}");

            // Kiem tra trang thai ca 
            ValidateShiftStatus(shift);

            // Load quy tac Store (neu co)
            var rules = await _ruleRepo.GetAllAsync(predicate: r => r.StoreId == shift.StoreId);
            var storeRule = rules.FirstOrDefault();

            int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;
            int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;

            // === CORE FIX: Load TAT CA assignment CUNG SOFT-DELETED ===
            // GenericRepository.GetAllAsync tu dong filter IsDeleted == false
            // => Khi user bi xoa roi gan lai, se khong tim thay row cu => INSERT => UNIQUE violation
            // => Dung method moi: GetAllAssignmentsIncludingDeletedAsync (khong filter IsDeleted)
            var allAssignments = await _shiftRepository.GetAllAssignmentsIncludingDeletedAsync(
                request.ShiftId, cancellationToken);

            var activeAssignments = allAssignments.Where(a => !a.IsDeleted).ToList();
            var deletedAssignments = allAssignments.Where(a => a.IsDeleted).ToList();

            var activeUserIds = activeAssignments.Select(a => a.UserId).ToHashSet();
            var requestUserIds = request.UserIds.ToHashSet();

            // Tinh toan Diff
            var toAdd = requestUserIds.Except(activeUserIds).ToList();
            var toRemove = activeUserIds.Except(requestUserIds).ToList();

            // Kiem tra so luong nhan vien sau khi sync
            int finalCount = activeAssignments.Count - toRemove.Count + toAdd.Count;
            if (finalCount > shift.RequiredStaffCount)
            {
                throw new InvalidOperationException(
                    $"Vuot qua so luong cho phep: ca nay can toi da {shift.RequiredStaffCount} nhan vien, " +
                    $"nhung sau khi dong bo se co {finalCount} nguoi.");
            }

            var outboxMessages = new List<OutboxMessage>();

            // === XOA: Soft Delete assignments khong con trong request ===
            foreach (var userId in toRemove)
            {
                var assignment = activeAssignments.First(a => a.UserId == userId);
                await _assignmentRepo.DeleteAsync(assignment.Id);
                outboxMessages.Add(CreateUnassignedOutboxMessage(assignment, shift));
            }

            // === THEM: Validate va tao/restore assignment ===
            foreach (var userId in toAdd)
            {
                // Validate user ton tai qua gRPC
                var userInfo = await _identityClient.GetUserByIdAsync(userId)
                    ?? throw new KeyNotFoundException(
                        $"Nhan vien {userId} khong ton tai trong he thong.");

                // Check trung gio (Overlap)
                var userDailyAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                    userId, shift.ShiftDate, shift.ShiftDate, cancellationToken);

                foreach (var existing in userDailyAssignments)
                {
                    if (IsOverlap(shift.StartTime, shift.EndTime,
                        existing.Shift.StartTime, existing.Shift.EndTime))
                    {
                        throw new InvalidOperationException(
                            $"Nhan vien {userInfo.Username} ({userId}) co ca trung gio: " +
                            $"{existing.Shift.StartTime:HH\\:mm} - {existing.Shift.EndTime:HH\\:mm}.");
                    }
                }

                // Check khoang nghi toi thieu
                var extendedAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                    userId, shift.ShiftDate.AddDays(-1), shift.ShiftDate.AddDays(1), cancellationToken);

                ValidateMinHoursBetweenShifts(shift, extendedAssignments, minHoursBreak, userId);

                // Check gioi han gio/tuan
                await ValidateMaxHoursPerWeek(shift, userId, maxHoursPerWeek, cancellationToken);

                // === FIX BUG 2: CHECK SOFT-DELETED TRƯỚC KHI INSERT ===
                // Neu user da tung duoc gan roi bi xoa => row cu van ton tai (is_deleted = true)
                // UNIQUE constraint (shift_id, user_id) se CHẶN insert moi
                // => RESTORE row cu thay vi tao moi
                var deletedAssignment = deletedAssignments.FirstOrDefault(a => a.UserId == userId);

                if (deletedAssignment != null)
                {
                    // RESTORE: set lai IsDeleted = false, cap nhat thong tin
                    deletedAssignment.IsDeleted = false;
                    deletedAssignment.Status = (int)ShiftAssignmentStatus.Assigned;
                    deletedAssignment.HourlyWage = userInfo.CurrentHourlyWage;
                    deletedAssignment.UpdatedAt = DateTimeOffset.UtcNow;

                    await _shiftRepository.RestoreAssignmentAsync(deletedAssignment, cancellationToken);

                    outboxMessages.Add(CreateAssignedOutboxMessage(
                        deletedAssignment.Id, shift, userId, deletedAssignment.HourlyWage ?? 0));
                }
                else
                {
                    // CREATE: user chua tung duoc gan => INSERT moi binh thuong
                    var assignment = new ShiftAssignment
                    {
                        Id = Guid.NewGuid(),
                        ShiftId = shift.Id,
                        UserId = userId,
                        HourlyWage = userInfo.CurrentHourlyWage,
                        Status = (int)ShiftAssignmentStatus.Assigned,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };

                    await _assignmentRepo.AddAsync(assignment);

                    outboxMessages.Add(CreateAssignedOutboxMessage(
                        assignment.Id, shift, userId, assignment.HourlyWage ?? 0));
                }
            }

            // Luu outbox messages
            foreach (var message in outboxMessages)
            {
                _outboxRepository.Add(message);
            }

            // Danh thuc OutboxWorker
            if (outboxMessages.Any())
            {
                _outboxSignal.Pulse();
            }

            return Unit.Value;
        }

        // === PRIVATE METHODS ===

        private static OutboxMessage CreateAssignedOutboxMessage(
            Guid assignmentId, Shift shift, Guid userId, decimal hourlyWage)
        {
            var assignedEvent = new ShiftAssignedEvent
            {
                AssignmentId = assignmentId,
                ShiftId = shift.Id,
                UserId = userId,
                StoreId = shift.StoreId,
                HourlyWage = hourlyWage
            };

            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ShiftAssigned",
                Content = JsonSerializer.Serialize(assignedEvent),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        private static OutboxMessage CreateUnassignedOutboxMessage(ShiftAssignment assignment, Shift shift)
        {
            var unassignedEvent = new ShiftUnassignedEvent
            {
                AssignmentId = assignment.Id,
                ShiftId = shift.Id,
                UserId = assignment.UserId,
                StoreId = shift.StoreId,
                ShiftDate = shift.ShiftDate
            };

            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ShiftUnassigned",
                Content = JsonSerializer.Serialize(unassignedEvent),
                OccurredOn = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        private static void ValidateShiftStatus(Shift shift)
        {
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Khong the gan nhan vien vao ca da hoan thanh.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Khong the gan nhan vien vao ca da huy.");
        }

        private static void ValidateMinHoursBetweenShifts(
            Shift newShift,
            IEnumerable<ShiftAssignment> existingAssignments,
            int minHoursBreak,
            Guid userId)
        {
            var newShiftStart = newShift.ShiftDate.ToDateTime(newShift.StartTime);
            var newShiftEnd = newShift.ShiftDate.ToDateTime(newShift.EndTime);

            if (newShift.EndTime <= newShift.StartTime)
                newShiftEnd = newShiftEnd.AddDays(1);

            foreach (var existing in existingAssignments)
            {
                var existStart = existing.Shift.ShiftDate.ToDateTime(existing.Shift.StartTime);
                var existEnd = existing.Shift.ShiftDate.ToDateTime(existing.Shift.EndTime);

                if (existing.Shift.EndTime <= existing.Shift.StartTime)
                    existEnd = existEnd.AddDays(1);

                double gapHours;
                if (newShiftStart >= existEnd)
                    gapHours = (newShiftStart - existEnd).TotalHours;
                else if (existStart >= newShiftEnd)
                    gapHours = (existStart - newShiftEnd).TotalHours;
                else
                    continue;

                if (gapHours < minHoursBreak)
                {
                    throw new InvalidOperationException(
                        $"Nhan vien {userId}: Vi pham quy tac nghi toi thieu {minHoursBreak}h giua 2 ca. " +
                        $"Khoang cach hien tai: {gapHours:F1}h " +
                        $"(Ca {existing.Shift.StartTime:HH\\:mm}-{existing.Shift.EndTime:HH\\:mm} " +
                        $"ngay {existing.Shift.ShiftDate}).");
                }
            }
        }

        private async Task ValidateMaxHoursPerWeek(
            Shift newShift, Guid userId, int maxHoursPerWeek, CancellationToken cancellationToken)
        {
            int daysSinceMonday = (int)newShift.ShiftDate.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;

            var startOfWeek = newShift.ShiftDate.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);

            var weeklyAssignments = await _shiftRepository.GetAssignmentsByUserAsync(
                userId, startOfWeek, endOfWeek, cancellationToken);

            double totalHoursWorked = weeklyAssignments.Sum(a =>
                CalculateNetHours(a.Shift.StartTime, a.Shift.EndTime, a.Shift.BreakDurationMinutes));

            double newShiftNetHours = CalculateNetHours(
                newShift.StartTime, newShift.EndTime, newShift.BreakDurationMinutes);

            if (totalHoursWorked + newShiftNetHours > maxHoursPerWeek)
            {
                throw new InvalidOperationException(
                    $"Nhan vien {userId}: Se vuot qua {maxHoursPerWeek}h/tuan " +
                    $"(Hien tai: {totalHoursWorked:F1}h + Ca moi: {newShiftNetHours:F1}h = " +
                    $"{totalHoursWorked + newShiftNetHours:F1}h).");
            }
        }

        private static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
        {
            int start1 = s1.Hour * 60 + s1.Minute;
            int end1 = e1.Hour * 60 + e1.Minute;
            if (end1 <= start1) end1 += 1440;

            int start2 = s2.Hour * 60 + s2.Minute;
            int end2 = e2.Hour * 60 + e2.Minute;
            if (end2 <= start2) end2 += 1440;

            return start1 < end2 && end1 > start2;
        }

        private static double CalculateNetHours(TimeOnly start, TimeOnly end, int breakDurationMinutes)
        {
            double grossHours;
            if (end <= start)
            {
                grossHours = (1440 - (start.Hour * 60 + start.Minute) + (end.Hour * 60 + end.Minute)) / 60.0;
            }
            else
            {
                grossHours = (end.Hour * 60 + end.Minute - start.Hour * 60 - start.Minute) / 60.0;
            }

            return Math.Max(0, grossHours - (breakDurationMinutes / 60.0));
        }
    }
}
