using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.Queries.GetActiveStaffsAtStore;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.Shifts;

public class GetActiveStaffsAtStoreHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetActiveStaffsAtStoreHandler _handler;

    public GetActiveStaffsAtStoreHandlerTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetActiveStaffsAtStoreHandler(_shiftRepositoryMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDistinctActiveStaffs_ForRegularShiftWindow()
    {
        var storeId = Guid.NewGuid();
        var userArabica = Guid.NewGuid();
        var userRobusta = Guid.NewGuid();

        var now = DateTime.UtcNow;

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = DateOnly.FromDateTime(now),
            StartTime = new TimeOnly(0, 0),
            EndTime = new TimeOnly(23, 59),
            ShiftAssignments = new List<ShiftAssignment>
            {
                new() { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userArabica, TimeLogs = new List<TimeLog>() },
                new() { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userArabica, TimeLogs = new List<TimeLog>() },
                new() { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userRobusta, TimeLogs = new List<TimeLog>() }
            }
        };

        _shiftRepositoryMock
            .Setup(r => r.GetShiftsWithAssignmentsAsync(
                storeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { shift });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userArabica))
            .ReturnsAsync(new UserInfo { Id = userArabica, FirstName = "Duy", LastName = "Tran", Email = "duy@coffee.vn" });
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userRobusta))
            .ReturnsAsync(new UserInfo { Id = userRobusta, FirstName = "Nhi", LastName = "Le", Email = "nhi@coffee.vn" });

        var result = await _handler.Handle(new GetActiveStaffsAtStoreQuery(storeId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.UserId).Should().OnlyHaveUniqueItems();
        result.Should().Contain(x => x.UserId == userArabica && x.FirstName == "Duy");
        result.Should().Contain(x => x.UserId == userRobusta && x.FirstName == "Nhi");
    }

    [Fact]
    public async Task Handle_ShouldHandleOvernightShift_AndSkipUserWhenIdentityNotFound()
    {
        var storeId = Guid.NewGuid();
        var userArabica = Guid.NewGuid();

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);

        var overnightShift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime = now.AddHours(12),
            EndTime = now.AddHours(11),
            ShiftAssignments = new List<ShiftAssignment>
            {
                new() { Id = Guid.NewGuid(), ShiftId = Guid.NewGuid(), UserId = userArabica, TimeLogs = new List<TimeLog>() }
            }
        };

        _shiftRepositoryMock
            .Setup(r => r.GetShiftsWithAssignmentsAsync(
                storeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overnightShift });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(userArabica))
            .ReturnsAsync((UserInfo?)null);

        var result = await _handler.Handle(new GetActiveStaffsAtStoreQuery(storeId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
