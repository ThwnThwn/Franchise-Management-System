using Ardalis.Result;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
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
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class DeductBalanceCommandHandlerTest
{
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly DeductBalanceCommandHandler _handler;

    public DeductBalanceCommandHandlerTest()
    {
        _handler = new DeductBalanceCommandHandler(_walletRepo.Object, _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_AmountLeZero()
    {
        var request = new DeductBalanceCommandRequest { Amount = 0 };
        var result = await _handler.Handle(new DeductBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), request), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_User_NotOwner()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new DeductBalanceCommandRequest { Amount = 10 };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, Guid.NewGuid(), request), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_InsufficientBalance()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 50, Status = WalletStatus.Active };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new DeductBalanceCommandRequest { Amount = 100 };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.First().ErrorMessage.Should().Contain("Insufficient balance");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_ExternalId_IsGeneric()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 200, Status = WalletStatus.Active };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new DeductBalanceCommandRequest { Amount = 100, ExternalTransactionId = "test" };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
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

        var request = new DeductBalanceCommandRequest { Amount = 50, RowVersion = new byte[] { 1 } };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.BalanceAfter.Should().Be(150);
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
            Amount = -50, BalanceBefore = 200, BalanceAfter = 150, 
            TransactionCode = "CODE1", UpdatedAt = DateTimeOffset.UtcNow 
        };
        var wallet = new Wallet { Id = walletId, UserId = userId, RowVersion = new byte[] { 2 }, Status = WalletStatus.Active, Balance = 200 };

        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetByExternalTransactionIdAsync(extId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTxn);

        var request = new DeductBalanceCommandRequest { Amount = 50, ExternalTransactionId = extId };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCode.Should().Be("CODE1");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_ExistingTxn_DifferentWallet()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var extId = "EXT1";
        var existingTxn = new WalletTransaction { WalletId = Guid.NewGuid(), ExternalTransactionId = extId };
        var wallet = new Wallet { Id = walletId, UserId = userId, Status = WalletStatus.Active };

        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetByExternalTransactionIdAsync(extId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTxn);

        var request = new DeductBalanceCommandRequest { Amount = 50, ExternalTransactionId = extId };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_Handle_ConcurrencyException()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 200, Status = WalletStatus.Active, RowVersion = new byte[] { 1 } };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var request = new DeductBalanceCommandRequest { Amount = 50, RowVersion = new byte[] { 1 } };
        var result = await _handler.Handle(new DeductBalanceCommand(walletId, userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeFalse();
        _transactionRepo.Verify(x => x.FailTransactionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
