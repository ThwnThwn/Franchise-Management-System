using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetTimeLogs;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetTimeLogsHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetTimeLogsHandler _handler;

    public GetTimeLogsHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetTimeLogsHandler(_shiftRepositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultDateRangeAndMapStaff_WhenUserInfoExists()
    {
        var storeId = Guid.NewGuid();
        var staffArabica = Guid.NewGuid();
        var staffRobusta = Guid.NewGuid();

        var logArabica = CreateTimeLog(staffArabica, "Ca sang Arabica", 1);
        var logRobusta = CreateTimeLog(staffRobusta, "Ca chieu Robusta", null);

        _shiftRepositoryMock
            .Setup(r => r.GetTimeLogsAsync(
                storeId,
                null,
                1,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                2,
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { logArabica, logRobusta }, 2));

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new()
                {
                    Id = staffArabica,
                    FirstName = "Binh",
                    LastName = "Le",
                    Email = "binh.arabica@coffee.vn",
                    AvatarUrl = "https://img/binh.png"
                }
            });

        var query = new GetTimeLogsQuery
        {
            StoreId = storeId,
            Status = 1,
            PageNumber = 2,
            PageSize = 5
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items[0].Staff.FirstName.Should().Be("Binh");
        result.Items[1].Staff.FirstName.Should().BeNull();
        result.Items[1].Status.Should().Be(0);

        var expectedFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var expectedTo = DateOnly.FromDateTime(DateTime.Today);
        _shiftRepositoryMock.Verify(r => r.GetTimeLogsAsync(
            storeId,
            null,
            1,
            expectedFrom,
            expectedTo,
            2,
            5,
            It.IsAny<CancellationToken>()), Times.Once);

        _identityClientMock.Verify(c => c.GetUsersByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Distinct().Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoTimeLogFound()
    {
        _shiftRepositoryMock
            .Setup(r => r.GetTimeLogsAsync(
                null,
                null,
                null,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<TimeLog>(), 0));

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>());

        var result = await _handler.Handle(new GetTimeLogsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _identityClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
    }

    private static TimeLog CreateTimeLog(Guid staffId, string shiftNote, int? status)
    {
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 4),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            Note = shiftNote,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = staffId,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };
        shift.ShiftAssignments.Add(assignment);

        var timeLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = assignment.Id,
            ShiftAssignment = assignment,
            CheckInAt = new DateTime(2026, 4, 4, 6, 0, 0, DateTimeKind.Utc),
            CheckOutAt = new DateTime(2026, 4, 4, 14, 0, 0, DateTimeKind.Utc),
            CheckInImageUrl = "https://img/checkin.png",
            CheckOutImageUrl = "https://img/checkout.png",
            RegularMinutes = 420,
            DailyOtMinutes = 30,
            WeeklyOtMinutes = 15,
            Status = status,
            ManagerNote = "Pha den da"
        };

        assignment.TimeLogs.Add(timeLog);
        return timeLog;
    }
}
