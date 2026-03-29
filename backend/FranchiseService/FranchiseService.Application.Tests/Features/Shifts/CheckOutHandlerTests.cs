using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Shifts.Commands.CheckOut;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using SharedKernel.Constants;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class CheckOutHandlerTests
{
    private readonly Mock<IGenericRepository<TimeLog>> _timeLogRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly Mock<IOvertimeCalculationService> _overtimeServiceMock;
    private readonly CheckOutHandler _handler;

    public CheckOutHandlerTests()
    {
        _timeLogRepositoryMock = new Mock<IGenericRepository<TimeLog>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();
        _overtimeServiceMock = new Mock<IOvertimeCalculationService>();

        _handler = new CheckOutHandler(
            _timeLogRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _shiftRuleServiceMock.Object,
            _overtimeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTimeLogMissing()
    {
        var logId = Guid.NewGuid();
        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync((TimeLog?)null);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = logId, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenAssignmentMissing()
    {
        var timeLog = CreateLog();
        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync((ShiftAssignment?)null);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = timeLog.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotOwner()
    {
        var timeLog = CreateLog();
        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = timeLog.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenAlreadyCheckedOut()
    {
        var timeLog = CreateLog();
        timeLog.CheckOutAt = DateTime.UtcNow;

        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = timeLog.Id, UserId = assignment.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*check-out roi*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenCheckInMissing()
    {
        var timeLog = CreateLog();
        timeLog.CheckInAt = null;

        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = timeLog.Id, UserId = assignment.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*check-in*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftMissing()
    {
        var timeLog = CreateLog();
        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(new CheckOutCommand { LogId = timeLog.Id, UserId = assignment.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCapCheckOutTime_AndUpdateOtAndNote_WhenValid()
    {
        var userId = Guid.NewGuid();
        var timeLog = CreateLog();
        timeLog.CheckInAt = DateTime.UtcNow.AddHours(-10);
        timeLog.ManagerNote = "Check-in binh thuong";

        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = userId,
            TimeLogs = new List<TimeLog>()
        };

        var vnNow = GetVietnamNow();
        var shiftDate = DateOnly.FromDateTime(vnNow.AddDays(-1));
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = shiftDate,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _shiftRuleServiceMock.Setup(s => s.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { MaxHoursPerWeek = 48 });
        _overtimeServiceMock
            .Setup(o => o.CalculateAsync(timeLog, shift, It.IsAny<ShiftRule>(), userId))
            .ReturnsAsync(new OvertimeResult { RegularMinutes = 420, DailyOtMinutes = 60, WeeklyOtMinutes = 20 });

        var result = await _handler.Handle(new CheckOutCommand
        {
            LogId = timeLog.Id,
            UserId = userId,
            Latitude = 10.78m,
            Longitude = 106.69m,
            Note = "Da ban giao may pha"
        }, CancellationToken.None);

        result.Should().BeTrue();
        timeLog.Status.Should().Be((int)TimeLogStatus.Valid);
        timeLog.RegularMinutes.Should().Be(420);
        timeLog.DailyOtMinutes.Should().Be(60);
        timeLog.WeeklyOtMinutes.Should().Be(20);
        timeLog.ManagerNote.Should().Contain("Check-in binh thuong");
        timeLog.ManagerNote.Should().Contain("Da ban giao may pha");

        var expectedMax = GetMaxCheckOutUtc(shift.ShiftDate, shift.StartTime, shift.EndTime);
        timeLog.CheckOutAt.Should().Be(expectedMax);

        _timeLogRepositoryMock.Verify(r => r.UpdateAsync(timeLog), Times.Once);
    }

    private static TimeLog CreateLog()
    {
        return new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = Guid.NewGuid(),
            CheckInAt = DateTime.UtcNow.AddHours(-4),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-4),
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static DateTime GetVietnamNow()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    private static DateTime GetMaxCheckOutUtc(DateOnly shiftDate, TimeOnly start, TimeOnly end)
    {
        var endDate = end < start ? shiftDate.AddDays(1) : shiftDate;
        var localEnd = endDate.ToDateTime(end);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);
        return utcEnd.AddHours(TimeConstants.MaxLateCheckOutHours);
    }
}
