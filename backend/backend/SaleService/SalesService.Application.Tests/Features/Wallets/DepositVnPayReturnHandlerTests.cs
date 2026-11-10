using Moq;
using FluentAssertions;
using SalesService.Application.Features.Wallets.Deposit;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using Microsoft.Extensions.Configuration;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Application.Helpers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SalesService.Application.Tests.Features.Wallets.Deposit;

public class DepositVnPayReturnHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IConfiguration> _configMock = new();

    private readonly DepositVnPayReturnHandler _handler;
    private const string HashSecret = "deposit_secret";

    public DepositVnPayReturnHandlerTests()
    {
        _configMock.Setup(x => x["VNPay:HashSecret"]).Returns(HashSecret);

        _handler = new DepositVnPayReturnHandler(
            _paymentRepoMock.Object,
            _walletRepoMock.Object,
            _transactionRepoMock.Object,
            _configMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenMissingHash()
    {
        var query = new Dictionary<string, string> { ["vnp_TxnRef"] = Guid.NewGuid().ToString() };
        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(x => x.Identifier == "vnp_SecureHash");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDepositConfirmedSuccess()
    {
        var paymentId = Guid.NewGuid();
        var walletTxnId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "50000000"
        };
        AddSignature(query);

        var payment = new Payment 
        { 
            Id = paymentId, 
            Amount = 500000, 
            Status = (int)PaymentStatus.Pending,
            ReferenceId = walletTxnId 
        };
        var walletTxn = new WalletTransaction { Id = walletTxnId, WalletId = walletId, Status = TransactionStatus.Pending };
        var wallet = new Wallet { Id = walletId, Balance = 1000 };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _transactionRepoMock.Setup(x => x.GetByIdAsync(walletTxnId, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepoMock.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        wallet.Balance.Should().Be(501000);
        walletTxn.Status.Should().Be(TransactionStatus.Completed);
        payment.Status.Should().Be((int)PaymentStatus.Paid);
    }

    [Fact]
    public async Task Handle_ShouldReturnAlreadyConfirmed_WhenStatusIsPaid()
    {
        var paymentId = Guid.NewGuid();
        var walletTxnId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "50000000"
        };
        AddSignature(query);

        var payment = new Payment 
        { 
            Id = paymentId, 
            Amount = 500000, 
            Status = (int)PaymentStatus.Paid,
            ReferenceId = walletTxnId 
        };
        var walletTxn = new WalletTransaction { Id = walletTxnId, WalletId = walletId };
        var wallet = new Wallet { Id = walletId, Balance = 501000 };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _transactionRepoMock.Setup(x => x.GetByIdAsync(walletTxnId, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepoMock.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.NewBalance.Should().Be(501000);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenSignatureInvalid()
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = Guid.NewGuid().ToString(),
            ["vnp_SecureHash"] = "INVALID"
        };
        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid VNPay signature"));
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenTxnRefInvalid()
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = "not_a_guid",
            ["vnp_SecureHash"] = "DUMMY"
        };
        _configMock.Setup(x => x["VNPay:HashSecret"]).Returns("secret");
        // Mocking VerifySignature to pass for dummy
        // But the code uses VNPayHelper.VerifySignature(hashSecret!, vnpParams, secureHash) which is static.
        // Wait, VNPayHelper.VerifySignature is static. I must provide a VALID signature if I want to pass step 1.
        
        AddSignature(query, "secret");
        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenAmountMismatch()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_Amount"] = "99900" // Mismatch
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Amount = 1000, Status = (int)PaymentStatus.Pending };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new DepositVnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Amount mismatch"));
    }

    private void AddSignature(Dictionary<string, string> query, string secret = HashSecret)
    {
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in query)
        {
            if (kv.Key.StartsWith("vnp_") && !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
            {
                vnpParams[kv.Key] = kv.Value;
            }
        }
        var rawData = string.Join("&", vnpParams.Select(kv => $"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}"));
        query["vnp_SecureHash"] = VNPayHelper.HmacSHA512(secret, rawData);
    }
}
