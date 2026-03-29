using FluentAssertions;
using Moq;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Shifts.Commands.CheckIn;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class CheckInHandlerTests
{
    private readonly Mock<IGenericRepository<TimeLog>> _timeLogRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly List<TimeLog> _addedLogs;
    private readonly CheckInHandler _handler;

    public CheckInHandlerTests()
    {
        _timeLogRepositoryMock = new Mock<IGenericRepository<TimeLog>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();
        _addedLogs = new List<TimeLog>();

        _timeLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TimeLog>()))
            .Callback<TimeLog>(l => _addedLogs.Add(l))
            .ReturnsAsync(Guid.NewGuid());

        _handler = new CheckInHandler(
            _timeLogRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _shiftRuleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenAssignmentMissing()
    {
        var command = new CheckInCommand { AssignmentId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(command.AssignmentId)).ReturnsAsync((ShiftAssignment?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotOwnerOfAssignment()
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        var act = () => _handler.Handle(new CheckInCommand { AssignmentId = assignment.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftNotPublished()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userId, TimeLogs = new List<TimeLog>() };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 18),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            Status = (int)ShiftStatus.Draft,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new CheckInCommand { AssignmentId = assignment.Id, UserId = userId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenCheckInTooEarly()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userId, TimeLogs = new List<TimeLog>() };

        var vnNow = GetVietnamNow();
        var future = vnNow.AddDays(1);

        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(future),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new CheckInCommand { AssignmentId = assignment.Id, UserId = userId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*som nhat*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenShiftAlreadyEnded()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userId, TimeLogs = new List<TimeLog>() };

        var vnNow = GetVietnamNow();
        var past = vnNow.AddDays(-1);

        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(past),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);

        var act = () => _handler.Handle(new CheckInCommand { AssignmentId = assignment.Id, UserId = userId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*da ket thuc*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenExistingOpenTimeLogFound()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userId, TimeLogs = new List<TimeLog>() };

        var vnNow = GetVietnamNow();
        var currentDate = DateOnly.FromDateTime(vnNow);
        var start = new TimeOnly(vnNow.Hour, vnNow.Minute).AddMinutes(-5);
        var end = new TimeOnly(vnNow.Hour, vnNow.Minute).AddHours(2);

        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = currentDate,
            StartTime = start,
            EndTime = end,
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _timeLogRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new TimeLog { Id = Guid.NewGuid(), ShiftAssignmentId = assignment.Id, CheckInAt = DateTime.UtcNow.AddHours(-1), CheckOutAt = null }
            });

        var act = () => _handler.Handle(new CheckInCommand { AssignmentId = assignment.Id, UserId = userId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*check-in roi*");
    }

    [Fact]
    public async Task Handle_ShouldCreateTimeLogAndApplyGracePeriod_WhenWithinGraceWindow()
    {
        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userId, TimeLogs = new List<TimeLog>() };

        var vnNow = GetVietnamNow();
        var currentDate = DateOnly.FromDateTime(vnNow);
        var start = new TimeOnly(vnNow.Hour, vnNow.Minute).AddMinutes(-5);
        var end = new TimeOnly(vnNow.Hour, vnNow.Minute).AddHours(2);

        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = currentDate,
            StartTime = start,
            EndTime = end,
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _timeLogRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<TimeLog, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<TimeLog>());
        _shiftRuleServiceMock
            .Setup(s => s.GetEffectiveRuleAsync(shift.StoreId))
            .ReturnsAsync(new ShiftRule { GracePeriodMinutes = 10 });

        var resultId = await _handler.Handle(new CheckInCommand
        {
            AssignmentId = assignment.Id,
            UserId = userId,
            Latitude = 10.776m,
            Longitude = 106.700m,
            Note = "Den som pha Arabica"
        }, CancellationToken.None);

        _addedLogs.Should().HaveCount(1);
        _addedLogs[0].Id.Should().Be(resultId);
        _addedLogs[0].Status.Should().Be((int)TimeLogStatus.Processing);
        _addedLogs[0].CheckInLat.Should().Be(10.776m);
        _addedLogs[0].CheckInLong.Should().Be(106.700m);

        var expectedShiftStartUtc = ToUtc(shift.ShiftDate, shift.StartTime);
        _addedLogs[0].CheckInAt.Should().Be(expectedShiftStartUtc);
    }

    private static DateTime GetVietnamNow()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    private static DateTime ToUtc(DateOnly date, TimeOnly time)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId());
        return TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(time), tz);
    }
}
