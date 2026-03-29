using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetShifts;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetShiftsHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetShiftsHandler _handler;

    public GetShiftsHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetShiftsHandler(_shiftRepositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultDateRangeAndMapAssignments_WhenFilterDateIsNull()
    {
        var storeId = Guid.NewGuid();
        var staffArabica = Guid.NewGuid();
        var staffRobusta = Guid.NewGuid();
        var shift = CreateShift(storeId, new DateOnly(2026, 4, 1), "Ca sang pha Arabica");
        shift.ShiftAssignments.Add(CreateAssignment(shift, staffArabica, 1, 42000m, new DateTime(2026, 4, 1, 7, 0, 0, DateTimeKind.Utc)));
        shift.ShiftAssignments.Add(CreateAssignment(shift, staffRobusta, 0, 40000m, null));

        _shiftRepositoryMock
            .Setup(r => r.GetShiftsWithAssignmentsAsync(
                storeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { shift });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(staffArabica))
            .ReturnsAsync(new UserInfo
            {
                Id = staffArabica,
                FirstName = "Minh",
                LastName = "Nguyen",
                Email = "minh.arabica@coffee.vn",
                AvatarUrl = "https://img/arabica.png"
            });
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(staffRobusta))
            .ReturnsAsync((UserInfo?)null);

        var query = new GetShiftsQuery(storeId, null, null, 1);
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].AssignedStaffCount.Should().Be(2);
        result[0].Assignments.Should().HaveCount(2);
        result[0].Assignments[0].FirstName.Should().Be("Minh");
        result[0].Assignments[1].FirstName.Should().BeEmpty();
        result[0].Assignments[1].TimeLogs[0].Status.Should().Be(0);

        var expectedFrom = DateOnly.FromDateTime(DateTime.Today);
        var expectedTo = expectedFrom.AddDays(7);
        _shiftRepositoryMock.Verify(r => r.GetShiftsWithAssignmentsAsync(
            storeId,
            expectedFrom,
            expectedTo,
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnShiftWithoutAssignments_WhenShiftAssignmentsIsNull()
    {
        var storeId = Guid.NewGuid();
        var shift = CreateShift(storeId, new DateOnly(2026, 4, 2), "Ca chieu sua dac");
        shift.ShiftAssignments = null!;

        _shiftRepositoryMock
            .Setup(r => r.GetShiftsWithAssignmentsAsync(
                storeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { shift });

        var query = new GetShiftsQuery(storeId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 7), null);
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Assignments.Should().BeEmpty();
        result[0].AssignedStaffCount.Should().Be(0);
        _identityClientMock.Verify(c => c.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    private static Shift CreateShift(Guid storeId, DateOnly date, string note)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = date,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30,
            RequiredStaffCount = 2,
            Status = 1,
            Note = note,
            ShiftAssignments = new List<ShiftAssignment>()
        };
    }

    private static ShiftAssignment CreateAssignment(
        Shift shift,
        Guid userId,
        int status,
        decimal hourlyWage,
        DateTime? checkInAt)
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Status = status,
            HourlyWage = hourlyWage,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        assignment.TimeLogs.Add(new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = assignment.Id,
            ShiftAssignment = assignment,
            CheckInAt = checkInAt,
            CheckOutAt = null,
            Status = null
        });

        return assignment;
    }
}
