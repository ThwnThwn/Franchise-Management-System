using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetAvailableStaffs
{
    /// <summary>
    /// Lấy danh sách nhân viên khả dụng cho ca làm việc.
    /// Mỗi nhân viên kèm trạng thái validation để FE hiện cảnh báo.
    /// </summary>
    public class GetAvailableStaffsHandler : IRequestHandler<GetAvailableStaffsQuery, List<StaffAssignmentCandidateDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IGenericRepository<Shift> _shiftGenericRepo;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
        private readonly IGenericRepository<ShiftRule> _ruleRepo;
        private readonly IGenericRepository<StaffAvailability> _availabilityRepo;
        private readonly IIdentityServiceClient _identityClient;
        private readonly IShiftValidationService _validationService;

        public GetAvailableStaffsHandler(
            IShiftRepository shiftRepository,
            IGenericRepository<Shift> shiftGenericRepo,
            IGenericRepository<ShiftAssignment> assignmentRepo,
            IGenericRepository<ShiftRule> ruleRepo,
            IGenericRepository<StaffAvailability> availabilityRepo,
            IIdentityServiceClient identityClient,
            IShiftValidationService validationService)
        {
            _shiftRepository = shiftRepository;
            _shiftGenericRepo = shiftGenericRepo;
            _assignmentRepo = assignmentRepo;
            _ruleRepo = ruleRepo;
            _availabilityRepo = availabilityRepo;
            _identityClient = identityClient;
            _validationService = validationService;
        }

        public async Task<List<StaffAssignmentCandidateDto>> Handle(
    GetAvailableStaffsQuery request, CancellationToken cancellationToken)
        {
            // 1. Load thông tin ca làm việc
            var shift = await _shiftGenericRepo.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Khong tim thay Ca lam viec {request.ShiftId}");

            // 2. Load tất cả Rules (Dùng cách này để check 2 đầu Shuttle mà không tốn nhiều query)
            var allRules = (await _ruleRepo.GetAllAsync()).ToList();
            var globalRule = allRules.FirstOrDefault(r => r.StoreId == null);
            var currentStoreRule = allRules.FirstOrDefault(r => r.StoreId == request.StoreId) ?? globalRule;

            int maxHoursPerWeek = currentStoreRule?.MaxHoursPerWeek ?? 48;
            int minHoursBreak = currentStoreRule?.MinHoursBetweenShifts ?? 8;
            bool currentStoreAllowsIn = currentStoreRule?.AllowShuttle ?? false;

            // 3. Lấy danh sách nhân viên
            var allUsers = await _identityClient.GetAllUsersAsync("staff");

            // 4. Chuẩn bị dữ liệu để tính Weekly Hours và Overlap (Batch Fetching)
            int daysSinceMonday = (int)shift.ShiftDate.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;
            var startOfWeek = shift.ShiftDate.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);

            var assignmentMap = (await _shiftRepository.GetAssignmentsByUsersAsync(
                    allUsers.Select(u => u.Id).ToList(),
                    startOfWeek.AddDays(-1), endOfWeek.AddDays(1), cancellationToken))
                .GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var availabilityMap = (await _availabilityRepo.GetAllAsync(
                    predicate: a => (a.IsRecurring == false && a.SpecificDate == shift.ShiftDate) ||
                                    (a.IsRecurring == true && (short)a.DayOfWeek == (short)shift.ShiftDate.DayOfWeek))
                ).GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var assignedUserIds = (await _assignmentRepo.GetAllAsync(predicate: a => a.ShiftId == request.ShiftId))
                .Select(a => a.UserId).ToHashSet();

            // 5. Xử lý logic từng nhân viên
            var result = new List<StaffAssignmentCandidateDto>();

            foreach (var user in allUsers)
            {
                bool isInternal = user.HomeStoreId == request.StoreId;
                var validation = new CandidateValidationDto();
                bool isAvailable = true;
                string? note = null;

                // Kiểm tra lịch khả dụng cá nhân của nhân viên
                if (availabilityMap.TryGetValue(user.Id, out var userAvails))
                {
                    var overlapping = userAvails.Where(a => IShiftValidationService.IsOverlap(
                        shift.StartTime, shift.EndTime, a.StartTime, a.EndTime)).ToList();
                    var busyRule = overlapping.FirstOrDefault(a => !a.IsAvailable);
                    if (busyRule != null) { isAvailable = false; note = busyRule.Note; }
                    else if (overlapping.Any()) { note = overlapping.First()?.Note; }
                }

                // Validate trùng giờ, nghỉ tối thiểu, giới hạn giờ bằng Service
                var userAssignments = assignmentMap.GetValueOrDefault(user.Id) ?? [];
                var candidateResult = _validationService.ValidateCandidate(
                    shift, userAssignments, minHoursBreak, maxHoursPerWeek);

                bool isConflict = candidateResult.IsConflict;
                validation.IsRestTimeInvalid = candidateResult.IsRestTimeInvalid;
                validation.RestHours = candidateResult.RestHours;
                validation.IsOverLimit = candidateResult.IsOverLimit;
                validation.WeeklyHours = candidateResult.WeeklyHours;

                // Kiểm tra quyền Shuttle giữa 2 cửa hàng
                if (!isInternal)
                {
                    var homeStoreRule = allRules.FirstOrDefault(r => r.StoreId == user.HomeStoreId) ?? globalRule;
                    bool homeStoreAllowsOut = homeStoreRule?.AllowShuttle ?? false;
                    validation.IsShuttleInvalid = !currentStoreAllowsIn || !homeStoreAllowsOut;
                }

                // Lọc kết quả: nhân viên nội bộ luôn hiện, nhân viên ngoài chỉ hiện khi hợp lệ
                bool isValidGlobal = isAvailable && !isConflict && !validation.IsRestTimeInvalid && !validation.IsOverLimit && !validation.IsShuttleInvalid;

                if (isInternal || isValidGlobal)
                {
                    result.Add(new StaffAssignmentCandidateDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AvatarUrl = user.AvatarUrl,
                        CurrentHourlyWage = user.CurrentHourlyWage ?? 0,
                        IsAssigned = assignedUserIds.Contains(user.Id),
                        IsAvailable = isAvailable,
                        IsConflict = isConflict,
                        Note = note,
                        Validation = validation
                    });
                }
            }

            return result;
        }
    }
}