using Moq;
using FluentAssertions;
using SalesService.Application.Common.Models;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class InternalUserCreateRefundTicketHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly InternalUserCreateRefundTicketCommandHandler _handler;

    public InternalUserCreateRefundTicketHandlerTests()
    {
        _handler = new InternalUserCreateRefundTicketCommandHandler(_refundRepoMock.Object, _walletRepoMock.Object, _txnRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOverrideIsNull()
    {
        var cmd = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), new InternalUserCreateRefundTicketRequest { Amount = 100 }, null);
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountZero()
    {
        var cmd = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), new InternalUserCreateRefundTicketRequest { Amount = 0 }, new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid() });
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletNotFound()
    {
        var userId = Guid.NewGuid();
        var cmd = new InternalUserCreateRefundTicketCommand(userId, new InternalUserCreateRefundTicketRequest { Amount = 100 }, new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid() });
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenWalletNotActive()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Status = WalletStatus.Locked, Balance = 500 };
        var cmd = new InternalUserCreateRefundTicketCommand(userId, new InternalUserCreateRefundTicketRequest { Amount = 100 }, new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid() });
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAutoApprove()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Status = WalletStatus.Active, Balance = 500 };
        var overrideInfo = new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid(), AutoApprove = true };
        var cmd = new InternalUserCreateRefundTicketCommand(userId, new InternalUserCreateRefundTicketRequest { Amount = 100, Reason = "Admin refund" }, overrideInfo);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _refundRepoMock.Setup(x => x.AddAsync(It.IsAny<RefundTicket>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RefundTicketStatus.Approved);
        wallet.Balance.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPendingNoAutoApprove()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Status = WalletStatus.Active, Balance = 500 };
        var overrideInfo = new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid(), AutoApprove = false };
        var cmd = new InternalUserCreateRefundTicketCommand(userId, new InternalUserCreateRefundTicketRequest { Amount = 100, Reason = "Reason" }, overrideInfo);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _refundRepoMock.Setup(x => x.AddAsync(It.IsAny<RefundTicket>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RefundTicketStatus.Pending);
        wallet.Balance.Should().Be(500); // no deduction when not auto-approved
    }
}
