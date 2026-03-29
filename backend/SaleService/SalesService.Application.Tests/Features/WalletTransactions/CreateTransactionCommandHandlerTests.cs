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
using System.Threading;
using System.Threading.Tasks;
using static SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction.CreateTransactionCommand;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IRefundAuthorizationService> _authService = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(
            _transactionRepo.Object,
            _walletRepo.Object,
            _authService.Object,
            _identityClient.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_Wallet_NotExist()
    {
        var walletId = Guid.NewGuid();
        _walletRepo.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var request = new CreateTransactionRequest { WalletId = walletId };
        var command = new CreateTransactionCommand(Guid.NewGuid(), request, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenDeposit_Completed()
    {
        var wallet = new Wallet { Id = Guid.NewGuid(), Balance = 100, Status = WalletStatus.Active, UserId = Guid.NewGuid(), RowVersion = new byte[] { 1 } };
        _walletRepo.Setup(x => x.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _transactionRepo.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var request = new CreateTransactionRequest { WalletId = wallet.Id, Amount = 50, TransactionType = TransactionType.Deposit };
        var command = new CreateTransactionCommand(wallet.UserId, request, new HasAdminOverride { AutoComplete = true });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.BalanceAfter.Should().Be(150);
    }

    [Fact]
    public async Task Handle_Should_ReturnIdempotentSuccess_When_ExternalId_Exists()
    {
        var wallet = new Wallet { Id = Guid.NewGuid(), Balance = 100, Status = WalletStatus.Active, UserId = Guid.NewGuid() };
        var existing = new WalletTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, Amount = 50, TransactionType = TransactionType.Deposit, TransactionCode = "EXISTING" };
        
        _walletRepo.Setup(x => x.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetByExternalTransactionIdAsync("EXT-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new CreateTransactionRequest { WalletId = wallet.Id, ExternalTransactionId = "EXT-1" };
        var command = new CreateTransactionCommand(wallet.UserId, request, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCode.Should().Be("EXISTING");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_InsufficientBalance()
    {
        var wallet = new Wallet { Id = Guid.NewGuid(), Balance = 10, Status = WalletStatus.Active, UserId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var request = new CreateTransactionRequest { WalletId = wallet.Id, Amount = 50, TransactionType = TransactionType.Payment };
        var command = new CreateTransactionCommand(wallet.UserId, request, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(x => x.Identifier == "Balance");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_DailyLimitExceeded()
    {
        var wallet = new Wallet { Id = Guid.NewGuid(), Balance = 1000000000, Status = WalletStatus.Active, UserId = Guid.NewGuid() };
        _walletRepo.Setup(x => x.GetByIdAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        _transactionRepo.Setup(x => x.GetTotalAmountAsync(wallet.Id, It.IsAny<TransactionType>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100_000_000); // Daily limit is 100M

        var request = new CreateTransactionRequest { WalletId = wallet.Id, Amount = 1, TransactionType = TransactionType.Payment };
        var command = new CreateTransactionCommand(wallet.UserId, request, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(x => x.Identifier == "DailyLimit");
    }

    [Fact]
    public async Task Handle_Should_ProcessTransfer_Successfully()
    {
        var sourceWallet = new Wallet { Id = Guid.NewGuid(), Balance = 1000, Status = WalletStatus.Active, UserId = Guid.NewGuid(), RowVersion = new byte[] { 1 } };
        var destWallet = new Wallet { Id = Guid.NewGuid(), Balance = 500, Status = WalletStatus.Active, UserId = Guid.NewGuid(), RowVersion = new byte[] { 2 } };

        _walletRepo.Setup(x => x.GetByIdAsync(sourceWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sourceWallet);
        _walletRepo.Setup(x => x.GetByIdAsync(destWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(destWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(destWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(destWallet);
        _identityClient.Setup(x => x.GetUserByIdAsync(sourceWallet.UserId)).ReturnsAsync(new UserInfo { Id = sourceWallet.UserId });
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _transactionRepo.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var request = new CreateTransactionRequest { 
            WalletId = sourceWallet.Id, 
            Amount = 100, 
            TransactionType = TransactionType.Transfer,
            DestinationWalletId = destWallet.Id 
        };
        var command = new CreateTransactionCommand(sourceWallet.UserId, request, new HasAdminOverride { AutoComplete = true });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
