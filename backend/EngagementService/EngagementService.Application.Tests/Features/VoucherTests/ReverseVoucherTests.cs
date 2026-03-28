using EngagementService.Application.Constants;
using EngagementService.Application.Features.Voucher.Commands.ReverseVoucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class ReverseVoucherHandlerTests
{
    private readonly Mock<IVoucherUsageRepository> _voucherUsageRepoMock;
    private readonly Mock<IUserVoucherRepository> _userVoucherRepoMock;
    private readonly Mock<IEngagementUow> _uowMock;
    private readonly ReverseVoucherHandler _handler;

    public ReverseVoucherHandlerTests()
    {
        _voucherUsageRepoMock = new Mock<IVoucherUsageRepository>();
        _userVoucherRepoMock = new Mock<IUserVoucherRepository>();
        _uowMock = new Mock<IEngagementUow>();

        _handler = new ReverseVoucherHandler(
            _voucherUsageRepoMock.Object,
            _userVoucherRepoMock.Object,
            _uowMock.Object);
    }

    // 1. Phủ nhánh: voucherUsages null hoặc trống (Dòng 31-34)
    [Theory]
    [InlineData(false)] // Case: null
    [InlineData(true)]  // Case: Empty list
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenNoVoucherUsageFound(bool isEmptyList)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ReverseVoucherCommand(orderId);

        List<Domain.Entities.VoucherUsage>? voucherUsages = isEmptyList
            ? new List<Domain.Entities.VoucherUsage>()
            : null;

        _voucherUsageRepoMock.Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(voucherUsages!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage(VoucherMsg.NotFound);

        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 2. Phủ xanh Happy Path & Logic Transaction (Dòng 38-54)
    [Fact]
    public async Task Handle_ShouldReverseVouchersSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userVoucherId = Guid.NewGuid();
        var command = new ReverseVoucherCommand(orderId);

        var voucherUsages = new List<Domain.Entities.VoucherUsage>
        {
            new Domain.Entities.VoucherUsage { UserVoucherId = userVoucherId }
        };

        var userVoucher = new Domain.Entities.UserVoucher
        {
            Id = userVoucherId,
            Status = UserVoucherStatus.USED
        };

        _voucherUsageRepoMock.Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(voucherUsages);

        _userVoucherRepoMock.Setup(x => x.GetByIdAsync(userVoucherId))
            .ReturnsAsync(userVoucher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        userVoucher.Status.Should().Be(UserVoucherStatus.AVAILABLE);

        // FIX CS0854: Truyền tường minh CancellationToken vào Verify
        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

        _userVoucherRepoMock.Verify(x => x.UpdateRangeAsync(
            It.Is<List<Domain.Entities.UserVoucher>>(l => l.Contains(userVoucher)),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _voucherUsageRepoMock.Verify(x => x.DeleteRangeAsync(
            voucherUsages,
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Thêm It.IsAny<CancellationToken>() vào trong ngoặc
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 3. Phủ nhánh: uv == null trong vòng lặp (Dòng 45)
    [Fact]
    public async Task Handle_ShouldSkipUpdate_WhenUserVoucherNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ReverseVoucherCommand(orderId);
        var voucherUsages = new List<Domain.Entities.VoucherUsage> { new Domain.Entities.VoucherUsage() };

        _voucherUsageRepoMock.Setup(x => x.GetByOrderIdAsync(orderId)).ReturnsAsync(voucherUsages);
        _userVoucherRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.UserVoucher)null!);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userVoucherRepoMock.Verify(x => x.UpdateRangeAsync(
            It.Is<List<Domain.Entities.UserVoucher>>(l => l.Count == 0),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 4. Phủ nhánh: Exception & Rollback (Dòng 55-60)
    [Fact]
    public async Task Handle_ShouldRollback_WhenExceptionOccurs()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ReverseVoucherCommand(orderId);
        var voucherUsages = new List<Domain.Entities.VoucherUsage> { new Domain.Entities.VoucherUsage() };

        _voucherUsageRepoMock.Setup(x => x.GetByOrderIdAsync(orderId)).ReturnsAsync(voucherUsages);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}