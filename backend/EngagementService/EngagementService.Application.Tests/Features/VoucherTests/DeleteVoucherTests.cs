using EngagementService.Application.Constants;
using EngagementService.Application.Features.Voucher.Commands.Delete;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using MediatR;
using Moq;

// Tuân thủ đúng Namespace Dynamic theo Guideline
namespace EngagementService.Application.Tests.Features.VoucherTests;

public class DeleteVoucherHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly DeleteVoucherHandler _handler;

    public DeleteVoucherHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _handler = new DeleteVoucherHandler(_voucherRepoMock.Object);
    }

    // 1. Phủ nhánh: voucher == null (Dòng 20)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenVoucherNotFound()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var command = new DeleteVoucherCommand(voucherId);

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync((Domain.Entities.Voucher)null!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage(VoucherMsg.NotFound); // Tận dụng Constant có sẵn

        _voucherRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // 2. Phủ nhánh: voucher.IsActive == true (Dòng 22-23)
    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenVoucherIsActive()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var command = new DeleteVoucherCommand(voucherId);
        var voucher = new Domain.Entities.Voucher { Id = voucherId, IsActive = true };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync(voucher);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(VoucherMsg.ActiveVoucher);

        _voucherRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // 3. Phủ nhánh: CurrentQuantity != InitQuantity (Dòng 25-26)
    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenVoucherIsAlreadyUsed()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var command = new DeleteVoucherCommand(voucherId);
        var voucher = new Domain.Entities.Voucher
        {
            Id = voucherId,
            IsActive = false,
            InitQuantity = 100,
            CurrentQuantity = 99 // Đã bị dùng 1 cái
        };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync(voucher);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.VoucherInUsed);

        _voucherRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // 4. Phủ xanh Happy Path (Dòng 28-29) - 100% Line Coverage
    [Fact]
    public async Task Handle_ShouldDeleteVoucher_WhenAllConditionsAreMet()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var command = new DeleteVoucherCommand(voucherId);
        var voucher = new Domain.Entities.Voucher
        {
            Id = voucherId,
            IsActive = false,
            InitQuantity = 100,
            CurrentQuantity = 100 // Chưa dùng cái nào
        };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId))
            .ReturnsAsync(voucher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _voucherRepoMock.Verify(x => x.DeleteAsync(voucherId), Times.Once);
    }
}