using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftSwaps.Commands.RespondSwap;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class RespondSwapHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly RespondSwapHandler _handler;

    public RespondSwapHandlerTests()
    {
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _handler = new RespondSwapHandler(_swapRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSwapNotFound()
    {
        var id = Guid.NewGuid();
        _swapRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftSwapRequest?)null);

        var act = () => _handler.Handle(new RespondSwapCommand { SwapRequestId = id, UserId = Guid.NewGuid(), Accept = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong ton tai*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenResponderIsNotTargetUser()
    {
        var swap = CreateSwap();
        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var act = () => _handler.Handle(new RespondSwapCommand { SwapRequestId = swap.Id, UserId = Guid.NewGuid(), Accept = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*co quyen tra loi*");
    }

    [Fact]
    public async Task Handle_ShouldExpireAndThrow_WhenRequestExpired()
    {
        var swap = CreateSwap();
        swap.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var act = () => _handler.Handle(new RespondSwapCommand { SwapRequestId = swap.Id, UserId = swap.TargetUserId, Accept = true }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*het han*");

        swap.Status.Should().Be((int)SwapRequestStatus.Expired);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMoveToPendingManager_WhenTargetAccepts()
    {
        var swap = CreateSwap();
        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var result = await _handler.Handle(new RespondSwapCommand
        {
            SwapRequestId = swap.Id,
            UserId = swap.TargetUserId,
            Accept = true
        }, CancellationToken.None);

        result.Should().BeTrue();
        swap.Status.Should().Be((int)SwapRequestStatus.PendingManager);
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenTargetDeclines()
    {
        var swap = CreateSwap();
        _swapRepoMock.Setup(r => r.GetByIdAsync(swap.Id)).ReturnsAsync(swap);

        var result = await _handler.Handle(new RespondSwapCommand
        {
            SwapRequestId = swap.Id,
            UserId = swap.TargetUserId,
            Accept = false,
            RejectReason = "Em ban tiec sinh nhat"
        }, CancellationToken.None);

        result.Should().BeFalse();
        swap.Status.Should().Be((int)SwapRequestStatus.Rejected);
        swap.RejectReason.Should().Contain("sinh nhat");
        _swapRepoMock.Verify(r => r.UpdateAsync(swap), Times.Once);
    }

    private static ShiftSwapRequest CreateSwap()
    {
        return new ShiftSwapRequest
        {
            Id = Guid.NewGuid(),
            SourceAssignmentId = Guid.NewGuid(),
            SourceUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Status = (int)SwapRequestStatus.PendingTarget,
            Reason = "Doi ca pha Arabica",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(12),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
