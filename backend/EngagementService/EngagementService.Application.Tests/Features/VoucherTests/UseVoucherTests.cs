using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class UseVoucherHandlerTests
{
    private readonly Mock<IUserVoucherRepository> _userVoucherRepoMock;
    private readonly Mock<IVoucherUsageRepository> _voucherUsageRepoMock;
    private readonly Mock<IEngagementUow> _uowMock;
    private readonly UseVoucherHandler _handler;

    public UseVoucherHandlerTests()
    {
        _userVoucherRepoMock = new Mock<IUserVoucherRepository>();
        _voucherUsageRepoMock = new Mock<IVoucherUsageRepository>();
        _uowMock = new Mock<IEngagementUow>();

        _handler = new UseVoucherHandler(
            _userVoucherRepoMock.Object,
            _voucherUsageRepoMock.Object,
            _uowMock.Object);
    }

    // 1. Phủ nhánh: userVouchers null hoặc rỗng (Dòng 30-31)
    [Theory]
    [InlineData(false)] // Case: null
    [InlineData(true)]  // Case: count == 0
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenNoVoucherFound(bool isListEmpty)
    {
        // Arrange
        var request = new UseVouchersRequest { VoucherIds = new List<Guid> { Guid.NewGuid() }, UserId = Guid.NewGuid() };
        var command = new UseVoucherCommand(request);

        List<Domain.Entities.UserVoucher>? userVouchers = isListEmpty
            ? new List<Domain.Entities.UserVoucher>()
            : null;

        _userVoucherRepoMock.Setup(x => x.GetByVoucherIdsAsync(request.VoucherIds, request.UserId))
            .ReturnsAsync(userVouchers!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage(VoucherMsg.NotFound);

        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 2. Phủ xanh Happy Path & Logic Transaction (Dòng 33-52)
    [Fact]
    public async Task Handle_ShouldUseVouchersSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var request = new UseVouchersRequest
        {
            VoucherIds = new List<Guid> { Guid.NewGuid() },
            UserId = Guid.NewGuid(),
            OrderId = Guid.NewGuid()
        };
        var command = new UseVoucherCommand(request);

        var userVouchers = new List<Domain.Entities.UserVoucher>
        {
            new Domain.Entities.UserVoucher { Id = Guid.NewGuid(), VoucherId = request.VoucherIds[0], Status = UserVoucherStatus.AVAILABLE }
        };

        _userVoucherRepoMock.Setup(x => x.GetByVoucherIdsAsync(request.VoucherIds, request.UserId))
            .ReturnsAsync(userVouchers);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        userVouchers[0].Status.Should().Be(UserVoucherStatus.USED);

        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Kiểm tra UpdateRangeAsync (Lưu ý: Truyền CancellationToken tường minh để tránh CS0854)
        _userVoucherRepoMock.Verify(x => x.UpdateRangeAsync(
            userVouchers,
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Kiểm tra AddRangeAsync
        _voucherUsageRepoMock.Verify(x => x.AddRangeAsync(
            It.Is<List<Domain.Entities.VoucherUsage>>(l => l.Count == 1 && l[0].OrderId == request.OrderId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 3. Phủ nhánh: Exception & Rollback (Dòng 53-58)
    [Fact]
    public async Task Handle_ShouldRollback_WhenExceptionOccurs()
    {
        // Arrange
        var request = new UseVouchersRequest { VoucherIds = new List<Guid> { Guid.NewGuid() } };
        var command = new UseVoucherCommand(request);
        var userVouchers = new List<Domain.Entities.UserVoucher> { new Domain.Entities.UserVoucher() };

        _userVoucherRepoMock.Setup(x => x.GetByVoucherIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<Guid>()))
            .ReturnsAsync(userVouchers);

        // Ép SaveChangesAsync ném lỗi
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}