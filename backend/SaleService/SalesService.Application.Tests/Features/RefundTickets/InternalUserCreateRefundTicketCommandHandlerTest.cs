using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.Common.Models;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund.InternalUserCreateRefundTicketCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class InternalUserCreateRefundTicketCommandHandlerTest
{
    private readonly Mock<IRefundTicketRepository> _refundRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly InternalUserCreateRefundTicketCommandHandler _handler;

    public InternalUserCreateRefundTicketCommandHandlerTest()
    {
        _handler = new InternalUserCreateRefundTicketCommandHandler(
            _refundRepo.Object,
            _walletRepo.Object,
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_OverrideInfoMissing()
    {
        var req = new InternalUserCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var command = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), req, null!);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_AmountZero()
    {
        var req = new InternalUserCreateRefundTicketRequest { Amount = 0, Reason = "test" };
        var overrideInfo = new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid() };
        var command = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), req, overrideInfo);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_WalletMissing()
    {
        var userId = Guid.NewGuid();
        var req = new InternalUserCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var overrideInfo = new InternalUserOverrideInfo { InternalUserId = Guid.NewGuid() };
        var command = new InternalUserCreateRefundTicketCommand(userId, req, overrideInfo);
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_AutoApprove()
    {
        var userId = Guid.NewGuid();
        var req = new InternalUserCreateRefundTicketRequest { Amount = 100, Reason = "test" };
        var overrideInfo = new InternalUserOverrideInfo 
        { 
            InternalUserId = Guid.NewGuid(), 
            AutoApprove = true 
        };
        var command = new InternalUserCreateRefundTicketCommand(userId, req, overrideInfo);
        var wallet = new Wallet { Id = Guid.NewGuid(), Balance = 200, Status = WalletStatus.Active };
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _refundRepo.Setup(x => x.AddAsync(It.IsAny<RefundTicket>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(100);
    }
}
