namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class ApproveWarehouseTransferTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly ApproveWarehouseTransferCommandHandler _handler;

    public ApproveWarehouseTransferTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _handler = new ApproveWarehouseTransferCommandHandler(_warehouseTransferRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApproveTransfer_WhenStatusIsPending()
    {
        var transferId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ARABICA-DAKLAK-001",
            Status = TransferStatus.Pending,
            TransferItems = [],
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new ApproveWarehouseTransferCommand(transferId, adminId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransferStatus.Approved);
        result.ApprovedBy.Should().Be(adminId);
        transfer.Status.Should().Be(TransferStatus.Approved);
        transfer.ApprovedBy.Should().Be(adminId);

        _warehouseTransferRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        var transferId = Guid.NewGuid();
        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransfer?)null);

        var command = new ApproveWarehouseTransferCommand(transferId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse transfer with ID '{transferId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsNotPending()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ROBUSTA-HCM-002",
            Status = TransferStatus.Completed // Đã hoàn thành, không thể duyệt lại
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new ApproveWarehouseTransferCommand(transferId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Only pending transfers can be approved. Current status: {TransferStatus.Completed}.");
    }
}