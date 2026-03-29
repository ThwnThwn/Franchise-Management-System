using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class GetMySwapRequestsHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetMySwapRequestsHandler _handler;

    public GetMySwapRequestsHandlerTests()
    {
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetMySwapRequestsHandler(_swapRepoMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldExpirePendingTargetRequests_WhenExpired()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 23),
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = userA,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        var expiredSwap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = assignment.Id,
            SourceAssignment = assignment,
            SourceUserId = userA,
            TargetUserId = userB,
            Status = (int)SwapRequestStatus.PendingTarget,
            Reason = "Doi ca pha Arabica",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        _swapRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[] { expiredSwap });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = userA, FirstName = "A", LastName = "Nguyen" },
                new() { Id = userB, FirstName = "B", LastName = "Tran" }
            });

        var result = await _handler.Handle(new GetMySwapRequestsQuery { UserId = userA }, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be((int)SwapRequestStatus.Expired);
        result[0].SourceUserName.Should().Be("A Nguyen");
        _swapRepoMock.Verify(r => r.UpdateAsync(expiredSwap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAndSortDescendingByCreatedAt()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var oldSwap = CreateSwap(userA, userB, DateTimeOffset.UtcNow.AddHours(-3));
        var newSwap = CreateSwap(userA, userB, DateTimeOffset.UtcNow.AddHours(-1));

        _swapRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[] { oldSwap, newSwap });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = userA, FirstName = "Lan", LastName = "Le" },
                new() { Id = userB, FirstName = "My", LastName = "Pham" }
            });

        var result = await _handler.Handle(new GetMySwapRequestsQuery { UserId = userA }, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newSwap.Id);
        result[1].Id.Should().Be(oldSwap.Id);
    }

    private static ShiftSwapRequest CreateSwap(Guid sourceUser, Guid targetUser, DateTimeOffset createdAt)
    {
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2026, 4, 24),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = sourceUser,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        return new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = assignment.Id,
            SourceAssignment = assignment,
            SourceUserId = sourceUser,
            TargetUserId = targetUser,
            Status = (int)SwapRequestStatus.PendingManager,
            Reason = "Doi ca sua dac",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(12),
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
