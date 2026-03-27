using Moq;
using Xunit;
using FluentAssertions;
using WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Enums;
using WarehouseTransferEntity = WarehouseService.Domain.Entities.WarehouseTransfer;

namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

public class RejectWarehouseTransferHandlerTests
{
    private readonly Mock<IWarehouseTransferRepository> _transferRepositoryMock;
    private readonly RejectWarehouseTransferCommandHandler _handler;
    private readonly WarehouseTransferEntity _pendingTransfer;

    public RejectWarehouseTransferHandlerTests()
    {
        _transferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _handler = new RejectWarehouseTransferCommandHandler(_transferRepositoryMock.Object);

        // Setup a default valid pending transfer
        _pendingTransfer = new WarehouseTransferEntity
        {
            Id = Guid.NewGuid(),
            TransferCode = "TRF-2026-001",
            Status = TransferStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Default Mock Setup: Return the transfer
        _transferRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pendingTransfer);

        _transferRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
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

        var command = new RejectWarehouseTransferCommand(id, "No stock", Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    // -----------------------------------------------------------------------
    // GROUP 2 — STATUS GUARD
    // Logic: Only Pending status can be rejected.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(TransferStatus.Approved)]
    [InlineData(TransferStatus.Completed)]
    [InlineData(TransferStatus.Rejected)]
    [InlineData(TransferStatus.Cancelled)]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending(TransferStatus invalidStatus)
    {
        // Arrange
        _pendingTransfer.Status = invalidStatus;
        var command = new RejectWarehouseTransferCommand(_pendingTransfer.Id, "Reason", Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Only pending transfers can be rejected*");

        _transferRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // GROUP 3 — SUCCESSFUL REJECTION
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldUpdateStatusToRejected_AndSetReason_WhenValid()
    {
        // Arrange
        var rejectionReason = "Insufficient space in destination";
        var command = new RejectWarehouseTransferCommand(_pendingTransfer.Id, rejectionReason, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Status.Should().Be(TransferStatus.Rejected);
        result.Reason.Should().Be(rejectionReason);
        _pendingTransfer.Status.Should().Be(TransferStatus.Rejected);
        _pendingTransfer.Reason.Should().Be(rejectionReason);
        _pendingTransfer.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Handle_ShouldSaveAndReturnCorrectResultFields_WhenSuccessful()
    {
        // Arrange
        var command = new RejectWarehouseTransferCommand(_pendingTransfer.Id, "Rejected by Admin", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Id.Should().Be(_pendingTransfer.Id);
        result.TransferCode.Should().Be(_pendingTransfer.TransferCode);

        _transferRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldWork_WhenReasonIsNull()
    {
        // Arrange
        var command = new RejectWarehouseTransferCommand(_pendingTransfer.Id, null, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Reason.Should().BeNull();
        _pendingTransfer.Reason.Should().BeNull();
        _pendingTransfer.Status.Should().Be(TransferStatus.Rejected);
    }
}