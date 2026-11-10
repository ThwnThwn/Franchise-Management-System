namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class CancelWarehouseTransferTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly CancelWarehouseTransferCommandHandler _handler;

    public CancelWarehouseTransferTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _handler = new CancelWarehouseTransferCommandHandler(_warehouseTransferRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelTransfer_WhenStatusIsPending()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-MOKA-001",
            Status = TransferStatus.Pending,
            Reason = "Lệnh xuất hàng đầu tuần",
            TransferItems = []
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new CancelWarehouseTransferCommand(transferId, "Sai số lượng hạt Moka", Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransferStatus.Cancelled);
        result.Reason.Should().Be("Sai số lượng hạt Moka");
        transfer.Status.Should().Be(TransferStatus.Cancelled);

        _warehouseTransferRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelTransfer_WhenStatusIsApproved()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ROBUSTA-002",
            Status = TransferStatus.Approved,
            TransferItems = []
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new CancelWarehouseTransferCommand(transferId, "Thay đổi kế hoạch nhập hàng", Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransferStatus.Cancelled);
        transfer.Status.Should().Be(TransferStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldKeepExistingReason_WhenRequestReasonIsNull()
    {
        var transferId = Guid.NewGuid();
        var originalReason = "Lịch trình xe tải chở hạt cà phê bị hủy";
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ARABICA-003",
            Status = TransferStatus.Pending,
            Reason = originalReason, // Lý do gốc đã có sẵn
            TransferItems = []
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        // Truyền null vào Reason để test dòng toán tử ??
        var command = new CancelWarehouseTransferCommand(transferId, null, Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Lý do phải giữ nguyên như ban đầu
        result.Status.Should().Be(TransferStatus.Cancelled);
        result.Reason.Should().Be(originalReason);
        transfer.Reason.Should().Be(originalReason);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        var transferId = Guid.NewGuid();
        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransfer?)null);

        var command = new CancelWarehouseTransferCommand(transferId, "Hủy", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Warehouse transfer with ID '{transferId}' was not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenStatusIsCompleted()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-MILK-004",
            Status = TransferStatus.Completed
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new CancelWarehouseTransferCommand(transferId, "Hủy lệnh đã xong", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Transfer cannot be cancelled. Current status: {TransferStatus.Completed}.");
    }
}