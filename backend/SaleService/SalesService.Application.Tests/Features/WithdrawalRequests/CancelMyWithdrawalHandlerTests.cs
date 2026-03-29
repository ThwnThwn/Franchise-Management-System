using FluentAssertions;
using Moq;
using SalesService.Application.Features.WithdrawalRequests.Commands.CancelMyWithdrawal;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class CancelMyWithdrawalHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWithdrawalRequestRepository> _withdrawalRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly CancelMyWithdrawalHandler _handler;

    public CancelMyWithdrawalHandlerTests()
    {
        _handler = new CancelMyWithdrawalHandler(
            _uow.Object,
            _walletRepo.Object,
            _withdrawalRepo.Object,
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowKeyNotFound_When_Withdrawal_NotExist()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _withdrawalRepo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WithdrawalRequest?)null);

        var command = new CancelMyWithdrawalCommand(requestId, userId, "reason");
        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperation_When_Status_NotPending()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest { Id = requestId, UserId = userId, Status = WithdrawalStatus.Approved };
        _withdrawalRepo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);

        var command = new CancelMyWithdrawalCommand(requestId, userId, "reason");
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Success_When_Valid()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest { Id = requestId, UserId = userId, Status = WithdrawalStatus.Pending, Amount = 100, RequestCode = "WDR1" };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 500, RowVersion = new byte[] { 1 } };

        _withdrawalRepo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var command = new CancelMyWithdrawalCommand(requestId, userId, "Changing mind");
        
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _withdrawalRepo.Verify(x => x.ApplyCancel(withdrawal, userId, "Changing mind"), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionRepo.Verify(x => x.Add(It.IsAny<WalletTransaction>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_On_ConcurrencyError()
    {
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest { Id = requestId, UserId = userId, Status = WithdrawalStatus.Pending, Amount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 500, RowVersion = new byte[] { 1 } };

        _withdrawalRepo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        _uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var command = new CancelMyWithdrawalCommand(requestId, userId, "reason");
        
        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        ex.Message.Should().NotBeNull();
        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
