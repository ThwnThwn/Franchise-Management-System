using Moq;
using FluentAssertions;
using SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Application.Interfaces;
using SalesService.Application.Dtos;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests.Commands;

public class WithdrawalRequestsCommandsTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IWithdrawalRequestRepository> _repoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();

    private readonly ApproveWithdrawalHandler _approveHandler;
    private readonly RejectWithdrawalHandler _rejectHandler;
    private readonly CancelWithdrawalHandler _cancelHandler;
    private readonly CompleteWithdrawalHandler _completeHandler;

    public WithdrawalRequestsCommandsTests()
    {
        _approveHandler = new ApproveWithdrawalHandler(_uowMock.Object, _repoMock.Object);
        _rejectHandler = new RejectWithdrawalHandler(_uowMock.Object, _repoMock.Object, _walletRepoMock.Object, _transactionRepoMock.Object);
        _cancelHandler = new CancelWithdrawalHandler(_uowMock.Object, _repoMock.Object, _walletRepoMock.Object, _transactionRepoMock.Object);
        _completeHandler = new CompleteWithdrawalHandler(_uowMock.Object, _repoMock.Object, _imageServiceMock.Object);
    }

    [Fact]
    public async Task ApproveWithdrawal_ShouldSucceed_WhenPending()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var request = new ApproveWithdrawalCommand(requestId, adminId, "Approved notes");
        var withdrawal = new Domain.Entities.WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Pending };

        _repoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(withdrawal);

        var result = await _approveHandler.Handle(request, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(x => x.ApplyApprove(withdrawal, adminId, "Approved notes"), Times.Once());
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RejectWithdrawal_ShouldSucceed_AndRefundWallet()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var command = new RejectWithdrawalCommand(requestId, adminId, "Reason");
        var withdrawal = new Domain.Entities.WithdrawalRequest { Id = requestId, UserId = userId, Amount = 500, Status = WithdrawalStatus.Pending };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 1000, RowVersion = new byte[] { 1, 2, 3 } };

        _repoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(withdrawal);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _rejectHandler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(x => x.ApplyReject(withdrawal, adminId, "Reason"), Times.Once());
        _walletRepoMock.Verify(x => x.ApplyUpdateBalance(It.IsAny<Wallet>(), It.IsAny<decimal>(), It.IsAny<byte[]>()), Times.Once());
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CancelWithdrawal_ShouldSucceed_AndRefundWallet()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var command = new CancelWithdrawalCommand(requestId, adminId, "Reason");
        var withdrawal = new Domain.Entities.WithdrawalRequest { Id = requestId, UserId = userId, Amount = 500, Status = WithdrawalStatus.Approved };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 1000, RowVersion = new byte[] { 1, 2, 3 } };

        _repoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(withdrawal);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _cancelHandler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(x => x.ApplyCancel(withdrawal, adminId, "Reason"), Times.Once());
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CompleteWithdrawal_ShouldSucceed_WithImageUpload()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var receiptFile = new byte[] { 0, 1, 2 };
        var command = new CompleteWithdrawalCommand(requestId, adminId, receiptFile, "BANK123");
        var withdrawal = new Domain.Entities.WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Approved, RequestCode = "W-001" };

        _repoMock.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(withdrawal);
        _imageServiceMock.Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://image.url", PublicId = "pid" });

        var result = await _completeHandler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(x => x.ApplyComplete(withdrawal, adminId, It.IsAny<Guid>(), "BANK123", "http://image.url", "pid"), Times.Once());
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}
