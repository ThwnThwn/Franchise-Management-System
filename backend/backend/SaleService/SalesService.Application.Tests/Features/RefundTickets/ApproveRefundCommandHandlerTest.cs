using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Command.ApproveRefund;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.ApproveRefund.ApproveRefundCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class ApproveRefundCommandHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityMock = new();
    private readonly ApproveRefundCommandHandler _handler;

    public ApproveRefundCommandHandlerTests()
    {
        _handler = new ApproveRefundCommandHandler(_refundRepoMock.Object, _walletRepoMock.Object, _txnRepoMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTicketNotFound()
    {
        var cmd = new ApproveRefundCommand(Guid.NewGuid(), new ApproveRefundRequest());
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(cmd.RefundTicketId, It.IsAny<CancellationToken>())).ReturnsAsync((RefundTicket?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenTicketNotPending()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Approved, UserId = Guid.NewGuid(), Amount = 100 };
        var cmd = new ApproveRefundCommand(ticket.Id, new ApproveRefundRequest());
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletNotFound()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Pending, UserId = Guid.NewGuid(), Amount = 100 };
        var cmd = new ApproveRefundCommand(ticket.Id, new ApproveRefundRequest());
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ticket.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenWalletNotActive()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Pending, UserId = Guid.NewGuid(), Amount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = ticket.UserId, Status = WalletStatus.Locked, Balance = 500 };
        var cmd = new ApproveRefundCommand(ticket.Id, new ApproveRefundRequest());
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ticket.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), TicketCode = "RFT-001", Status = RefundTicketStatus.Pending, UserId = Guid.NewGuid(), Amount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = ticket.UserId, Status = WalletStatus.Active, Balance = 500 };
        var cmd = new ApproveRefundCommand(ticket.Id, new ApproveRefundRequest { ApprovalNotes = "OK" });
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ticket.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RefundTicketStatus.Approved);
        result.Value.ApprovalNotes.Should().Be("OK");
        _refundRepoMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }
}
