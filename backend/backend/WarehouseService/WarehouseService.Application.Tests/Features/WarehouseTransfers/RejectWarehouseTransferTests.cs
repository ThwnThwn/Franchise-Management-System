namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentAssertions;
using Moq;
using WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class RejectWarehouseTransferTests
{
    private readonly Mock<IWarehouseTransferRepository> _warehouseTransferRepositoryMock;
    private readonly RejectWarehouseTransferCommandHandler _handler;

    public RejectWarehouseTransferTests()
    {
        _warehouseTransferRepositoryMock = new Mock<IWarehouseTransferRepository>();
        _handler = new RejectWarehouseTransferCommandHandler(_warehouseTransferRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectTransfer_WhenStatusIsPending()
    {
        var transferId = Guid.NewGuid();
        var transfer = new WarehouseTransfer
        {
            Id = transferId,
            TransferCode = "TRF-ARABICA-GL-005",
            Status = TransferStatus.Pending,
            TransferItems = []
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new RejectWarehouseTransferCommand(transferId, "Chất lượng hạt Arabica không đạt chuẩn test mẫu", Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(TransferStatus.Rejected);
        result.Reason.Should().Be("Chất lượng hạt Arabica không đạt chuẩn test mẫu");
        transfer.Status.Should().Be(TransferStatus.Rejected);

        _warehouseTransferRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTransferDoesNotExist()
    {
        var transferId = Guid.NewGuid();
        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseTransfer?)null);

        var command = new RejectWarehouseTransferCommand(transferId, "Lỗi hệ thống", Guid.NewGuid());

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
            TransferCode = "TRF-ROBUSTA-HCM-009",
            Status = TransferStatus.Approved // Đã duyệt rồi thì không được reject (phải dùng Cancel)
        };

        _warehouseTransferRepositoryMock
            .Setup(x => x.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var command = new RejectWarehouseTransferCommand(transferId, "Từ chối đơn đã duyệt", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Only pending transfers can be rejected. Current status: {TransferStatus.Approved}.");
    }
}