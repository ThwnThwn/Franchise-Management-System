using FluentAssertions;
using Moq;
using SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;
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

public class RejectWithdrawalHandlerTest
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly RejectWithdrawalHandler _handler;

    public RejectWithdrawalHandlerTest()
    {
        _handler = new RejectWithdrawalHandler(
            _uow.Object, 
            _repo.Object, 
            _walletRepo.Object, 
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectAndRefund_WhenValid()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cmd = new RejectWithdrawalCommand(requestId, adminId, "Reason");
        var withdrawal = new WithdrawalRequest { Id = requestId, UserId = userId, Amount = 100, Status = WithdrawalStatus.Pending };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 200, RowVersion = new byte[] { 1 } };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repo.Verify(x => x.ApplyReject(withdrawal, adminId, "Reason"), Times.Once);
        _walletRepo.Verify(x => x.ApplyAddBalance(wallet, 100, It.IsAny<byte[]>()), Times.Once);
        _transactionRepo.Verify(x => x.Add(It.IsAny<WalletTransaction>()), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRequestNotFound()
    {
        var cmd = new RejectWithdrawalCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason");
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WithdrawalRequest?)null);

        var act = () => _handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
