using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Services;

public class OvertimeCalculationServiceTests
{
    private readonly Mock<IGenericRepository<TimeLog>> _timeLogRepoMock;
    private readonly OvertimeCalculationService _service;

    public OvertimeCalculationServiceTests()
    {
        _timeLogRepoMock = new Mock<IGenericRepository<TimeLog>>();
        _service = new OvertimeCalculationService(_timeLogRepoMock.Object);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseDefaultThresholds_WhenRuleIsNull()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift(new DateOnly(2026, 3, 23), new TimeOnly(8, 0), new TimeOnly(17, 0), 60, "Ca pha Arabica buoi sang");
        var timeLog = CreateTimeLog(
            shift,
            userId,
            ToUtc(shift.ShiftDate, new TimeOnly(7, 30)),
            ToUtc(shift.ShiftDate, new TimeOnly(17, 0)));

        _timeLogRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<TimeLog>());

        var result = await _service.CalculateAsync(timeLog, shift, null, userId);

        result.RegularMinutes.Should().Be(480);
        result.DailyOtMinutes.Should().Be(0);
        result.WeeklyOtMinutes.Should().Be(0);
    }

    [Fact]
    public async Task CalculateAsync_ShouldClampTotalWorkedMinutesToZero_WhenBreakGreaterThanWorkedTime()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(9, 0), new TimeOnly(10, 0), 120, "Ca sua da gio ngan");
        var timeLog = CreateTimeLog(
            shift,
            userId,
            ToUtc(shift.ShiftDate, new TimeOnly(9, 30)),
            ToUtc(shift.ShiftDate, new TimeOnly(10, 0)));

        _timeLogRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<TimeLog>());

        var result = await _service.CalculateAsync(timeLog, shift, new ShiftRule(), userId);

        result.RegularMinutes.Should().Be(0);
        result.DailyOtMinutes.Should().Be(0);
        result.WeeklyOtMinutes.Should().Be(0);
    }

    [Fact]
    public async Task CalculateAsync_ShouldCalculateDailyAndWeeklyOvertime_WhenWeeklyThresholdExceeded()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift(new DateOnly(2026, 3, 25), new TimeOnly(8, 0), new TimeOnly(18, 0), 0, "Ca Robusta full day");
        var timeLog = CreateTimeLog(
            shift,
            userId,
            ToUtc(shift.ShiftDate, new TimeOnly(8, 0)),
            ToUtc(shift.ShiftDate, new TimeOnly(18, 0)));

        var mondayShift = CreateShift(new DateOnly(2026, 3, 23), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca pha espresso dau tuan");
        var mondayAssignment = CreateAssignment(mondayShift, userId);
        var weeklyLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = mondayAssignment.Id,
            ShiftAssignment = mondayAssignment,
            CheckInAt = ToUtc(mondayShift.ShiftDate, mondayShift.StartTime),
            CheckOutAt = ToUtc(mondayShift.ShiftDate, mondayShift.EndTime),
            Status = (int)TimeLogStatus.Valid,
            RegularMinutes = 2380
        };
        mondayAssignment.TimeLogs.Add(weeklyLog);

        _timeLogRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>()))
            .ReturnsAsync(new[] { weeklyLog });

        var rule = new ShiftRule
        {
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        var result = await _service.CalculateAsync(timeLog, shift, rule, userId);

        result.RegularMinutes.Should().Be(20);
        result.DailyOtMinutes.Should().Be(120);
        result.WeeklyOtMinutes.Should().Be(460);
    }

    [Fact]
    public async Task CalculateAsync_ShouldMoveAllRegularMinutesToWeeklyOvertime_WhenWeeklyOverageIsHigherThanRegular()
    {
        var userId = Guid.NewGuid();
        var shift = CreateShift(new DateOnly(2026, 3, 26), new TimeOnly(8, 0), new TimeOnly(16, 0), 0, "Ca pha sua dac");
        var timeLog = CreateTimeLog(
            shift,
            userId,
            ToUtc(shift.ShiftDate, new TimeOnly(8, 0)),
            ToUtc(shift.ShiftDate, new TimeOnly(16, 0)));

        var weeklyLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = Guid.NewGuid(),
            ShiftAssignment = CreateAssignment(CreateShift(new DateOnly(2026, 3, 24), new TimeOnly(6, 0), new TimeOnly(14, 0), 0, "Ca rang xay"), userId),
            CheckInAt = ToUtc(new DateOnly(2026, 3, 24), new TimeOnly(6, 0)),
            CheckOutAt = ToUtc(new DateOnly(2026, 3, 24), new TimeOnly(14, 0)),
            Status = (int)TimeLogStatus.Valid,
            RegularMinutes = 2500
        };

        _timeLogRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>()))
            .ReturnsAsync(new[] { weeklyLog });

        var rule = new ShiftRule
        {
            DailyOtThresholdHours = 8,
            WeeklyOtThresholdHours = 40
        };

        var result = await _service.CalculateAsync(timeLog, shift, rule, userId);

        result.RegularMinutes.Should().Be(0);
        result.DailyOtMinutes.Should().Be(0);
        result.WeeklyOtMinutes.Should().Be(480);
    }

    private static DateTime ToUtc(DateOnly date, TimeOnly time)
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(time), vietnamTimeZone);
    }

    private static Shift CreateShift(DateOnly date, TimeOnly start, TimeOnly end, int breakMinutes, string note)
    {
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CHB-ARABICA-01",
            Name = "Cua hang Arabica Sua Da",
            Address = "123 Nguyen Hue",
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

    private static TimeLog CreateTimeLog(Shift shift, Guid userId, DateTime checkInUtc, DateTime checkOutUtc)
    {
        var assignment = CreateAssignment(shift, userId);
        var timeLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = assignment.Id,
            ShiftAssignment = assignment,
            CheckInAt = checkInUtc,
            CheckOutAt = checkOutUtc,
            Status = (int)TimeLogStatus.Valid
        };

        assignment.TimeLogs.Add(timeLog);
        return timeLog;
    }
}
