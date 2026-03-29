using FluentAssertions;
using Moq;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Services;

public class ShiftValidationServiceTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly ShiftValidationService _service;

    public ShiftValidationServiceTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _service = new ShiftValidationService(_shiftRepositoryMock.Object);
    }

    [Fact]
    public async Task ValidateAssignment_ShouldReturnAllWarnings_WhenShiftViolatesMultipleRules()
    {
        var userId = Guid.NewGuid();
        var newShift = CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(13, 0), new TimeOnly(21, 0), 30, "Ca pha Arabica buoi chieu");

        var overlapShift = CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(10, 0), new TimeOnly(14, 0), 0, "Ca Robusta overlap");
        var overlapAssignment = CreateAssignment(overlapShift, userId);

        var restShift = CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(11, 0), new TimeOnly(12, 0), 0, "Ca chuan bi sua dac");
        var restAssignment = CreateAssignment(restShift, userId);

        var weeklyAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 23), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca Thu Hai Arabica"), userId),
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca Thu Ba Robusta"), userId),
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 25), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca Thu Tu Latte"), userId),
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 26), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca Thu Nam Mocha"), userId),
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 27), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca Thu Sau Cold Brew"), userId)
        };

        _shiftRepositoryMock
            .SetupSequence(r => r.GetAssignmentsByUserAsync(
                userId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overlapAssignment })
            .ReturnsAsync(new[] { restAssignment })
            .ReturnsAsync(weeklyAssignments);

        var result = await _service.ValidateAssignment(newShift, userId, minHoursBreak: 2, maxHoursPerWeek: 40, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain(w => w.Code == "OVERLAP" && w.Severity == "BLOCKING");
        result.Should().Contain(w => w.Code == "REST_TIME" && w.Severity == "WARNING");
        result.Should().Contain(w => w.Code == "MAX_HOURS_WEEK" && w.Severity == "WARNING");
    }

    [Fact]
    public async Task ValidateAssignment_ShouldReturnNoWarning_WhenShiftIsSafe()
    {
        var userId = Guid.NewGuid();
        var newShift = CreateShift(new DateOnly(2026, 3, 28), new TimeOnly(8, 0), new TimeOnly(12, 0), 0, "Ca pha sua tuoi");

        var weeklyAssignments = new List<ShiftAssignment>
        {
            CreateAssignment(CreateShift(new DateOnly(2026, 3, 27), new TimeOnly(14, 0), new TimeOnly(18, 0), 0, "Ca latte cuoi tuan"), userId)
        };

        _shiftRepositoryMock
            .SetupSequence(r => r.GetAssignmentsByUserAsync(
                userId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>())
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>())
            .ReturnsAsync(weeklyAssignments);

        var result = await _service.ValidateAssignment(newShift, userId, minHoursBreak: 2, maxHoursPerWeek: 40, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCandidate_ShouldFlagConflictRestAndOverLimit_WhenCandidateIsNotSuitable()
    {
        var userId = Guid.NewGuid();
        var newShift = CreateShift(new DateOnly(2026, 3, 29), new TimeOnly(14, 0), new TimeOnly(22, 0), 30, "Ca toi cappuccino");

        var overlapAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 3, 29), new TimeOnly(13, 0), new TimeOnly(17, 0), 0, "Ca overlap pha espresso"),
            userId);

        var closeGapAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 3, 29), new TimeOnly(6, 0), new TimeOnly(13, 0), 0, "Ca sang pha sua dac"),
            userId);

        var extraHoursAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 3, 27), new TimeOnly(8, 0), new TimeOnly(16, 0), 0, "Ca chieu cold brew"),
            userId);

        var candidateAssignments = new List<ShiftAssignment>
        {
            overlapAssignment,
            closeGapAssignment,
            extraHoursAssignment
        };

        var result = _service.ValidateCandidate(newShift, candidateAssignments, minHoursBreak: 2, maxHoursPerWeek: 12);

        result.IsConflict.Should().BeTrue();
        result.IsRestTimeInvalid.Should().BeTrue();
        result.IsOverLimit.Should().BeTrue();
        result.RestHours.Should().Be(1);
        result.WeeklyHours.Should().BeGreaterThan(12);
    }

    [Fact]
    public void ValidateCandidate_ShouldReturnSafeDefaults_WhenNoAssignmentExists()
    {
        var newShift = CreateShift(new DateOnly(2026, 3, 30), new TimeOnly(6, 0), new TimeOnly(14, 0), 30, "Ca rang xay buoi sang");

        var result = _service.ValidateCandidate(newShift, new List<ShiftAssignment>(), minHoursBreak: 8, maxHoursPerWeek: 48);

        result.IsConflict.Should().BeFalse();
        result.IsRestTimeInvalid.Should().BeFalse();
        result.IsOverLimit.Should().BeFalse();
        result.RestHours.Should().Be(double.MaxValue);
        result.WeeklyHours.Should().Be(0);
    }

    [Fact]
    public void ValidateCandidate_ShouldHandleOvernightShifts_WhenCalculatingRest()
    {
        var userId = Guid.NewGuid();
        var newShift = CreateShift(new DateOnly(2026, 3, 31), new TimeOnly(22, 0), new TimeOnly(6, 0), 60, "Ca dem Robusta");

        var sameShiftAssignment = CreateAssignment(newShift, userId);
        var overnightAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 3, 30), new TimeOnly(20, 0), new TimeOnly(4, 0), 30, "Ca dem Arabica qua nua dem"),
            userId);
        var futureAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 4, 1), new TimeOnly(10, 0), new TimeOnly(14, 0), 0, "Ca sach may sua"),
            userId);

        var result = _service.ValidateCandidate(
            newShift,
            new List<ShiftAssignment> { sameShiftAssignment, overnightAssignment, futureAssignment },
            minHoursBreak: 5,
            maxHoursPerWeek: 60);

        result.IsConflict.Should().BeFalse();
        result.IsRestTimeInvalid.Should().BeTrue();
        result.RestHours.Should().Be(4);
        result.WeeklyHours.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ValidateCandidate_ShouldUseGapFromUpcomingShift_WhenExistingStartsAfterNewShift()
    {
        var userId = Guid.NewGuid();
        var newShift = CreateShift(new DateOnly(2026, 4, 2), new TimeOnly(6, 0), new TimeOnly(12, 0), 30, "Ca pha phin buoi sang");

        var upcomingAssignment = CreateAssignment(
            CreateShift(new DateOnly(2026, 4, 2), new TimeOnly(14, 0), new TimeOnly(18, 0), 0, "Ca chieu sua tuoi"),
            userId);

        var result = _service.ValidateCandidate(
            newShift,
            new List<ShiftAssignment> { upcomingAssignment },
            minHoursBreak: 3,
            maxHoursPerWeek: 40);

        result.IsConflict.Should().BeFalse();
        result.IsRestTimeInvalid.Should().BeTrue();
        result.RestHours.Should().Be(2);
    }

    private static Shift CreateShift(DateOnly date, TimeOnly start, TimeOnly end, int breakMinutes, string note)
    {
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CHB-ROBUSTA-01",
            Name = "Cua hang Robusta Sua Da",
            Address = "456 Le Loi",
            Shifts = new List<Shift>(),
            StoreHours = new List<StoreHour>()
        };

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ShiftDate = date,
            StartTime = start,
            EndTime = end,
            BreakDurationMinutes = breakMinutes,
            RequiredStaffCount = 2,
            Note = note,
            ShiftAssignments = new List<ShiftAssignment>(),
            Store = store
        };

        store.Shifts.Add(shift);
        return shift;
    }

    private static ShiftAssignment CreateAssignment(Shift shift, Guid userId)
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        shift.ShiftAssignments.Add(assignment);
        return assignment;
    }
}
