using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class UpdateVoucherStatusHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly UpdateVoucherStatusHandler _handler;

    public UpdateVoucherStatusHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _handler = new UpdateVoucherStatusHandler(_voucherRepoMock.Object);
    }

    // 1. Phủ nhánh: voucher == null
    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenVoucherNotFound()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var request = new UpdateVoucherStatusRequest { Id = voucherId, IsActive = true };
        var command = new UpdateVoucherStatusCommand(request);

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync((Domain.Entities.Voucher)null!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.NotFound);

        _voucherRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Voucher>()), Times.Never);
    }

    // 2. Phủ nhánh: voucher.IsActive != updateRequest.IsActive (Cần cập nhật)
    [Fact]
    public async Task Handle_ShouldUpdateVoucher_WhenStatusIsDifferent()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var request = new UpdateVoucherStatusRequest { Id = voucherId, IsActive = true };
        var command = new UpdateVoucherStatusCommand(request);

        var voucher = new Domain.Entities.Voucher
        {
            Id = voucherId,
            IsActive = false
        };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync(voucher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        voucher.IsActive.Should().BeTrue();

        // FIX CS1501: Chỉ truyền 1 tham số theo đúng Interface của Duy
        _voucherRepoMock.Verify(x => x.UpdateAsync(
            It.Is<Domain.Entities.Voucher>(v => v.IsActive == true)),
            Times.Once);
    }

    // 3. Phủ nhánh: voucher.IsActive == updateRequest.IsActive (Không cần cập nhật)
    [Fact]
    public async Task Handle_ShouldNotCallUpdate_WhenStatusIsAlreadySame()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var request = new UpdateVoucherStatusRequest { Id = voucherId, IsActive = true };
        var command = new UpdateVoucherStatusCommand(request);

        var voucher = new Domain.Entities.Voucher
        {
            Id = voucherId,
            IsActive = true
        };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync(voucher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _voucherRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Voucher>()), Times.Never);
    }
}