using FluentAssertions;
using FranchiseService.Application.Features.ShiftSwaps.Commands.CancelSwap;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using Moq;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class CancelSwapHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenSwapRequestNotFound()
    {
        var command = new CancelSwapCommand
        {
            SwapRequestId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        _swapRepoMock
            .Setup(x => x.GetByIdAsync(command.SwapRequestId))
            .ReturnsAsync((ShiftSwapRequest?)null);

        var handler = new CancelSwapHandler(_swapRepoMock.Object);
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserIsNotRequester()
    {
        var swap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.PendingTarget
        };

        _swapRepoMock
            .Setup(x => x.GetByIdAsync(swap.Id))
            .ReturnsAsync(swap);

        var handler = new CancelSwapHandler(_swapRepoMock.Object);
        var act = () => handler.Handle(
            new CancelSwapCommand
            {
                SwapRequestId = swap.Id,
                UserId = Guid.NewGuid()
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*quyen huy*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSwapAlreadyProcessed()
    {
        var userId = Guid.NewGuid();
        var swap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceUserId = userId,
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.Approved
        };

        _swapRepoMock
            .Setup(x => x.GetByIdAsync(swap.Id))
            .ReturnsAsync(swap);

        var handler = new CancelSwapHandler(_swapRepoMock.Object);
        var act = () => handler.Handle(
            new CancelSwapCommand
            {
                SwapRequestId = swap.Id,
                UserId = userId
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong the huy*");
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenStatusPendingTarget()
    {
        var userId = Guid.NewGuid();
        var swap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceUserId = userId,
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.PendingTarget
        };

        _swapRepoMock
            .Setup(x => x.GetByIdAsync(swap.Id))
            .ReturnsAsync(swap);

        _swapRepoMock
            .Setup(x => x.UpdateAsync(swap))
            .ReturnsAsync(1);

        var handler = new CancelSwapHandler(_swapRepoMock.Object);
        var result = await handler.Handle(
            new CancelSwapCommand
            {
                SwapRequestId = swap.Id,
                UserId = userId
            },
            CancellationToken.None);

        result.Should().BeTrue();
        swap.Status.Should().Be((int)SwapRequestStatus.Cancelled);
        _swapRepoMock.Verify(x => x.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenStatusPendingManager()
    {
        var userId = Guid.NewGuid();
        var swap = new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceUserId = userId,
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.PendingManager
        };

        _swapRepoMock
            .Setup(x => x.GetByIdAsync(swap.Id))
            .ReturnsAsync(swap);

        _swapRepoMock
            .Setup(x => x.UpdateAsync(swap))
            .ReturnsAsync(1);

        var handler = new CancelSwapHandler(_swapRepoMock.Object);
        var result = await handler.Handle(
            new CancelSwapCommand
            {
                SwapRequestId = swap.Id,
                UserId = userId
            },
            CancellationToken.None);

        result.Should().BeTrue();
        swap.Status.Should().Be((int)SwapRequestStatus.Cancelled);
    }
}
