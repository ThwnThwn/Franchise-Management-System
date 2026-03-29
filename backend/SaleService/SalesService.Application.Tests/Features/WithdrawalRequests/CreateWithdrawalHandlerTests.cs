using Moq;
using FluentAssertions;
using SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;
using SalesService.Application.Interfaces;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class CreateWithdrawalHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWithdrawalRequestRepository> _withdrawalRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IAesEncryptionService> _aesMock = new();
    private readonly CreateWithdrawalHandler _handler;

    public CreateWithdrawalHandlerTests()
    {
        _handler = new CreateWithdrawalHandler(
            _uowMock.Object,
            _walletRepoMock.Object,
            _withdrawalRepoMock.Object,
            _transactionRepoMock.Object,
            _aesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenHasPendingWithdrawal()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100", "VCB", "Vietcombank", "Test Name", "123456", "Notes");
        _withdrawalRepoMock.Setup(x => x.HasPendingWithdrawalAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Bạn hiện đang có một yêu cầu rút tiền đang chờ xử lý.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenWalletNotFound()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100", "VCB", "Vietcombank", "Test Name", "123456", "Notes");
        _withdrawalRepoMock.Setup(x => x.HasPendingWithdrawalAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var act = () => _handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Đã xảy ra lỗi trong quá trình tạo yêu cầu rút tiền.");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateSuccessfully_WhenValid()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100000", "VCB", "Vietcombank", "Test Name", "123456", "Notes");
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = cmd.UserId, Balance = 200000, RowVersion = new byte[] { 1 } };

        _withdrawalRepoMock.Setup(x => x.HasPendingWithdrawalAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _aesMock.Setup(x => x.Encrypt(cmd.AccountNumber)).Returns("ENCRYPTED");
        _aesMock.Setup(x => x.GenerateMaskedAccountNumber(cmd.AccountNumber)).Returns("MASKED");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeEmpty();
        _walletRepoMock.Verify(x => x.ApplyDeductBalance(wallet, 100000, It.IsAny<byte[]>()), Times.Once);
        _withdrawalRepoMock.Verify(x => x.Add(It.IsAny<WithdrawalRequest>()), Times.Once);
        _transactionRepoMock.Verify(x => x.Add(It.IsAny<WalletTransaction>()), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenWalletMissingRowVersion()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100", "VCB", "Vietcombank", "Test Name", "123456", "Notes");
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = cmd.UserId, RowVersion = null };
 
        _withdrawalRepoMock.Setup(x => x.HasPendingWithdrawalAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
 
        var act = () => _handler.Handle(cmd, CancellationToken.None);
 
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Dữ liệu ví không hợp lệ (Thiếu Version).");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_OnConcurrencyError()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100", "VCB", "Vietcombank", "Test Name", "123456", "Notes");
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = cmd.UserId, Balance = 200, RowVersion = new byte[] { 1 } };
 
        _withdrawalRepoMock.Setup(x => x.HasPendingWithdrawalAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        
        _uowMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());
 
        var act = () => _handler.Handle(cmd, CancellationToken.None);
 
        var ex = await act.Should().ThrowAsync<Exception>();
        ex.WithMessage("Hệ thống bận*");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
