using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class CustomerCreateRefundTicketCommandHandlerTest
{
    private readonly Mock<IRefundTicketRepository> _refundRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly CustomerCreateRefundTicketCommandHandler _handler;

    public CustomerCreateRefundTicketCommandHandlerTest()
    {
        _handler = new CustomerCreateRefundTicketCommandHandler(
            _refundRepo.Object,
            _walletRepo.Object,
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_AmountZero()
    {
        var req = new CustomerCreateRefundTicketRequest { Amount = 0, Reason = "test" };
        var command = new CustomerCreateRefundTicketCommand(Guid.NewGuid(), req);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_WalletMissing()
    {
        var customerId = Guid.NewGuid();
        var req = new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var command = new CustomerCreateRefundTicketCommand(customerId, req);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_InsufficientBalance()
    {
        var customerId = Guid.NewGuid();
        var req = new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var command = new CustomerCreateRefundTicketCommand(customerId, req);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Balance = 50 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_Valid()
    {
        var customerId = Guid.NewGuid();
        var req = new CustomerCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var command = new CustomerCreateRefundTicketCommand(customerId, req);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Id = Guid.NewGuid(), Balance = 200 });
        _refundRepo.Setup(x => x.AddAsync(It.IsAny<RefundTicket>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
