using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseTransferEntity = WarehouseService.Domain.Entities.WarehouseTransfer;

namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

public class CancelWarehouseTransferHandlerTests
{
    private readonly Mock<IWarehouseTransferRepository> _transferRepositoryMock;
    private readonly CancelWarehouseTransferCommandHandler _handler;

    // Reusable pending transfer — cancellable by default.
    private readonly WarehouseTransferEntity _pendingTransfer;

    public CancelWarehouseTransferHandlerTests()
    {
        _transferRepositoryMock = new Mock<IWarehouseTransferRepository>();

        _handler = new CancelWarehouseTransferCommandHandler(
            _transferRepositoryMock.Object
        );

        _pendingTransfer = new WarehouseTransferEntity
        {
            Id = Guid.NewGuid(),
            TransferCode = "TRF-20260101-XYZABC",
            Status = TransferStatus.Pending,
            Reason = "Original reason",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default: transfer found
        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pendingTransfer);

        _transferRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    // -----------------------------------------------------------------------
    // GROUP 1 — TRANSFER NOT FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransferEntity?)null);

        var command = new CancelWarehouseTransferCommand(id, null, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenTransferDoesNotExist()
    {
        // Arrange
        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransferEntity?)null);

        var command = new CancelWarehouseTransferCommand(Guid.NewGuid(), null, Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _transferRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — STATUS GUARD
    // Pending and Approved are the only two cancellable statuses.
    // Completed, Rejected, Cancelled are not.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(TransferStatus.Completed)]
    [InlineData(TransferStatus.Rejected)]
    [InlineData(TransferStatus.Cancelled)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransferIsNotCancellable(TransferStatus nonCancellableStatus)
    {
        // Arrange
        _pendingTransfer.Status = nonCancellableStatus;
        var command = new CancelWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{nonCancellableStatus}*");
    }

    [Theory]
    [InlineData(TransferStatus.Pending)]
    [InlineData(TransferStatus.Approved)]
    public async Task Handle_ShouldNotThrow_WhenTransferIsCancellable(TransferStatus cancellableStatus)
    {
        // Arrange — both Pending and Approved should succeed
        _pendingTransfer.Status = cancellableStatus;
        var command = new CancelWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenTransferIsNotCancellable()
    {
        // Arrange
        _pendingTransfer.Status = TransferStatus.Completed;
        var command = new CancelWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _transferRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — REASON UPDATE LOGIC
    // transfer.Reason = request.Reason ?? transfer.Reason
    // Two branches: request.Reason provided → overwrite; null → keep existing.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldOverwriteReason_WhenRequestReasonIsProvided()
    {
        // Arrange
        _pendingTransfer.Reason = "Original reason";
        var command = new CancelWarehouseTransferCommand(
            _pendingTransfer.Id, "Customer request", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — reason overwritten
        result.Reason.Should().Be("Customer request");
        _pendingTransfer.Reason.Should().Be("Customer request");
    }

    [Fact]
    public async Task Handle_ShouldKeepExistingReason_WhenRequestReasonIsNull()
    {
        // Arrange
        _pendingTransfer.Reason = "Original reason";
        var command = new CancelWarehouseTransferCommand(
            _pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — existing reason preserved via ?? operator
        result.Reason.Should().Be("Original reason");
        _pendingTransfer.Reason.Should().Be("Original reason");
    }

    [Fact]
    public async Task Handle_ShouldReturnNullReason_WhenBothRequestAndExistingReasonAreNull()
    {
        // Arrange
        _pendingTransfer.Reason = null;
        var command = new CancelWarehouseTransferCommand(
            _pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert — null ?? null = null
        result.Reason.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — SUCCESSFUL CANCELLATION & FIELD MAPPING
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithCancelledStatus_WhenRequestIsValid()
    {
        // Arrange
        var command = new CancelWarehouseTransferCommand(
            _pendingTransfer.Id, "Out of stock", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_pendingTransfer.Id);
        result.TransferCode.Should().Be("TRF-20260101-XYZABC");
        result.Status.Should().Be(TransferStatus.Cancelled);
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldMutateTransferEntity_WithCancelledValues()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var command = new CancelWarehouseTransferCommand(
            _pendingTransfer.Id, "Stock issue", Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert — entity mutated in place before SaveChanges
        _pendingTransfer.Status.Should().Be(TransferStatus.Cancelled);
        _pendingTransfer.UpdatedAt.Should().BeOnOrAfter(before);
        _pendingTransfer.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // -----------------------------------------------------------------------
    // GROUP 5 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = new CancelWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        _transferRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsync_WithCorrectId()
    {
        // Arrange
        var command = new CancelWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        _transferRepositoryMock.Verify(
            x => x.GetByIdAsync(_pendingTransfer.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}