using FluentAssertions;
using Moq;
using SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class CancelWithdrawalHandlerTest
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly CancelWithdrawalHandler _handler;

    public CancelWithdrawalHandlerTest()
    {
        _handler = new CancelWithdrawalHandler(
            _uow.Object, 
            _repo.Object, 
            _walletRepo.Object, 
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelAndRefund_WhenApproved()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cmd = new CancelWithdrawalCommand(requestId, adminId, "Reason");
        var withdrawal = new WithdrawalRequest { Id = requestId, UserId = userId, Amount = 100, Status = WithdrawalStatus.Approved };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 200, RowVersion = new byte[] { 1 } };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repo.Verify(x => x.ApplyCancel(withdrawal, adminId, "Reason"), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenStatusIsPending()
    {
        var requestId = Guid.NewGuid();
        var cmd = new CancelWithdrawalCommand(requestId, Guid.NewGuid(), "Reason");
        var withdrawal = new WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Pending };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);

        var act = () => _handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
