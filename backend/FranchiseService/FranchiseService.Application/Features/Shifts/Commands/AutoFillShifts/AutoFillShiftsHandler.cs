using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.AutoFillShifts;

/// <summary>
/// Thuat toan Greedy tu dong xep ca.
///
/// Quy trinh:
///   1. Batch fetch toan bo du lieu can thiet (5 queries)
///   2. Sap xep ca theo ngay va gio (som truoc, muon sau)
///   3. Voi moi ca: loc ung vien hop le, sap xep theo cong bang va tiet kiem, gan top N
///   4. Sau moi lan gan: tao Virtual Assignment cap nhat trang thai in-memory
///   5. Tra ve ket qua: so slot da dien, danh sach ca chua du nguoi
///
/// Dac diem:
///   - Chi 5 DB queries cho toan bo qua trinh
///   - Idempotent: chay lai khong lam hong du lieu cu
///   - Tai su dung ValidateCandidate (in-memory, 0 query them)
/// </summary>
public class AutoFillShiftsHandler : IRequestHandler<AutoFillShiftsCommand, AutoFillResult>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IGenericRepository<ShiftAssignment> _assignmentRepo;
    private readonly IGenericRepository<StaffAvailability> _availabilityRepo;
    private readonly IIdentityServiceClient _identityClient;
    private readonly IShiftRuleService _shiftRuleService;
    private readonly IShiftValidationService _validationService;

    public AutoFillShiftsHandler(
        IShiftRepository shiftRepository,
        IGenericRepository<ShiftAssignment> assignmentRepo,
        IGenericRepository<StaffAvailability> availabilityRepo,
        IIdentityServiceClient identityClient,
        IShiftRuleService shiftRuleService,
        IShiftValidationService validationService)
    {
        _shiftRepository = shiftRepository;
        _assignmentRepo = assignmentRepo;
        _availabilityRepo = availabilityRepo;
        _identityClient = identityClient;
        _shiftRuleService = shiftRuleService;
        _validationService = validationService;
    }

    public async Task<AutoFillResult> Handle(
        AutoFillShiftsCommand request, CancellationToken cancellationToken)
    {
        // ================================================================
        // Buoc 1: Batch fetch du lieu (5 queries duy nhat)
        // ================================================================

        // Lay tat ca ca Draft cua Store, kem danh sach phan cong hien tai
        var allShifts = (await _shiftRepository.GetShiftsWithAssignmentsAsync(
            request.StoreId, request.StartDate, request.EndDate,
            (int)ShiftStatus.Draft, cancellationToken)).ToList();

        if (!allShifts.Any())
            return new AutoFillResult();

        // Lay danh sach nhan vien thuoc cua hang nay
        var allStaff = await _identityClient.GetAllUsersAsync("staff");
        var staffInStore = allStaff
            .Where(u => u.HomeStoreId == request.StoreId)
            .ToList();

        if (!staffInStore.Any())
            return BuildEmptyResult(allShifts);

        // Lay quy tac xep ca (Store rieng hoac Global mac dinh)
        var storeRule = await _shiftRuleService.GetEffectiveRuleAsync(request.StoreId);
        int minHoursBreak = storeRule?.MinHoursBetweenShifts ?? 8;
        int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;

        // Lay lich ranh/ban cua tat ca nhan vien
        var userIds = staffInStore.Select(u => u.Id).ToList();
        var allAvailabilities = (await _availabilityRepo.GetAllAsync(
            predicate: a => userIds.Contains(a.UserId)
                && ((a.IsRecurring == false
                     && a.SpecificDate >= request.StartDate
                     && a.SpecificDate <= request.EndDate)
                    || a.IsRecurring == true)
        )).ToList();

        var availsByUser = allAvailabilities
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Lay phan cong hien tai (mo rong 1 ngay truoc/sau de check nghi giua 2 ca)
        var allAssignments = (await _shiftRepository.GetAssignmentsByUsersAsync(
            userIds,
            request.StartDate.AddDays(-1),
            request.EndDate.AddDays(1),
            cancellationToken)).ToList();

        var assignsByUser = allAssignments
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ================================================================
        // Buoc 2: Loc ca can dien (Draft, chua du nguoi)
        // ================================================================
        var shiftsToFill = new List<(Shift Shift, int SlotsNeeded)>();

        foreach (var shift in allShifts)
        {
            int currentCount = shift.ShiftAssignments?.Count(a => !a.IsDeleted) ?? 0;
            int needed = shift.RequiredStaffCount - currentCount;
            if (needed > 0)
                shiftsToFill.Add((shift, needed));
        }

        if (!shiftsToFill.Any())
            return new AutoFillResult();

        // ================================================================
        // Buoc 3: Sap xep ca theo ngay va gio bat dau
        // Ca som xep truoc de rest time check dung cho ca sau
        // ================================================================
        shiftsToFill.Sort((a, b) =>
        {
            int dateCompare = a.Shift.ShiftDate.CompareTo(b.Shift.ShiftDate);
            return dateCompare != 0
                ? dateCompare
                : a.Shift.StartTime.CompareTo(b.Shift.StartTime);
        });

        // ================================================================
        // Buoc 4: Greedy - Xep tung ca mot
        // ================================================================
        var result = new AutoFillResult
        {
            TotalSlots = shiftsToFill.Sum(x => x.SlotsNeeded)
        };

        foreach (var (shift, slotsNeeded) in shiftsToFill)
        {
            int assignedCount = 0;

            // Lay danh sach nhan vien da gan ca nay (bao gom Virtual Assignment)
            var alreadyAssignedIds = GetAlreadyAssignedIds(shift.Id, assignsByUser);

            // Xac dinh khoang tuan (Thu Hai den Chu Nhat) de tinh gio lam dung
            var weekStart = ShiftCalculationHelper.GetMondayOfWeek(shift.ShiftDate);
            var weekEnd = weekStart.AddDays(6);

            // Loc va cham diem ung vien
            var candidates = new List<(UserInfo Staff, double WeeklyHours, decimal Wage)>();

            foreach (var staff in staffInStore)
            {
                // Bo qua neu da duoc gan ca nay roi
                if (alreadyAssignedIds.Contains(staff.Id)) continue;

                // Bo qua neu nhan vien bao ban trung gio ca
                if (ShiftCalculationHelper.IsStaffBusy(shift, staff.Id, availsByUser))
                    continue;

                // Loc phan cong theo dung tuan chua ca nay
                var weekAssigns = ShiftCalculationHelper.GetAssignmentsInWeek(
                    staff.Id, weekStart, weekEnd, assignsByUser);

                // Validate in-memory: trung ca, nghi chua du, vuot gio tuan
                var validation = _validationService.ValidateCandidate(
                    shift, weekAssigns, minHoursBreak, maxHoursPerWeek);

                // Auto-Fill khong cho Override, bat ky vi pham nao deu bo qua
                if (validation.IsConflict || validation.IsRestTimeInvalid
                    || validation.IsOverLimit)
                    continue;

                candidates.Add((staff, validation.WeeklyHours,
                    staff.CurrentHourlyWage ?? 0));
            }

            // Sap xep ung vien: nguoi it gio nhat truoc (cong bang), cung gio thi luong thap truoc (tiet kiem)
            candidates.Sort((a, b) =>
            {
                int hoursCompare = a.WeeklyHours.CompareTo(b.WeeklyHours);
                return hoursCompare != 0
                    ? hoursCompare
                    : a.Wage.CompareTo(b.Wage);
            });

            // Gan top N ung vien vao ca
            for (int i = 0; i < slotsNeeded && i < candidates.Count; i++)
            {
                var staff = candidates[i].Staff;

                // Tao phan cong that va luu vao DB
                var newAssignment = new ShiftAssignment
                {
                    Id = Guid.NewGuid(),
                    ShiftId = shift.Id,
                    UserId = staff.Id,
                    Status = (int)ShiftAssignmentStatus.Assigned,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await _assignmentRepo.AddAsync(newAssignment);

                // Tao Virtual Assignment de cap nhat trang thai in-memory.
                // Phai set Shift (navigation property) de ValidateCandidate
                // doc duoc ShiftDate, StartTime, EndTime, BreakDurationMinutes.
                var virtualAssign = new ShiftAssignment
                {
                    ShiftId = shift.Id,
                    UserId = staff.Id,
                    Shift = shift
                };

                if (!assignsByUser.ContainsKey(staff.Id))
                    assignsByUser[staff.Id] = new List<ShiftAssignment>();
                assignsByUser[staff.Id].Add(virtualAssign);

                assignedCount++;
            }

            result.FilledCount += assignedCount;

            // Khong du nguoi cho ca nay, them vao danh sach can xu ly thu cong
            if (assignedCount < slotsNeeded)
                result.UnfilledShiftIds.Add(shift.Id);
        }

        return result;
    }

    /// <summary>
    /// Lay danh sach ID nhan vien da duoc gan vao ca (bao gom Virtual Assignment).
    /// </summary>
    private static HashSet<Guid> GetAlreadyAssignedIds(
        Guid shiftId,
        Dictionary<Guid, List<ShiftAssignment>> assignsByUser)
    {
        return assignsByUser
            .Where(kvp => kvp.Value.Any(a => a.ShiftId == shiftId))
            .Select(kvp => kvp.Key)
            .ToHashSet();
    }

    /// <summary>
    /// Tao ket qua rong khi cua hang khong co nhan vien nao.
    /// Tra ve toan bo ca la unfilled de Manager biet.
    /// </summary>
    private static AutoFillResult BuildEmptyResult(List<Shift> shifts)
    {
        var result = new AutoFillResult();
        foreach (var shift in shifts)
        {
            int currentCount = shift.ShiftAssignments?.Count(a => !a.IsDeleted) ?? 0;
            int needed = shift.RequiredStaffCount - currentCount;
            if (needed > 0)
            {
                result.TotalSlots += needed;
                result.UnfilledShiftIds.Add(shift.Id);
            }
        }
        return result;
    }
}
