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
            // Load thong tin ca lam viec
            var shift = await _shiftGenericRepo.GetByIdAsync(request.ShiftId)
                ?? throw new KeyNotFoundException($"Khong tim thay Ca lam viec {request.ShiftId}");

            // Load quy tac cua Store (neu co) 
            var rules = await _ruleRepo.GetAllAsync(predicate: r => r.StoreId == request.StoreId);
            var storeRule = rules.FirstOrDefault();

            int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;
            int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;
            bool allowShuttle = storeRule?.AllowShuttle ?? false;

            // Lay danh sach nhan vien tu IdentityService
            // Lay tat ca user co role "staff" (hoac theo role_slug phu hop)
            var allUsers = await _identityClient.GetAllUsersAsync("staff");

            // Load danh sach assignment hien tai cua ca nay
            var currentAssignments = (await _assignmentRepo.GetAllAsync(
                predicate: a => a.ShiftId == request.ShiftId)).ToList();
            var assignedUserIds = currentAssignments.Select(a => a.UserId).ToHashSet();

            // Load thong tin Availability cua tat ca user lien quan trong ngay do
            // Loc theo: (SpecificDate == shiftDate OR (IsRecurring == true AND DayOfWeek == shiftDayOfWeek))
            // Co lay du ca ngay, sau do check overlap gio sau
            var shiftDate = shift.ShiftDate;
            var dayOfWeek = (short)shift.ShiftDate.DayOfWeek;
            
            var availabilities = await _availabilityRepo.GetAllAsync(
                predicate: a => (a.IsRecurring == false && a.SpecificDate == shiftDate) || 
                                (a.IsRecurring == true && a.DayOfWeek == dayOfWeek)
            );
            
            // Group by User cho de query memory
            var availabilityMap = availabilities.GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.ToList());

            // --- BATCH FETCHING ASSIGNMENTS (Fix N+1) ---
            // 1. Calculate Date Range (Cover overlap, rest time, weekly hours)
            int daysSinceMonday = (int)shift.ShiftDate.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;

            // Tuan hien tai
            var startOfWeek = shift.ShiftDate.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);

            // Range can fetch: Tuan hien tai + 1 ngay truoc/sau de check Rest Time (neu ca o bien tuan)
            var fetchFromDate = startOfWeek.AddDays(-1);
            var fetchToDate = endOfWeek.AddDays(1);

            // 2. Fetch all assignments for all candidate users
            var candidateIds = allUsers.Select(u => u.Id).ToList();
            var allAssignmentsRequest = await _shiftRepository.GetAssignmentsByUsersAsync(
                candidateIds, fetchFromDate, fetchToDate, cancellationToken);
            
            var assignmentMap = allAssignmentsRequest.GroupBy(a => a.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());
            // ---------------------------------------------

            // Xu ly tung nhan vien, tinh toan validation
            var result = new List<StaffAssignmentCandidateDto>();

            foreach (var user in allUsers)
            {
                var candidate = new StaffAssignmentCandidateDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = user.AvatarUrl,
                    CurrentHourlyWage = user.CurrentHourlyWage ?? 0,
                    IsAssigned = assignedUserIds.Contains(user.Id),
                    IsAvailable = true, // Default true
                    Note = null
                };

                // Get user assignments from memory
                var userAssignments = assignmentMap.GetValueOrDefault(user.Id) ?? new List<ShiftAssignment>();

                // CHECK AVAILABILITY (Lich ranh/ban)
                if (availabilityMap.TryGetValue(user.Id, out var userAvails))
                {
                    // Tim nhung rule overlap voi gio lam viec cua ca
                    var overlappingRules = userAvails.Where(a => 
                        IsOverlap(shift.StartTime, shift.EndTime, a.StartTime, a.EndTime)).ToList();

                    if (overlappingRules.Any())
                    {
                        // Logic: Neu co bat ky rule nao la BUSY (IsAvailable=false) -> Candidate la Busy
                        var busyRule = overlappingRules.FirstOrDefault(a => !a.IsAvailable);
                        if (busyRule != null)
                        {
                            candidate.IsAvailable = false;
                            candidate.Note = busyRule.Note; // Lay note cua rule ban
                        }
                        else 
                        {
                            // Neu tat ca rules deu la Available=true (Dang ky ranh) -> Van la true
                            // Lay note cua rule dau tien (neu co)
                            candidate.Note = overlappingRules.First().Note;
                        }
                    }
                }

                // CHECK TRUNG GIO (OVERLAP) - Use Memory Data
                // Filter for current shift date
                var checkOverlapAssignments = userAssignments
                    .Where(a => a.Shift.ShiftDate == shiftDate)
                    .ToList();
                
                // Loc bo assignment cua chinh ca hien tai (tranh tu trung)
                var otherAssignments = checkOverlapAssignments
                    .Where(a => a.ShiftId != request.ShiftId)
                    .ToList();

                candidate.IsConflict = otherAssignments.Any(a =>
                    IsOverlap(shift.StartTime, shift.EndTime, a.Shift.StartTime, a.Shift.EndTime));

                // CHECK REST TIME va WEEKLY CAP 
                var validation = new CandidateValidationDto();

                // Check khoang nghi toi thieu
                // Rest Time check range: [Date - 1, Date + 1]
                var restCheckStart = shiftDate.AddDays(-1);
                var restCheckEnd = shiftDate.AddDays(1);
                
                var nearbyAssignments = userAssignments
                    .Where(a => a.Shift.ShiftDate >= restCheckStart 
                             && a.Shift.ShiftDate <= restCheckEnd
                             && a.ShiftId != request.ShiftId)
                    .ToList();

                var restResult = CalculateMinRestHours(shift, nearbyAssignments);
                validation.IsRestTimeInvalid = restResult.minGap < minHoursBreak;
                validation.RestHours = restResult.minGap;

                // Check gioi han gio tuan - Use Memory Data
                var weeklyAssignments = userAssignments
                    .Where(a => a.Shift.ShiftDate >= startOfWeek && a.Shift.ShiftDate <= endOfWeek)
                    .ToList();

                var weeklyResult = CalculateWeeklyHours(shift, weeklyAssignments);
                var newShiftHours = CalculateNetHours(shift.StartTime, shift.EndTime, shift.BreakDurationMinutes);
                validation.IsOverLimit = (weeklyResult + newShiftHours) > maxHoursPerWeek;
                validation.WeeklyHours = weeklyResult;

                // CHECK SHUTTLE (DIEU CHUYEN) 
                // Neu nhan vien thuoc Store khac va Store hien tai khong cho phep Shuttle
                validation.IsShuttleInvalid = user.HomeStoreId.HasValue
                    && user.HomeStoreId.Value != request.StoreId
                    && !allowShuttle;

                candidate.Validation = validation;
                result.Add(candidate);
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
