using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetAvailableStaffs
{
    // Handler lay danh sach nhan vien kha dung cho mot ca lam viec
    // Moi nhan vien duoc kem theo trang thai validation (trung lich, nghi ngoi, gioi han gio)
    // De FE hien canh bao truoc khi Manager chon gan
    public class GetAvailableStaffsHandler : IRequestHandler<GetAvailableStaffsQuery, List<StaffAssignmentCandidateDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IGenericRepository<Shift> _shiftGenericRepo;
        private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
        private readonly IGenericRepository<ShiftRule> _ruleRepo;
        private readonly IGenericRepository<StaffAvailability> _availabilityRepo;
        private readonly IIdentityServiceClient _identityClient;

        public GetAvailableStaffsHandler(
            IShiftRepository shiftRepository,
            IGenericRepository<Shift> shiftGenericRepo,
            IGenericRepository<ShiftAssignment> assignmentRepo,
            IGenericRepository<ShiftRule> ruleRepo,
            IGenericRepository<StaffAvailability> availabilityRepo,
            IIdentityServiceClient identityClient)
        {
            _shiftRepository = shiftRepository;
            _shiftGenericRepo = shiftGenericRepo;
            _assignmentRepo = assignmentRepo;
            _ruleRepo = ruleRepo;
            _availabilityRepo = availabilityRepo;
            _identityClient = identityClient;
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

                // A. Check Availability
                if (availabilityMap.TryGetValue(user.Id, out var userAvails))
                {
                    var overlapping = userAvails.Where(a => IsOverlap(shift.StartTime, shift.EndTime, a.StartTime, a.EndTime)).ToList();
                    var busyRule = overlapping.FirstOrDefault(a => !a.IsAvailable);
                    if (busyRule != null) { isAvailable = false; note = busyRule.Note; }
                    else if (overlapping.Any()) { note = overlapping.First()?.Note; }
                }

                // B. Check Overlap & Rest Time & Weekly Hours
                var userAssignments = assignmentMap.GetValueOrDefault(user.Id) ?? [];

                bool isConflict = userAssignments.Any(a => a.Shift.ShiftDate == shift.ShiftDate && a.ShiftId != shift.Id
                                    && IsOverlap(shift.StartTime, shift.EndTime, a.Shift.StartTime, a.Shift.EndTime));

                var restResult = CalculateMinRestHours(shift, userAssignments.Where(a => a.ShiftId != shift.Id).ToList());
                validation.IsRestTimeInvalid = restResult.minGap < minHoursBreak;
                validation.RestHours = restResult.minGap;

                double weeklyHours = CalculateWeeklyHours(shift, userAssignments);
                double newShiftHours = CalculateNetHours(shift.StartTime, shift.EndTime, shift.BreakDurationMinutes);
                validation.IsOverLimit = (weeklyHours + newShiftHours) > maxHoursPerWeek;
                validation.WeeklyHours = weeklyHours;

                // C. Check Shuttle (2 đầu)
                if (!isInternal)
                {
                    var homeStoreRule = allRules.FirstOrDefault(r => r.StoreId == user.HomeStoreId) ?? globalRule;
                    bool homeStoreAllowsOut = homeStoreRule?.AllowShuttle ?? false;
                    validation.IsShuttleInvalid = !currentStoreAllowsIn || !homeStoreAllowsOut;
                }

                // D. Lọc kết quả
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

        // === PRIVATE METHODS ===

        // Kiem tra 2 khoang thoi gian co trung nhau khong
        // Ho tro ca qua dem (StartTime > EndTime)
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

        // Tinh khoang nghi nho nhat giua ca moi va cac ca hien tai
        // Tra ve khoang nghi nho nhat (tinh bang gio)
        private static (double minGap, bool hasNearby) CalculateMinRestHours(
            Shift newShift, List<ShiftAssignment> existingAssignments)
        {
            if (!existingAssignments.Any())
                return (double.MaxValue, false);

            var newStart = newShift.ShiftDate.ToDateTime(newShift.StartTime);
            var newEnd = newShift.ShiftDate.ToDateTime(newShift.EndTime);
            if (newShift.EndTime <= newShift.StartTime)
                newEnd = newEnd.AddDays(1);

            double minGap = double.MaxValue;

            foreach (var existing in existingAssignments)
            {
                var existStart = existing.Shift.ShiftDate.ToDateTime(existing.Shift.StartTime);
                var existEnd = existing.Shift.ShiftDate.ToDateTime(existing.Shift.EndTime);
                if (existing.Shift.EndTime <= existing.Shift.StartTime)
                    existEnd = existEnd.AddDays(1);

                double gap;
                if (newStart >= existEnd)
                    gap = (newStart - existEnd).TotalHours;
                else if (existStart >= newEnd)
                    gap = (existStart - newEnd).TotalHours;
                else
                    continue; // Overlap -> khong can tinh gap

                if (gap < minGap)
                    minGap = gap;
            }

            return (minGap, true);
        }

        // Tinh tong gio lam trong tuan cua nhan vien (tru break)
        // Tuan tinh tu Thu Hai -> Chu Nhat
        // REFACTORED: Use In-Memory List
        private static double CalculateWeeklyHours(Shift shift, List<ShiftAssignment> weeklyAssignments)
        {
            // Loc bo assignment cua chinh ca hien tai (tranh dem trung)
            var filteredAssignments = weeklyAssignments
                .Where(a => a.ShiftId != shift.Id);

            return filteredAssignments.Sum(a =>
                CalculateNetHours(a.Shift.StartTime, a.Shift.EndTime, a.Shift.BreakDurationMinutes));
        }

        // Tinh so gio lam thuc te (da tru thoi gian nghi)
        private static double CalculateNetHours(TimeOnly start, TimeOnly end, int breakMinutes)
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

            return Math.Max(0, grossHours - (breakMinutes / 60.0));
        }
    }
}