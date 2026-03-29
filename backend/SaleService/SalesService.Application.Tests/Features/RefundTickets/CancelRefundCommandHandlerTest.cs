using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Command.CancelRefund;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CancelRefund.CancelRefundCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class CancelRefundCommandHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly CancelRefundCommandHandler _handler;

    public CancelRefundCommandHandlerTests()
    {
        _handler = new CancelRefundCommandHandler(_refundRepoMock.Object, _walletRepoMock.Object, _txnRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTicketNotFound()
    {
        var cmd = new CancelRefundCommand(Guid.NewGuid());
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(cmd.RefundTicketId, It.IsAny<CancellationToken>())).ReturnsAsync((RefundTicket?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenTicketNotPending()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Approved };
        var cmd = new CancelRefundCommand(ticket.Id);
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenWalletNotFound()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Pending, UserId = Guid.NewGuid(), Amount = 100 };
        var cmd = new CancelRefundCommand(ticket.Id);
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ticket.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), TicketCode = "RFT-001", Status = RefundTicketStatus.Pending, UserId = Guid.NewGuid(), Amount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = ticket.UserId, Balance = 500 };
        var cmd = new CancelRefundCommand(ticket.Id, new CancelRefundRequest { CancellationReason = "Changed my mind" });

        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ticket.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RefundTicketStatus.Cancelled);
        result.Value.CancellationReason.Should().Be("Changed my mind");
        wallet.Balance.Should().Be(600); // restored
        _refundRepoMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        _txnRepoMock.Verify(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
