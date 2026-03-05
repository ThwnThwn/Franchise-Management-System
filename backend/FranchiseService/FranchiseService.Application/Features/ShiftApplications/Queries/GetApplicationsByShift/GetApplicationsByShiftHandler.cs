using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftApplications.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.ShiftApplications.Queries.GetApplicationsByShift;

/// <summary>
/// Handler lay danh sach ung vien da dang ky 1 ca, kem Smart Score.
/// Smart Score = 0.5 * CostScore + 0.5 * FairnessScore
///   - CostScore: luong thap hon max => diem cao (tiet kiem chi phi)
///   - FairnessScore: gio tuan it hon max => diem cao (cong bang)
/// Dung batch fetch de tranh N+1 query.
/// </summary>
public class GetApplicationsByShiftHandler
    : IRequestHandler<GetApplicationsByShiftQuery, List<ShiftApplicationDto>>
{
    private readonly IGenericRepository<ShiftApplication> _applicationRepository;
    private readonly IGenericRepository<Shift> _shiftRepository;
    private readonly IIdentityServiceClient _identityClient;
    private readonly IShiftRepository _shiftRepo;
    private readonly IShiftRuleService _shiftRuleService;

    public GetApplicationsByShiftHandler(
        IGenericRepository<ShiftApplication> applicationRepository,
        IGenericRepository<Shift> shiftRepository,
        IIdentityServiceClient identityClient,
        IShiftRepository shiftRepo,
        IShiftRuleService shiftRuleService)
    {
        _applicationRepository = applicationRepository;
        _shiftRepository = shiftRepository;
        _identityClient = identityClient;
        _shiftRepo = shiftRepo;
        _shiftRuleService = shiftRuleService;
    }

    public async Task<List<ShiftApplicationDto>> Handle(
        GetApplicationsByShiftQuery request, CancellationToken cancellationToken)
    {
        // 1. Lay tat ca don Pending cua ca nay
        var applications = await _applicationRepository.GetAllAsync(
            predicate: a => a.ShiftId == request.ShiftId
                         && a.Status == (int)ShiftApplicationStatus.Pending);

        var appList = applications.ToList();
        if (!appList.Any()) return new List<ShiftApplicationDto>();

        // 2. Batch fetch thong tin nhan vien tu Identity Service (1 lan duy nhat)
        var userIds = appList.Select(a => a.UserId).Distinct().ToList();
        var users = await _identityClient.GetUsersByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);

        // 3. Lay thong tin Shift va ShiftRule de tinh Smart Score
        var shift = await _shiftRepository.GetByIdAsync(request.ShiftId);
        var storeRule = shift != null
            ? await _shiftRuleService.GetEffectiveRuleAsync(shift.StoreId)
            : null;
        int maxHoursPerWeek = storeRule?.MaxHoursPerWeek ?? 48;

        // 4. Xac dinh khoang tuan cua ca (Mon-Sun)
        DateOnly shiftDate = shift?.ShiftDate ?? DateOnly.FromDateTime(DateTime.Today);
        var startOfWeek = ShiftCalculationHelper.GetMondayOfWeek(shiftDate);
        var endOfWeek = startOfWeek.AddDays(6);

        // 5. Batch fetch: lay assignments cua TAT CA ung vien trong 1 query duy nhat
        var allWeeklyAssignments = await _shiftRepo
            .GetAssignmentsByUsersAsync(userIds, startOfWeek, endOfWeek, cancellationToken);

        // Group theo UserId de tra cuu nhanh trong vong lap
        var assignmentsByUser = allWeeklyAssignments
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 6. Lay max wage trong nhom ung vien de chuan hoa CostScore
        decimal maxWage = users
            .Where(u => u.CurrentHourlyWage.HasValue && u.CurrentHourlyWage.Value > 0)
            .Select(u => u.CurrentHourlyWage!.Value)
            .DefaultIfEmpty(1)
            .Max();

        // 7. Tinh Smart Score cho tung ung vien
        var dtos = new List<ShiftApplicationDto>();

        foreach (var app in appList)
        {
            userMap.TryGetValue(app.UserId, out var userInfo);
            decimal hourlyWage = userInfo?.CurrentHourlyWage ?? 0;

            // Tinh so gio da lam trong tuan
            assignmentsByUser.TryGetValue(app.UserId, out var userAssignments);
            double weeklyHours = userAssignments?.Sum(a =>
                ShiftCalculationHelper.CalculateNetHours(
                    a.Shift.StartTime, a.Shift.EndTime, a.Shift.BreakDurationMinutes)) ?? 0;

            // CostScore: luong thap => diem cao (tiet kiem chi phi cho cua hang)
            double costScore = maxWage > 0
                ? 0.5 * (1.0 - (double)(hourlyWage / maxWage))
                : 0;

            // FairnessScore: gio tuan it => diem cao (chia deu gio cho nhan vien)
            double fairnessScore = maxHoursPerWeek > 0
                ? 0.5 * Math.Max(0, (maxHoursPerWeek - weeklyHours) / maxHoursPerWeek)
                : 0;

            double smartScore = Math.Round(costScore + fairnessScore, 2);

            // Ghep ten day du tu FirstName + LastName
            string? fullName = userInfo != null
                ? $"{userInfo.FirstName} {userInfo.LastName}".Trim()
                : null;

            dtos.Add(new ShiftApplicationDto
            {
                Id = app.Id,
                ShiftId = app.ShiftId,
                UserId = app.UserId,
                Status = app.Status,
                Note = app.Note,
                AppliedAt = app.AppliedAt,
                UserName = fullName,
                UserEmail = userInfo?.Email,
                AvatarUrl = userInfo?.AvatarUrl,
                HourlyWage = hourlyWage > 0 ? hourlyWage : null,
                SmartScore = smartScore,
                WeeklyHoursWorked = Math.Round(weeklyHours, 1)
            });
        }

        // Sap xep theo Smart Score giam dan (ung vien tot nhat len truoc)
        return dtos.OrderByDescending(d => d.SmartScore).ToList();
    }
}
