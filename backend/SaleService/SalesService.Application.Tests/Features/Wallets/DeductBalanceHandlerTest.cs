using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.DeductBalance.DeductBalanceCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class DeductBalanceCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IWalletTransactionRepository> _transactionRepositoryMock;
    private readonly DeductBalanceCommandHandler _handler;

    private readonly Guid _walletId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly byte[] _rowVersion = [1, 2, 3, 4];

    public DeductBalanceCommandHandlerTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _transactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _handler = new DeductBalanceCommandHandler(
            _walletRepositoryMock.Object,
            _transactionRepositoryMock.Object);
    }

    private Wallet CreateActiveWallet(decimal balance = 200_000m)
    {
        return new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = balance,
            Status = WalletStatus.Active,
            RowVersion = _rowVersion
        };
    }

    // 1. THÀNH CÔNG - Trừ tiền thành công
    [Fact]
    public async Task Handle_ShouldDeductBalance_WhenAllValidationsPass()
    {
        // Arrange
        var wallet = CreateActiveWallet(200_000m);
        var updatedWallet = new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = 150_000m, // Sau khi trừ 50k
            Status = WalletStatus.Active,
            RowVersion = [5, 6, 7, 8]
        };

        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _walletRepositoryMock
            .Setup(x => x.UpdateBalanceAsync(_walletId, 150_000m, _rowVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _walletRepositoryMock
            .Setup(x => x.GetByIdAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedWallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BalanceBefore.Should().Be(200_000m);
        result.Value.BalanceAfter.Should().Be(150_000m);
        result.Value.AmountDeducted.Should().Be(50_000m);

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<WalletTransaction>(t =>
                t.Amount == -50_000m && // Amount lưu âm
                t.TransactionType == TransactionType.Payment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2. LỖI - Amount <= 0 → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = -100m,
            RowVersion = _rowVersion
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Amount must be greater than zero");
        _walletRepositoryMock.Verify(
            x => x.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 3. LỖI - Ví không tồn tại → NotFound
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletNotExist()
    {
        // Arrange
        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    // 4. LỖI - Ví không phải của user này → Forbidden
    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenWalletBelongsToAnotherUser()
    {
        // Arrange
        var wallet = new Wallet
        {
            Id = _walletId,
            UserId = Guid.NewGuid(), // Khác userId
            Balance = 200_000m,
            Status = WalletStatus.Active,
            RowVersion = _rowVersion
        };

        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    // 5. LỖI - Ví bị khóa (không Active) → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenWalletIsNotActive()
    {
        // Arrange
        var wallet = new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = 200_000m,
            Status = WalletStatus.Suspended,
            RowVersion = _rowVersion
        };

        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Wallet is not active"));
    }

    // 6. LỖI - Số dư không đủ → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenInsufficientBalance()
    {
        // Arrange
        var wallet = CreateActiveWallet(30_000m); // Chỉ có 30k
        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 100_000m, // Muốn trừ 100k
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Insufficient balance"));
    }

    // 7. LỖI - DbUpdateConcurrencyException → trả về Error
    [Fact]
    public async Task Handle_ShouldReturnError_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var command = new DeductBalanceCommand(_walletId, _userId, new DeductBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _walletRepositoryMock
            .Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        _transactionRepositoryMock
            .Setup(x => x.FailTransactionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Wallet was modified by another transaction"));

        _transactionRepositoryMock.Verify(
            x => x.FailTransactionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenExternalTransactionIdIsGeneric()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeductBalanceCommand(walletId, userId, new DeductBalanceCommandRequest
        {
            Amount = 100,
            ExternalTransactionId = "test" // Generic
        });

        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 500, Status = WalletStatus.Active };
        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenExternalTransactionIdExistsForDifferentWallet()
    {
        var walletId = Guid.NewGuid();
        var otherWalletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeductBalanceCommand(walletId, userId, new DeductBalanceCommandRequest
        {
            Amount = 100,
            ExternalTransactionId = "unique_123"
        });

        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 500, Status = WalletStatus.Active };
        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var existingTxn = new WalletTransaction { Id = Guid.NewGuid(), WalletId = otherWalletId, Amount = -100 };
        _transactionRepositoryMock.Setup(x => x.GetByExternalTransactionIdAsync("unique_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTxn);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_OnGenericException()
    {
        var walletId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeductBalanceCommand(walletId, userId, new DeductBalanceCommandRequest
        {
            Amount = 100,
            RowVersion = new byte[] { 1 }
        });

        var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 500, Status = WalletStatus.Active };
        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepositoryMock.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Generic error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Generic error"));
    }
}
