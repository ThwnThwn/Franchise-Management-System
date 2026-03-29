using Moq;
using FluentAssertions;
using SalesService.Application.Features.Wallets.Withdraw;
using SalesService.Application.Persisitences.WalletEvents;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Tests.Features.Wallets;

public class WithdrawWalletHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _txnRepoMock = new();
    private readonly Mock<IWalletEventRepository> _eventRepoMock = new();
    private readonly WithdrawWalletHandler _handler;

    public WithdrawWalletHandlerTests()
    {
        _handler = new WithdrawWalletHandler(_walletRepoMock.Object, _txnRepoMock.Object, _eventRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountIsZero()
    {
        var cmd = new WithdrawWalletCommand(Guid.NewGuid(), new WithdrawWalletReq { Amount = 0, Reason = "Test" });
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletNotFound()
    {
        var cmd = new WithdrawWalletCommand(Guid.NewGuid(), new WithdrawWalletReq { Amount = 100, Reason = "Test" });
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(cmd.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenInsufficientBalance()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 50 };
        var cmd = new WithdrawWalletCommand(userId, new WithdrawWalletReq { Amount = 100, Reason = "Test" });
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Insufficient"));
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 500 };
        var cmd = new WithdrawWalletCommand(userId, new WithdrawWalletReq { Amount = 100, Reason = "Cash out" });
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100);
        result.Value.Status.Should().Be("Pending");
        _txnRepoMock.Verify(x => x.AddAsync(It.Is<WalletTransaction>(t => t.Amount == -100), It.IsAny<CancellationToken>()), Times.Once);
        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<WalletEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
