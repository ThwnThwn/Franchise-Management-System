using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Commands.RedeemVoucher;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class RedeemVoucherHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyRepoMock;
    private readonly Mock<IVoucherQueueRepository> _queueRepoMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<IEngagementUow> _uowMock;
    private readonly Mock<IPointRuleCache> _pointRuleCacheMock;
    private readonly RedeemVoucherHandler _handler;

    public RedeemVoucherHandlerTests()
    {
        _loyaltyRepoMock = new Mock<ILoyaltyAccountRepository>();
        _queueRepoMock = new Mock<IVoucherQueueRepository>();
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _uowMock = new Mock<IEngagementUow>();
        _pointRuleCacheMock = new Mock<IPointRuleCache>();

        _handler = new RedeemVoucherHandler(
            _loyaltyRepoMock.Object,
            _queueRepoMock.Object,
            _voucherRepoMock.Object,
            _uowMock.Object,
            _pointRuleCacheMock.Object);
    }

    // 1. Happy Path: Đổi voucher thành công
    [Fact]
    public async Task Handle_ShouldRedeemSuccessfully_WhenAllValidationsPass()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        var command = new RedeemVoucherCommand(request);

        var account = new LoyaltyAccount { Id = request.LoyaltyAccountId, AvailablePoints = 100, UserVouchers = new List<UserVoucher>() };
        var voucher = new Voucher { Id = request.VoucherId, IsActive = true, PointCost = 50, CurrentQuantity = 10 };

        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);
        _queueRepoMock.Setup(x => x.CountVoucherInQueue(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 2. Phủ nhánh: Rollback khi có lỗi xảy ra
    [Fact]
    public async Task Handle_ShouldRollback_WhenExceptionOccurs()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database Error"));

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- BỔ SUNG CÁC CASE VALIDATION ĐỂ PHỦ KÍN SONARQUBE ---

    // 3. Validation: Account null (Dòng 48-49)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenAccountNotFound()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((LoyaltyAccount)null!);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage(LoyaltyAccountMsg.NotFound);
    }

    // 4. Validation: Voucher null (Dòng 51-52)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherNotFound()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(new LoyaltyAccount());
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync((Voucher)null!);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.NotFound);
    }

    // 5. Validation: Voucher Inactive (Dòng 54-55)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherIsInactive()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(new LoyaltyAccount());
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync(new Voucher { IsActive = false });

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(VoucherMsg.InactiveVoucher);
    }

    // 6. Validation: Voucher hết hạn
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherExpired()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        var account = new LoyaltyAccount();
        var voucher = new Voucher { IsActive = true, EndDate = DateTimeOffset.UtcNow.AddDays(-1) };

        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(VoucherMsg.ExpiredVoucher);
    }

    // 7. Validation: Quá số lần sử dụng
    [Fact]
    public async Task Handle_ShouldThrowException_WhenUsageLimitReached()
    {
        var voucherId = Guid.NewGuid();
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = voucherId };
        var voucher = new Voucher { Id = voucherId, IsActive = true, MaxUsagePerUser = 1 };
        var account = new LoyaltyAccount { UserVouchers = new List<UserVoucher> { new UserVoucher { VoucherId = voucherId } } };

        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(voucherId, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(VoucherMsg.UsageLimitReached);
    }

    // 8. Validation: Hết hàng (Theory cho cả 2 nhánh của IF/ELSE Stock)
    [Theory]
    [InlineData(0, 0)] // Hết hàng trực tiếp
    [InlineData(5, 5)] // Hết hàng do hàng đợi
    public async Task Handle_ShouldThrowException_WhenVoucherOutOfStock(int currentQty, int queueAmount)
    {
        var voucherId = Guid.NewGuid();
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = voucherId };
        var account = new LoyaltyAccount { AvailablePoints = 1000, UserVouchers = new List<UserVoucher>() };
        var voucher = new Voucher { Id = voucherId, IsActive = true, CurrentQuantity = currentQty, PointCost = 10 };

        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(voucherId, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);
        _queueRepoMock.Setup(x => x.CountVoucherInQueue(voucherId, It.IsAny<CancellationToken>())).ReturnsAsync(queueAmount);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(VoucherMsg.OutOfStock);
    }

    // 9. Validation: Thiếu điểm
    [Fact]
    public async Task Handle_ShouldThrowException_WhenInsufficientPoints()
    {
        var request = new RedeemVoucherRequest { LoyaltyAccountId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        var account = new LoyaltyAccount { AvailablePoints = 10 };
        var voucher = new Voucher { IsActive = true, PointCost = 100 };

        _loyaltyRepoMock.Setup(x => x.GetForUpdateAsync(request.LoyaltyAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _voucherRepoMock.Setup(x => x.GetForUpdateAsync(request.VoucherId, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);

        var act = () => _handler.Handle(new RedeemVoucherCommand(request), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(LoyaltyAccountMsg.InsufficientPoints);
    }
}