using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.Shifts.Queries.GetUserCurrentShift;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetUserCurrentShiftHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly GetUserCurrentShiftHandler _handler;

    public GetUserCurrentShiftHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _handler = new GetUserCurrentShiftHandler(_shiftRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIdIsInvalid()
    {
        var query = new GetUserCurrentShiftQuery { UserId = "arabica-invalid-guid" };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid UserId*");
    }

    [Fact]
    public async Task Handle_ShouldReturnHasShiftTodayFalse_WhenNoActiveAssignment()
    {
        var userId = Guid.NewGuid();
        _shiftRepositoryMock
            .Setup(r => r.GetActiveShiftAssignmentAsync(
                userId,
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((ShiftAssignment?)null);

        var result = await _handler.Handle(new GetUserCurrentShiftQuery { UserId = userId.ToString() }, CancellationToken.None);

        result.HasShiftToday.Should().BeFalse();
        _shiftRepositoryMock.Verify(r => r.GetActiveShiftAssignmentAsync(
            userId,
            It.IsAny<DateTime>(),
            30,
            2), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapCurrentShiftAndFlags_WhenActiveAssignmentExists()
    {
        var userId = Guid.NewGuid();
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Name = "Quan Arabica Nguyen Hue",
            Address = "123 Nguyen Hue",
            Code = "CH-ARABICA",
            Latitude = 10.776m,
            Longitude = 106.700m,
            Shifts = new List<Shift>(),
            StoreHours = new List<StoreHour>()
        };

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            StartTime = new TimeOnly(22, 0),
            EndTime = new TimeOnly(6, 0),
            BreakDurationMinutes = 30,
            RequiredStaffCount = 2,
            ShiftAssignments = new List<ShiftAssignment>(),
            Store = store
        };

        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Shift = shift,
            TimeLogs = new List<TimeLog>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ShiftAssignmentId = Guid.NewGuid(),
                    CheckInAt = DateTime.UtcNow.AddHours(-3),
                    CheckOutAt = null,
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ShiftAssignmentId = Guid.NewGuid(),
                    CheckInAt = DateTime.UtcNow.AddHours(-6),
                    CheckOutAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                }
            }
        };

        _shiftRepositoryMock
            .Setup(r => r.GetActiveShiftAssignmentAsync(
                userId,
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(assignment);

        var result = await _handler.Handle(new GetUserCurrentShiftQuery { UserId = userId.ToString() }, CancellationToken.None);

        result.HasShiftToday.Should().BeTrue();
        result.StoreName.Should().Be("Quan Arabica Nguyen Hue");
        result.CanCheckIn.Should().BeFalse();
        result.CanCheckOut.Should().BeTrue();
        result.IsOvertime.Should().BeTrue();
        result.CurrentLogId.Should().Be(assignment.TimeLogs.First().Id);
        result.CheckInAt.Should().Contain("T");
        result.ServerTime.Should().Contain("T");
    }
}
