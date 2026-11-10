using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Commands.UpdateTimeLog;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class UpdateTimeLogHandlerTests
{
    private readonly Mock<IGenericRepository<TimeLog>> _timeLogRepositoryMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepositoryMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepositoryMock;
    private readonly Mock<IShiftRuleService> _shiftRuleServiceMock;
    private readonly Mock<IOvertimeCalculationService> _overtimeServiceMock;
    private readonly UpdateTimeLogHandler _handler;

    public UpdateTimeLogHandlerTests()
    {
        _timeLogRepositoryMock = new Mock<IGenericRepository<TimeLog>>();
        _assignmentRepositoryMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepositoryMock = new Mock<IGenericRepository<Shift>>();
        _shiftRuleServiceMock = new Mock<IShiftRuleService>();
        _overtimeServiceMock = new Mock<IOvertimeCalculationService>();

        _handler = new UpdateTimeLogHandler(
            _timeLogRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _shiftRuleServiceMock.Object,
            _overtimeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenStatusInvalid()
    {
        var act = () => _handler.Handle(new UpdateTimeLogCommand { Id = Guid.NewGuid(), Status = 99 }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Trang thai khong hop le*");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenStatusInvalidButManagerNoteMissing()
    {
        var act = () => _handler.Handle(new UpdateTimeLogCommand
        {
            Id = Guid.NewGuid(),
            Status = (int)TimeLogStatus.Invalid,
            ManagerNote = "  "
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Bat buoc phai nhap ly do*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTimeLogMissing()
    {
        var id = Guid.NewGuid();
        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TimeLog?)null);

        var act = () => _handler.Handle(new UpdateTimeLogCommand { Id = id, Status = (int)TimeLogStatus.Valid }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenAssignmentMissingDuringRecalculate()
    {
        var timeLog = CreateTimeLog();
        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync((ShiftAssignment?)null);

        var act = () => _handler.Handle(new UpdateTimeLogCommand
        {
            Id = timeLog.Id,
            Status = (int)TimeLogStatus.Valid,
            ManagerNote = "Duyet cong Arabica"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*phan cong ca*");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftMissingDuringRecalculate()
    {
        var timeLog = CreateTimeLog();
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

        var act = () => _handler.Handle(new UpdateTimeLogCommand
        {
            Id = timeLog.Id,
            Status = (int)TimeLogStatus.Valid,
            ManagerNote = "Duyet cong Robusta"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*ca lam viec*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateAndRecalculateOt_WhenStatusValidAndDataEnough()
    {
        var timeLog = CreateTimeLog();
        var assignment = new ShiftAssignment
        {
            Id = timeLog.ShiftAssignmentId,
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };
        var shift = new Shift
        {
            Id = assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 16),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.ShiftAssignmentId)).ReturnsAsync(assignment);
        _shiftRepositoryMock.Setup(r => r.GetByIdAsync(assignment.ShiftId)).ReturnsAsync(shift);
        _shiftRuleServiceMock.Setup(s => s.GetEffectiveRuleAsync(shift.StoreId)).ReturnsAsync(new ShiftRule { MaxHoursPerWeek = 48 });
        _overtimeServiceMock
            .Setup(o => o.CalculateAsync(timeLog, shift, It.IsAny<ShiftRule>(), assignment.UserId))
            .ReturnsAsync(new OvertimeResult { RegularMinutes = 420, DailyOtMinutes = 30, WeeklyOtMinutes = 10 });

        var result = await _handler.Handle(new UpdateTimeLogCommand
        {
            Id = timeLog.Id,
            Status = (int)TimeLogStatus.Valid,
            ManagerNote = "Cong hop le"
        }, CancellationToken.None);

        result.Should().BeTrue();
        timeLog.RegularMinutes.Should().Be(420);
        timeLog.DailyOtMinutes.Should().Be(30);
        timeLog.WeeklyOtMinutes.Should().Be(10);
        _timeLogRepositoryMock.Verify(r => r.UpdateAsync(timeLog), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateWithoutRecalculate_WhenStatusNotValid()
    {
        var timeLog = CreateTimeLog();

        _timeLogRepositoryMock.Setup(r => r.GetByIdAsync(timeLog.Id)).ReturnsAsync(timeLog);

        var result = await _handler.Handle(new UpdateTimeLogCommand
        {
            Id = timeLog.Id,
            Status = (int)TimeLogStatus.Approved,
            ManagerNote = "Da xac nhan"
        }, CancellationToken.None);

        result.Should().BeTrue();
        _overtimeServiceMock.Verify(o => o.CalculateAsync(It.IsAny<TimeLog>(), It.IsAny<Shift>(), It.IsAny<ShiftRule>(), It.IsAny<Guid>()), Times.Never);
        _timeLogRepositoryMock.Verify(r => r.UpdateAsync(timeLog), Times.Once);
    }

    private static TimeLog CreateTimeLog()
    {
        return new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = Guid.NewGuid(),
            CheckInAt = DateTime.UtcNow.AddHours(-8),
            CheckOutAt = DateTime.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-8),
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
