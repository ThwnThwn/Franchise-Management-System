using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetShiftById;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetShiftByIdHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetShiftByIdHandler _handler;

    public GetShiftByIdHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetShiftByIdHandler(_shiftRepositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenShiftDoesNotExist()
    {
        var shiftId = Guid.NewGuid();
        _shiftRepositoryMock
            .Setup(r => r.GetShiftByIdWithAssignmentsAsync(shiftId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        var act = () => _handler.Handle(new GetShiftByIdQuery(shiftId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{shiftId}*");
    }

    [Fact]
    public async Task Handle_ShouldMapAssignmentsAndTimeLogs_WhenShiftExists()
    {
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 3),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            RequiredStaffCount = 2,
            BreakDurationMinutes = 45,
            Note = "Ca toi pha cappuccino",
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var userId = Guid.NewGuid();
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Status = 1,
            HourlyWage = 45000m,
            Shift = shift,
            TimeLogs = new List<TimeLog>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ShiftAssignmentId = Guid.NewGuid(),
                    CheckInAt = new DateTime(2026, 4, 3, 14, 0, 0, DateTimeKind.Utc),
                    CheckOutAt = new DateTime(2026, 4, 3, 22, 0, 0, DateTimeKind.Utc),
                    Status = 1
                }
            }
        };
        shift.ShiftAssignments.Add(assignment);

        _shiftRepositoryMock
            .Setup(r => r.GetShiftByIdWithAssignmentsAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userId))
            .ReturnsAsync(new UserInfo
            {
                Id = userId,
                FirstName = "Lan",
                LastName = "Tran",
                Email = "lan.robusta@coffee.vn",
                AvatarUrl = "https://img/lan.png"
            });

        var result = await _handler.Handle(new GetShiftByIdQuery(shift.Id), CancellationToken.None);

        result.Id.Should().Be(shift.Id);
        result.Assignments.Should().HaveCount(1);
        result.Assignments[0].FirstName.Should().Be("Lan");
        result.Assignments[0].TimeLogs.Should().HaveCount(1);
        result.Assignments[0].TimeLogs[0].Status.Should().Be(1);
    }
}
