using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftSwaps.Queries.GetSwapRequestsByStore;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class GetSwapRequestsByStoreHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetSwapRequestsByStoreHandler _handler;

    public GetSwapRequestsByStoreHandlerTests()
    {
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetSwapRequestsByStoreHandler(_swapRepoMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFilterOutExpired_WhenStatusFilterApplied()
    {
        var storeId = Guid.NewGuid();
        var swap = CreateSwap(storeId, (int)SwapRequestStatus.PendingTarget, DateTimeOffset.UtcNow.AddMinutes(-1));

        _swapRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[] { swap });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>());

        var result = await _handler.Handle(new GetSwapRequestsByStoreQuery
        {
            StoreId = storeId,
            Status = (int)SwapRequestStatus.PendingTarget
        }, CancellationToken.None);

        result.Should().BeEmpty();
        swap.Status.Should().Be((int)SwapRequestStatus.Expired);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapDtos_WhenNoStatusFilter()
    {
        var storeId = Guid.NewGuid();
        var swap1 = CreateSwap(storeId, (int)SwapRequestStatus.PendingManager, DateTimeOffset.UtcNow.AddHours(-2));
        var swap2 = CreateSwap(storeId, (int)SwapRequestStatus.Approved, DateTimeOffset.UtcNow.AddHours(-1));

        _swapRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, object>>[]>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[] { swap1, swap2 });

        _identityClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<UserInfo>
            {
                new() { Id = swap1.SourceUserId, FirstName = "Anh", LastName = "Vo" },
                new() { Id = swap1.TargetUserId, FirstName = "Binh", LastName = "Do" }
            });

        var result = await _handler.Handle(new GetSwapRequestsByStoreQuery { StoreId = storeId }, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
        result.Should().Contain(x => !string.IsNullOrWhiteSpace(x.SourceUserName));
    }

    private static ShiftSwapRequest CreateSwap(Guid storeId, int status, DateTimeOffset expiresAt)
    {
        var srcUser = Guid.NewGuid();
        var tgtUser = Guid.NewGuid();

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ShiftDate = new DateOnly(2026, 4, 25),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            UserId = srcUser,
            Shift = shift,
            TimeLogs = new List<TimeLog>()
        };

        return new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = assignment.Id,
            SourceAssignment = assignment,
            SourceUserId = srcUser,
            TargetUserId = tgtUser,
            Status = status,
            Reason = "Doi ca pha Robusta",
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}

