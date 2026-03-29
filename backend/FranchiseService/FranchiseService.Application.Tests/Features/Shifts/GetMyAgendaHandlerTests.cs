using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetMyAgenda;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetMyAgendaHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetMyAgendaHandler _handler;

    public GetMyAgendaHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetMyAgendaHandler(_shiftRepositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenUserHasNoAgenda()
    {
        var query = new GetMyAgendaQuery
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2026, 4, 7)
        };

        _shiftRepositoryMock
            .Setup(r => r.GetAgendaByUserAsync(query.UserId, query.StartDate, query.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
        _shiftRepositoryMock.Verify(r => r.GetAssignmentsByShiftIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMapAgendaAndCoworkers_WhenDataExists()
    {
        var userId = Guid.NewGuid();
        var coworkerId = Guid.NewGuid();
        var currentDay = (int)DateTime.UtcNow.DayOfWeek;

        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CHB-AGENDA-01",
            Name = "Quan Robusta Le Loi",
            Address = "456 Le Loi",
            Phone = "0909000111",
            Status = (int)StoreStatus.Open,
            StoreHours = new List<StoreHour>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = currentDay,
                    IsClosed = false,
                    OpenTime = new TimeOnly(0, 0),
                    CloseTime = new TimeOnly(23, 59)
                }
            },
            Shifts = new List<Shift>()
        };

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ShiftDate = new DateOnly(2026, 4, 5),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            ShiftAssignments = new List<ShiftAssignment>(),
            Store = store
        };

        var latestLog = new TimeLog
        {
            Id = Guid.NewGuid(),
            CheckInAt = DateTime.UtcNow.AddHours(-2),
            CheckOutAt = DateTime.UtcNow.AddHours(-1)
        };

        var myAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Shift = shift,
            TimeLogs = new List<TimeLog>
            {
                new() { Id = Guid.NewGuid(), CheckInAt = DateTime.UtcNow.AddHours(-4), CheckOutAt = null },
                latestLog
            }
        };

        var coworkerAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = coworkerId,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        var query = new GetMyAgendaQuery
        {
            UserId = userId,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2026, 4, 10)
        };

        _shiftRepositoryMock
            .Setup(r => r.GetAgendaByUserAsync(query.UserId, query.StartDate, query.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { myAssignment });

        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByShiftIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { myAssignment, coworkerAssignment });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new()
                {
                    Id = coworkerId,
                    FirstName = "Thao",
                    LastName = "Do",
                    AvatarUrl = "https://img/thao.png",
                    Phone = "0911222333"
                }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Store.Name.Should().Be("Quan Robusta Le Loi");
        result[0].Store.IsOpenNow.Should().BeTrue();
        result[0].CoWorkers.Should().HaveCount(1);
        result[0].CoWorkers[0].FirstName.Should().Be("Thao");
        result[0].CheckInAt.Should().Be(latestLog.CheckInAt);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOpenNowFalse_WhenStoreClosedOrNoHours()
    {
        var userId = Guid.NewGuid();
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = "CHB-AGENDA-02",
            Name = "Quan Sua Dac Pho Di Bo",
            Address = "789 Nguyen Hue",
            Status = (int)StoreStatus.Closed,
            StoreHours = new List<StoreHour>(),
            Shifts = new List<Shift>()
        };

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ShiftDate = new DateOnly(2026, 4, 6),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            ShiftAssignments = new List<ShiftAssignment>(),
            Store = store
        };

        var myAssignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userId,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        var query = new GetMyAgendaQuery
        {
            UserId = userId,
            StartDate = new DateOnly(2026, 4, 6),
            EndDate = new DateOnly(2026, 4, 6)
        };

        _shiftRepositoryMock
            .Setup(r => r.GetAgendaByUserAsync(query.UserId, query.StartDate, query.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { myAssignment });

        _shiftRepositoryMock
            .Setup(r => r.GetAssignmentsByShiftIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftAssignment> { myAssignment });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Store.IsOpenNow.Should().BeFalse();
        result[0].CoWorkers.Should().BeEmpty();
        _identityClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }
}
