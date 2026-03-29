using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class CustomerCreateRefundTicketCommandHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly CustomerCreateRefundTicketCommandHandler _handler;

    public CustomerCreateRefundTicketCommandHandlerTests()
    {
        _handler = new CustomerCreateRefundTicketCommandHandler(
            _refundRepoMock.Object, _walletRepoMock.Object, _txnRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountIsZero()
    {
        var cmd = new CustomerCreateRefundTicketCommand(Guid.NewGuid(),
            new CustomerCreateRefundTicketRequest { Amount = 0, Reason = "Test" });

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenWalletNotFound()
    {
        var cmd = new CustomerCreateRefundTicketCommand(Guid.NewGuid(),
            new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "Test" });

        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("wallet not found"));
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenBalanceInsufficient()
    {
        var customerId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = customerId, Balance = 50 };
        var cmd = new CustomerCreateRefundTicketCommand(customerId,
            new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "Test" });

        _walletRepoMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Insufficient"));
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var customerId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = customerId, Balance = 500 };
        var cmd = new CustomerCreateRefundTicketCommand(customerId,
            new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "Refund reason" });

        _walletRepoMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _refundRepoMock.Setup(x => x.AddAsync(It.IsAny<RefundTicket>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100);
        result.Value.Status.Should().Be(RefundTicketStatus.Pending);
        wallet.Balance.Should().Be(400);
        _walletRepoMock.Verify(x => x.UpdateAsync(wallet, It.IsAny<CancellationToken>()), Times.Once);
        _txnRepoMock.Verify(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
