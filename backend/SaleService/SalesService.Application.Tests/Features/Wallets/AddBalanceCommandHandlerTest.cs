using Ardalis.Result;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class AddBalanceCommandHandlerTest
{
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly AddBalanceCommandHandler _handler;

    public AddBalanceCommandHandlerTest()
    {
        _handler = new AddBalanceCommandHandler(_walletRepo.Object, _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_AmountLeZero()
    {
        var request = new AddBalanceCommandRequest { Amount = 0 };
        var result = await _handler.Handle(new AddBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), request), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_WalletMissing()
    {
        var walletId = Guid.NewGuid();
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var request = new AddBalanceCommandRequest { Amount = 100 };
        var result = await _handler.Handle(new AddBalanceCommand(walletId, Guid.NewGuid(), request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_NotOwner()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new AddBalanceCommandRequest { Amount = 100 };
        var result = await _handler.Handle(new AddBalanceCommand(walletId, Guid.NewGuid(), request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_WalletInactive()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Status = WalletStatus.Locked };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new AddBalanceCommandRequest { Amount = 100 };
        var result = await _handler.Handle(new AddBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_Valid()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 200, Status = WalletStatus.Active, RowVersion = new byte[] { 1 } };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new AddBalanceCommandRequest { Amount = 50, RowVersion = new byte[] { 1 } };
        var result = await _handler.Handle(new AddBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.BalanceAfter.Should().Be(250);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_Idempotent_When_ExistingTxn()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var extId = "EXT1";
        var existingTxn = new WalletTransaction 
        { 
            WalletId = walletId, UserId = userId, ExternalTransactionId = extId, 
            Amount = 50, BalanceBefore = 200, BalanceAfter = 250, 
            UpdatedAt = DateTimeOffset.UtcNow 
        };
        var wallet = new Wallet { Id = walletId, UserId = userId, RowVersion = new byte[] { 2 }, Status = WalletStatus.Active };

        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetByExternalTransactionIdAsync(extId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTxn);

        var request = new AddBalanceCommandRequest { Amount = 50, ExternalTransactionId = extId };
        var result = await _handler.Handle(new AddBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
    }
}
