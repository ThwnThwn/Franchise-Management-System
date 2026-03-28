using EngagementService.Application.EventHandler;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;
using SharedKernel.Events;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.EventHandlerTests;

public class RedeemCampaignVoucherIntegrationTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyRepoMock;
    private readonly Mock<IEngagementUow> _uowMock;
    private readonly RedeemCampaignVoucherEventHanlder _handler;

    public RedeemCampaignVoucherIntegrationTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _loyaltyRepoMock = new Mock<ILoyaltyAccountRepository>();
        _uowMock = new Mock<IEngagementUow>();

        _handler = new RedeemCampaignVoucherEventHanlder(
            _voucherRepoMock.Object,
            _loyaltyRepoMock.Object,
            _uowMock.Object);
    }

    // 1. Phủ nhánh: Voucher hoặc Loyalty null -> Rollback (Dòng 23-27)
    [Fact]
    public async Task Handle_ShouldRollback_WhenVoucherOrLoyaltyIsNull()
    {
        // Arrange
        var @event = new RedeemCampaignVoucherEvent { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(@event.VoucherId)).ReturnsAsync((Voucher)null!);

        // Act
        await _handler.Handle(@event);

        // Assert
        // FIX CS0854: Thêm It.IsAny<CancellationToken>()
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 2. Phủ nhánh: Đã tồn tại Voucher trong tài khoản -> Rollback (Dòng 29-33)
    [Fact]
    public async Task Handle_ShouldRollback_WhenVoucherAlreadyExistsInAccount()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = new RedeemCampaignVoucherEvent { UserId = userId, VoucherId = voucherId };

        var voucher = new Voucher { Id = voucherId };
        var loyalty = new LoyaltyAccount
        {
            Id = userId,
            UserVouchers = new List<UserVoucher> { new UserVoucher { VoucherId = voucherId } }
        };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId)).ReturnsAsync(voucher);
        _loyaltyRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(loyalty);

        // Act
        await _handler.Handle(@event);

        // Assert
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 3. Happy Path: Redeem thành công (Dòng 35-46)
    [Fact]
    public async Task Handle_ShouldRedeemSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var voucherId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = new RedeemCampaignVoucherEvent { UserId = userId, VoucherId = voucherId };

        var voucher = new Voucher { Id = voucherId, CurrentQuantity = 10 };
        var loyalty = new LoyaltyAccount { Id = userId, UserVouchers = new List<UserVoucher>() };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(voucherId)).ReturnsAsync(voucher);
        _loyaltyRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(loyalty);

        // Act
        await _handler.Handle(@event);

        // Assert
        voucher.CurrentQuantity.Should().Be(9);
        loyalty.UserVouchers.Should().HaveCount(1);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 4. Phủ nhánh: Exception -> Rollback và Throw (Dòng 48-52)
    [Fact]
    public async Task Handle_ShouldRollbackAndThrow_WhenExceptionOccurs()
    {
        // Arrange
        var @event = new RedeemCampaignVoucherEvent { UserId = Guid.NewGuid() };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = () => _handler.Handle(@event);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}