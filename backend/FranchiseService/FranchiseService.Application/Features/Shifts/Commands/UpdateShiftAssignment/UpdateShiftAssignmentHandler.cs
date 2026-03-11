using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using SharedKernel.Events;
using MediatR;
using System.Text.Json;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment
{
    /// <summary>
    /// Xử lý đồng bộ danh sách nhân viên trong ca làm việc.
    /// So sánh mảng userIds từ request với DB để tìm ra ai cần thêm, ai cần xóa.
    /// Hỗ trợ Override: khi có cảnh báo, FE có thể gửi lại với ForceOverride = true.
    /// </summary>
    public class UpdateShiftAssignmentHandler : IRequestHandler<UpdateShiftAssignmentCommand, AssignStaffResult>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IGenericRepository<Shift> _shiftGenericRepo;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
        private readonly IGenericRepository<ShiftRule> _ruleRepo;
        private readonly IOutboxRepository _outboxRepository;
        private readonly Application.Signals.IOutboxSignal _outboxSignal;
        private readonly IIdentityServiceClient _identityClient;
        private readonly IShiftValidationService _validationService;
        private readonly IShiftApplicationAutoRejectService _autoRejectService;

        public UpdateShiftAssignmentHandler(
            IShiftRepository shiftRepository,
            IGenericRepository<Shift> shiftGenericRepo,
            IGenericRepository<ShiftAssignment> assignmentRepo,
            IGenericRepository<ShiftRule> ruleRepo,
            IOutboxRepository outboxRepository,
            Application.Signals.IOutboxSignal outboxSignal,
            IIdentityServiceClient identityClient,
            IShiftValidationService validationService,
            IShiftApplicationAutoRejectService autoRejectService)
        {
            _shiftRepository = shiftRepository;
            _shiftGenericRepo = shiftGenericRepo;
            _assignmentRepo = assignmentRepo;
            _ruleRepo = ruleRepo;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _identityClient = identityClient;
            _validationService = validationService;
            _autoRejectService = autoRejectService;
        }

        public async Task<AssignStaffResult> Handle(UpdateShiftAssignmentCommand request, CancellationToken cancellationToken)
        {
            // Tải thông tin ca làm việc
            var shift = await _shiftGenericRepo.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Khong tim thay Ca lam viec {request.ShiftId}");

            // Kiểm tra trạng thái ca
            ValidateShiftStatus(shift);

            // Tải quy tắc của cửa hàng, nếu không có thì dùng giá trị mặc định
            var rules = await _ruleRepo.GetAllAsync(predicate: r => r.StoreId == shift.StoreId);
            var storeRule = rules.FirstOrDefault();

            int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;
            int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;

            // Tải tất cả assignment bao gồm cả bản đã xóa mềm
            // để tránh lỗi UNIQUE constraint khi gán lại nhân viên đã bị xóa trước đó
            var allAssignments = await _shiftRepository.GetAllAssignmentsIncludingDeletedAsync(
                request.ShiftId, cancellationToken);

            var activeAssignments = allAssignments.Where(a => !a.IsDeleted).ToList();
            var deletedAssignments = allAssignments.Where(a => a.IsDeleted).ToList();

            var activeUserIds = activeAssignments.Select(a => a.UserId).ToHashSet();
            var requestUserIds = request.UserIds.ToHashSet();

            // Tính toán sự khác biệt giữa danh sách hiện tại và yêu cầu
            var toAdd = requestUserIds.Except(activeUserIds).ToList();
            var toRemove = activeUserIds.Except(requestUserIds).ToList();

            // Kiểm tra số lượng nhân viên sau khi đồng bộ
            int finalCount = activeAssignments.Count - toRemove.Count + toAdd.Count;
            if (finalCount > shift.RequiredStaffCount)
            {
                throw new InvalidOperationException(
                    $"Vuot qua so luong cho phep: ca nay can toi da {shift.RequiredStaffCount} nhan vien, " +
                    $"nhung sau khi dong bo se co {finalCount} nguoi.");
            }

            var outboxMessages = new List<OutboxMessage>();

            // Xóa mềm các assignment không còn trong danh sách yêu cầu
            foreach (var userId in toRemove)
            {
                var assignment = activeAssignments.First(a => a.UserId == userId);
                await _assignmentRepo.DeleteAsync(assignment.Id);
                outboxMessages.Add(CreateUnassignedOutboxMessage(assignment, shift));
            }

            // Kiểm tra nhân viên tồn tại và cache lại thông tin để tránh gọi gRPC 2 lần
            var userInfoCache = new Dictionary<Guid, dynamic>();
            foreach (var userId in toAdd)
            {
                var userInfo = await _identityClient.GetUserByIdAsync(userId)
                    ?? throw new KeyNotFoundException(
                        $"Nhan vien {userId} khong ton tai trong he thong.");
                userInfoCache[userId] = userInfo;
            }

            // Validate tất cả nhân viên cần thêm bằng ShiftValidationService
            var allWarnings = new List<ValidationWarning>();
            foreach (var userId in toAdd)
            {
                var warnings = await _validationService.ValidateAssignment(
                    shift, userId, minHoursBreak, maxHoursPerWeek, cancellationToken);
                allWarnings.AddRange(warnings);
            }

            // Nếu có lỗi BLOCKING thì chặn luôn, không cho override
            if (allWarnings.Any(w => w.Severity == "BLOCKING"))
            {
                throw new InvalidOperationException(
                    string.Join(" | ", allWarnings
                        .Where(w => w.Severity == "BLOCKING")
                        .Select(w => w.Message)));
            }

            // Nếu có cảnh báo WARNING nhưng chưa xác nhận ghi đè, trả về cho FE
            // Success = true vì request hợp lệ, nhưng FE cần kiểm tra Warnings để hiện popup xác nhận
            if (allWarnings.Any(w => w.Severity == "WARNING") && !request.ForceOverride)
            {
                return new AssignStaffResult { Success = true, Warnings = allWarnings };
            }

            // Đánh dấu override nếu Manager đã xác nhận ghi đè cảnh báo
            bool shouldMarkOverridden = request.ForceOverride && allWarnings.Any();

            // Tạo hoặc khôi phục assignment cho từng nhân viên
            foreach (var userId in toAdd)
            {
                var userInfo = userInfoCache[userId];
                var deletedAssignment = deletedAssignments.FirstOrDefault(a => a.UserId == userId);

                if (deletedAssignment != null)
                {
                    // Khôi phục assignment đã bị xóa mềm trước đó
                    deletedAssignment.IsDeleted = false;
                    deletedAssignment.Status = (int)ShiftAssignmentStatus.Assigned;
                    deletedAssignment.HourlyWage = userInfo.CurrentHourlyWage;
                    deletedAssignment.IsOverridden = shouldMarkOverridden;
                    deletedAssignment.OverrideReason = shouldMarkOverridden ? request.OverrideReason : null;
                    deletedAssignment.OverriddenBy = shouldMarkOverridden ? request.OverriddenBy : null;
                    deletedAssignment.UpdatedAt = DateTimeOffset.UtcNow;

                    await _shiftRepository.RestoreAssignmentAsync(deletedAssignment, cancellationToken);

                    outboxMessages.Add(CreateAssignedOutboxMessage(
                        deletedAssignment.Id, shift, userId, deletedAssignment.HourlyWage ?? 0));
                }
                else
                {
                    // Tạo mới assignment
                    var assignment = new ShiftAssignment
                    {
                        Id = Guid.NewGuid(),
                        ShiftId = shift.Id,
                        UserId = userId,
                        HourlyWage = userInfo.CurrentHourlyWage,
                        Status = (int)ShiftAssignmentStatus.Assigned,
                        IsOverridden = shouldMarkOverridden,
                        OverrideReason = shouldMarkOverridden ? request.OverrideReason : null,
                        OverriddenBy = shouldMarkOverridden ? request.OverriddenBy : null,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };

                    await _assignmentRepo.AddAsync(assignment);

                    outboxMessages.Add(CreateAssignedOutboxMessage(
                        assignment.Id, shift, userId, assignment.HourlyWage ?? 0));
                }
            }

            // Lưu outbox messages để gửi thông báo
            foreach (var message in outboxMessages)
            {
                _outboxRepository.Add(message);
            }

            // Đánh thức OutboxWorker xử lý bất đồng bộ
            if (outboxMessages.Any())
            {
                _outboxSignal.Pulse();
            }

            // Khi ca vừa đủ người sau gán tay: chuyển Shift về Published và tự động từ chối các đơn Pending còn lại
            if (finalCount >= shift.RequiredStaffCount && toAdd.Count > 0)
            {
                shift.Status = (int)ShiftStatus.Published;
                shift.UpdatedAt = DateTimeOffset.UtcNow;
                await _shiftGenericRepo.UpdateAsync(shift);
                await _autoRejectService.RejectPendingApplicationsAsync(
                    shift.Id, excludeApplicationId: null, request.OverriddenBy ?? Guid.Empty, cancellationToken);
            }

            return new AssignStaffResult { Success = true };
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

        /// <summary>
        /// Kiểm tra trạng thái ca làm việc trước khi cho phép phân công
        /// </summary>
        private static void ValidateShiftStatus(Shift shift)
        {
            if (shift.Status == (int)ShiftStatus.Completed)
                throw new InvalidOperationException("Khong the gan nhan vien vao ca da hoan thanh.");
            if (shift.Status == (int)ShiftStatus.Cancelled)
                throw new InvalidOperationException("Khong the gan nhan vien vao ca da huy.");
        }
    }
}
