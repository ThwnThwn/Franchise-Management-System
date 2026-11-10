using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.Authorization;
using SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;
using SalesService.Application.Interfaces.Identity;
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
using static SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction.CreateTransactionCommand;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class CreateTransactionCommandHandlerTest
{
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IRefundAuthorizationService> _authService = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTest()
    {
        _handler = new CreateTransactionCommandHandler(
            _transactionRepo.Object,
            _walletRepo.Object,
            _authService.Object,
            _identityClient.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_WalletMissing()
    {
        var request = new CreateTransactionRequest { WalletId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetByIdAsync(request.WalletId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(new CreateTransactionCommand(Guid.NewGuid(), request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_NotOwner()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var request = new CreateTransactionRequest { WalletId = walletId };
        var result = await _handler.Handle(new CreateTransactionCommand(Guid.NewGuid(), request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_Idempotent()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var extId = "EXT_1";
        var wallet = new Wallet { Id = walletId, UserId = userId, Status = WalletStatus.Active };
        var existingTxn = new WalletTransaction { Id = Guid.NewGuid(), ExternalTransactionId = extId, WalletId = walletId, UserId = userId, TransactionCode = "T1" };

        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetByExternalTransactionIdAsync(extId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTxn);

        var request = new CreateTransactionRequest { WalletId = walletId, ExternalTransactionId = extId };
        var result = await _handler.Handle(new CreateTransactionCommand(userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(existingTxn.Id);
    }

    [Fact]
    public async Task Handle_Should_Success_Payment_With_WalletUpdate()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Status = WalletStatus.Active, Balance = 1000, RowVersion = new byte[] { 1 } };

        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.UpdateBalanceAsync(walletId, It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _transactionRepo.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var request = new CreateTransactionRequest { WalletId = walletId, Amount = 100, TransactionType = TransactionType.Payment };
        var result = await _handler.Handle(new CreateTransactionCommand(userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        _walletRepo.Verify(x => x.UpdateBalanceAsync(walletId, 900, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_BalanceUpdateFails()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = walletId, UserId = userId, Status = WalletStatus.Active, Balance = 1000, RowVersion = new byte[] { 1 } };

        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new CreateTransactionRequest { WalletId = walletId, Amount = 100, TransactionType = TransactionType.Payment };
        var result = await _handler.Handle(new CreateTransactionCommand(userId, request), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_Transfer()
    {
        var sourceWalletId = Guid.NewGuid();
        var destinationWalletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var sourceWallet = new Wallet { Id = sourceWalletId, UserId = userId, Status = WalletStatus.Active, Balance = 1000, RowVersion = new byte[] { 1 } };
        var destWallet = new Wallet { Id = destinationWalletId, UserId = Guid.NewGuid(), Status = WalletStatus.Active, Balance = 500, RowVersion = new byte[] { 2 } };

        _walletRepo.Setup(x => x.GetByIdAsync(sourceWalletId, It.IsAny<CancellationToken>())).ReturnsAsync(sourceWallet);
        _walletRepo.Setup(x => x.GetByIdAsync(destinationWalletId, It.IsAny<CancellationToken>())).ReturnsAsync(destWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(destinationWalletId, It.IsAny<CancellationToken>())).ReturnsAsync(destWallet);
        
        _walletRepo.Setup(x => x.UpdateBalanceAsync(destinationWalletId, It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _transactionRepo.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var request = new CreateTransactionRequest { WalletId = sourceWalletId, Amount = 100, TransactionType = TransactionType.Transfer, DestinationWalletId = destinationWalletId };
        var result = await _handler.Handle(new CreateTransactionCommand(userId, request), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        _walletRepo.Verify(x => x.UpdateBalanceAsync(destinationWalletId, 600, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
