using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseTransferEntity = WarehouseService.Domain.Entities.WarehouseTransfer;

namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

public class ApproveWarehouseTransferHandlerTests
{
    private readonly Mock<IWarehouseTransferRepository> _transferRepositoryMock;
    private readonly ApproveWarehouseTransferCommandHandler _handler;

    // Reusable pending transfer — safe to approve by default.
    private readonly WarehouseTransferEntity _pendingTransfer;

    public ApproveWarehouseTransferHandlerTests()
    {
        _transferRepositoryMock = new Mock<IWarehouseTransferRepository>();

        _handler = new ApproveWarehouseTransferCommandHandler(
            _transferRepositoryMock.Object
        );

        _pendingTransfer = new WarehouseTransferEntity
        {
            Id = Guid.NewGuid(),
            TransferCode = "TRF-20260101-ABCDEF",
            Status = TransferStatus.Pending,
            ApprovedBy = null,
            ApprovedAt = null,
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

        var command = new ApproveWarehouseTransferCommand(id, Guid.NewGuid());

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

        var command = new ApproveWarehouseTransferCommand(Guid.NewGuid(), Guid.NewGuid());

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
    // GROUP 2 — STATUS GUARD (only Pending may be approved)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(TransferStatus.Approved)]
    [InlineData(TransferStatus.Rejected)]
    [InlineData(TransferStatus.Completed)]
    [InlineData(TransferStatus.Cancelled)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTransferIsNotPending(TransferStatus nonPendingStatus)
    {
        // Arrange
        _pendingTransfer.Status = nonPendingStatus;
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{nonPendingStatus}*");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChangesAsync_WhenTransferIsNotPending()
    {
        // Arrange
        _pendingTransfer.Status = TransferStatus.Approved;
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, Guid.NewGuid());

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
    // GROUP 3 — SUCCESSFUL APPROVAL
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldReturnResult_WithApprovedStatus_WhenRequestIsValid()
    {
        // Arrange
        var approvedBy = Guid.NewGuid();
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, approvedBy);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_pendingTransfer.Id);
        result.TransferCode.Should().Be("TRF-20260101-ABCDEF");
        result.Status.Should().Be(TransferStatus.Approved);
        result.ApprovedBy.Should().Be(approvedBy);
        result.ApprovedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldMutateTransferEntity_WithApprovedValues()
    {
        // Arrange
        var approvedBy = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, approvedBy);

        // Act
        await _handler.Handle(command, default);

        // Assert — entity mutated in place before SaveChanges
        _pendingTransfer.Status.Should().Be(TransferStatus.Approved);
        _pendingTransfer.ApprovedBy.Should().Be(approvedBy);
        _pendingTransfer.ApprovedAt.Should().NotBeNull();
        _pendingTransfer.ApprovedAt!.Value.Should().BeOnOrAfter(before.UtcDateTime);
        _pendingTransfer.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // -----------------------------------------------------------------------
    // GROUP 4 — REPOSITORY INTERACTION VERIFICATION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, Guid.NewGuid());

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
        var command = new ApproveWarehouseTransferCommand(_pendingTransfer.Id, Guid.NewGuid());

        // Act
        await _handler.Handle(command, default);

        // Assert
        _transferRepositoryMock.Verify(
            x => x.GetByIdAsync(_pendingTransfer.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}