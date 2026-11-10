using Ardalis.Result;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Features.Wallets.Deposit;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SalesService.Application.Tests.Features.Wallets;

public class DepositWalletHandlerTest
{
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly DepositWalletHandler _handler;

    public DepositWalletHandlerTest()
    {
        _handler = new DepositWalletHandler(
            _walletRepo.Object,
            _transactionRepo.Object,
            _paymentRepo.Object,
            _config.Object
        );
        
        var vnpSection = new Mock<IConfigurationSection>();
        vnpSection.Setup(x => x["Version"]).Returns("2.1.0");
        vnpSection.Setup(x => x["TmnCode"]).Returns("TMN");
        vnpSection.Setup(x => x["HashSecret"]).Returns("secret");
        vnpSection.Setup(x => x["PaymentUrl"]).Returns("https://vnpay.vn");
        vnpSection.Setup(x => x["WalletVNPayReturn"]).Returns("http://localhost/return");
        vnpSection.Setup(x => x["WalletClientReturnUrl"]).Returns("http://client/return");
        
        _config.Setup(x => x.GetSection("VNPay")).Returns(vnpSection.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenAmountZero()
    {
        var request = new DepositWalletReq { Amount = 0 };
        var cmd = new DepositWalletCommand(Guid.NewGuid(), request);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWalletDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new DepositWalletReq { Amount = 10000 };
        var cmd = new DepositWalletCommand(userId, request);

        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithUrl_WhenValid()
    {
        var userId = Guid.NewGuid();
        var request = new DepositWalletReq { Amount = 10000, ReturnUrl = "http://myclient/thanks" };
        var cmd = new DepositWalletCommand(userId, request);
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 0 };

        _walletRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentUrl.Should().Contain("https://vnpay.vn");
        result.Value.PaymentUrl.Should().Contain("vnp_Amount=1000000"); // 10000 * 100
        
        _transactionRepo.Verify(x => x.AddAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepo.Verify(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
