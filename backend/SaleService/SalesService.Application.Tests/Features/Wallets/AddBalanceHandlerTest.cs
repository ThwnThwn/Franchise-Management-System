using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class AddBalanceCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IWalletTransactionRepository> _transactionRepositoryMock;
    private readonly AddBalanceCommandHandler _handler;

    // Fixture dữ liệu tái sử dụng
    private readonly Guid _walletId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly byte[] _rowVersion = [1, 2, 3, 4];

    public AddBalanceCommandHandlerTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _transactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _handler = new AddBalanceCommandHandler(
            _walletRepositoryMock.Object,
            _transactionRepositoryMock.Object);
    }

    private Wallet CreateActiveWallet()
    {
        return new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = 100_000m,
            Status = WalletStatus.Active,
            RowVersion = _rowVersion
        };
    }

    // 1. THÀNH CÔNG - Nạp tiền vào ví
    [Fact]
    public async Task Handle_ShouldAddBalance_WhenAllValidationsPass()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var updatedWallet = new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = 150_000m, // Sau khi cộng 50k
            Status = WalletStatus.Active,
            RowVersion = [5, 6, 7, 8]
        };

        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
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
        result.Value.BalanceBefore.Should().Be(100_000m);
        result.Value.BalanceAfter.Should().Be(150_000m);
        result.Value.AmountAdded.Should().Be(50_000m);
        result.Value.UserId.Should().Be(_userId);

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<WalletTransaction>(t =>
                t.Amount == 50_000m &&
                t.TransactionType == TransactionType.Deposit),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2. LỖI - Amount <= 0 → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
        {
            Amount = 0,
            RowVersion = _rowVersion
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Amount must be greater than zero");

        _walletRepositoryMock.Verify(x => x.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 3. LỖI - Ví không tồn tại → NotFound
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletNotExist()
    {
        // Arrange
        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
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
        var anotherUserId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = _walletId,
            UserId = anotherUserId, // Khác userId trong command
            Balance = 100_000m,
            Status = WalletStatus.Active,
            RowVersion = _rowVersion
        };

        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
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

    // 5. LỖI - Ví không Active → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenWalletIsNotActive()
    {
        // Arrange
        var wallet = new Wallet
        {
            Id = _walletId,
            UserId = _userId,
            Balance = 100_000m,
            Status = WalletStatus.Suspended, // Ví bị khóa
            RowVersion = _rowVersion
        };

        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
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

    // 6. LỖI - ExternalTransactionId là giá trị cấm ("test", "string", ...)
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenExternalTransactionIdIsInvalidValue()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion,
            ExternalTransactionId = "test" // Giá trị bị cấm
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage.Contains("ExternalTransactionId cannot be a generic test value"));
    }

    // 7. IDEMPOTENT - ExternalTransactionId đã tồn tại cùng wallet → trả về kết quả cũ (không gián thêm)
    [Fact]
    public async Task Handle_ShouldReturnIdempotentResult_WhenExternalTransactionIdAlreadyExistsForSameWallet()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var existingTxnId = "EXT-TXN-12345";
        var existingTxn = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = _walletId, // Cùng wallet
            UserId = _userId,
            Amount = 50_000m,
            BalanceBefore = 100_000m,
            BalanceAfter = 150_000m,
            ExternalTransactionId = existingTxnId
        };

        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion,
            ExternalTransactionId = existingTxnId
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _transactionRepositoryMock
            .Setup(x => x.GetByExternalTransactionIdAsync(existingTxnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTxn);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Trả về kết quả cũ, không giao dịch lần nữa
        result.IsSuccess.Should().BeTrue();
        result.Value.WalletId.Should().Be(_walletId);
        result.Value.AmountAdded.Should().Be(existingTxn.Amount);

        // Xác nhận KHÔNG tạo giao dịch mới
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 8. LỖI - ExternalTransactionId đã tồn tại nhưng thuộc wallet khác → Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenExternalTransactionIdBelongsToAnotherWallet()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var existingTxnId = "EXT-TXN-99999";
        var anotherWalletId = Guid.NewGuid();
        var existingTxn = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = anotherWalletId, // Wallet khác
            UserId = Guid.NewGuid(),
            Amount = 50_000m,
            ExternalTransactionId = existingTxnId
        };

        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
        {
            Amount = 50_000m,
            RowVersion = _rowVersion,
            ExternalTransactionId = existingTxnId
        });

        _walletRepositoryMock.Setup(x => x.GetForUpdateAsync(_walletId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _transactionRepositoryMock
            .Setup(x => x.GetByExternalTransactionIdAsync(existingTxnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTxn);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage.Contains($"ExternalTransactionId '{existingTxnId}' already exists for a different wallet"));
    }

    // 9. LỖI - DbUpdateConcurrencyException → trả về Error (conflict version)
    [Fact]
    public async Task Handle_ShouldReturnError_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var wallet = CreateActiveWallet();
        var command = new AddBalanceCommand(_walletId, _userId, new AddBalanceCommandRequest
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
}
