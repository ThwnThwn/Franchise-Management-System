using Moq;
using FluentAssertions;
using SalesService.Application.Features.Payments.VnPayIpn;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Application.Helpers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using SharedKernel.Events;
using SharedKernel.Constants;

namespace SalesService.Application.Tests.Features.Payments.VnPayIpn;

public class VnPayIpnHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepoMock = new();
    private readonly Mock<IOutboxSignal> _outboxSignalMock = new();
    private readonly Mock<ILogger<VnPayIpnHandler>> _loggerMock = new();

    private readonly VnPayIpnHandler _handler;
    private const string HashSecret = "my_secret";

    public VnPayIpnHandlerTests()
    {
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x["HashSecret"]).Returns(HashSecret);
        _configMock.Setup(x => x.GetSection("VNPay")).Returns(configSectionMock.Object);

        _handler = new VnPayIpnHandler(
            _paymentRepoMock.Object,
            _walletRepoMock.Object,
            _transactionRepoMock.Object,
            _configMock.Object,
            _outboxRepoMock.Object,
            _outboxSignalMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturn97_WhenSignatureInvalid()
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = Guid.NewGuid().ToString(),
            ["vnp_SecureHash"] = "INVALID"
        };
        var cmd = new VnPayIpnCommand(query);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("97");
    }

    [Fact]
    public async Task Handle_ShouldReturn01_WhenTxnRefInvalid()
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = "not_a_guid"
        };
        // Need to add valid signature so it passes the Verify part
        AddSignature(query);
        var cmd = new VnPayIpnCommand(query);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("01");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOrderPaymentConfirmedSuccess()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "10000000", // 100,000 VND
            ["vnp_TransactionNo"] = "TRANS_123"
        };
        AddSignature(query);

        var payment = new Payment 
        { 
            Id = paymentId, 
            Amount = 100000, 
            Status = (int)PaymentStatus.Pending,
            OrderId = orderId 
        };
        var order = new Order 
        { 
            Id = orderId, 
            Status = OrderStatus.PaymentPending,
            OrderItems = new List<OrderItem> { new OrderItem { Id = Guid.NewGuid() } }
        };
        payment.Order = order;

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _paymentRepoMock.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00");
        payment.Status.Should().Be((int)PaymentStatus.Paid);
        order.Status.Should().Be(OrderStatus.ConfirmationPending);
        _outboxRepoMock.Verify(x => x.Add(It.IsAny<OutboxMessage>()), Times.AtLeastOnce());
        _outboxSignalMock.Verify(x => x.Pulse(), Times.Once());
    }

    [Fact]
    public async Task Handle_ShouldReturn04_WhenAmountMismatch()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_Amount"] = "20000000" // Corresponds to 200,000
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Amount = 100000, Status = (int)PaymentStatus.Pending };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("04");
        payment.Status.Should().Be((int)PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenWalletDepositSuccess()
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
            ReferenceType = PaymentReferenceType.Deposit,
            ReferenceId = walletTxnId 
        };
        var walletTxn = new WalletTransaction { Id = walletTxnId, WalletId = walletId, Status = TransactionStatus.Pending };
        var wallet = new Wallet { Id = walletId, Balance = 1000 };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _transactionRepoMock.Setup(x => x.GetByIdAsync(walletTxnId, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepoMock.Setup(x => x.GetByIdAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00");
        wallet.Balance.Should().Be(501000);
        walletTxn.Status.Should().Be(TransactionStatus.Completed);
        payment.Status.Should().Be((int)PaymentStatus.Paid);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithFailedStatus_WhenResponseCodeNot00()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "24", // Cancelled
            ["vnp_Amount"] = "10000000"
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Amount = 100000, Status = (int)PaymentStatus.Pending, Order = new Order { Id = Guid.NewGuid() } };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be((int)PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_ShouldReturn02_WhenPaymentAlreadyConfirmed()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString()
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Status = (int)PaymentStatus.Paid };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("02");
    }

    private void AddSignature(Dictionary<string, string> query)
    {
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in query)
        {
            if (kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash")
            {
                vnpParams[kv.Key] = kv.Value;
            }
        }
        var rawData = string.Join("&", vnpParams.Select(kv => $"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}"));
        query["vnp_SecureHash"] = VNPayHelper.HmacSHA512(HashSecret, rawData);
    }

    [Fact]
    public async Task Handle_ShouldReturn01_WhenWalletTxnNotFoundInDeposit()
    {
        var paymentId = Guid.NewGuid();
        var walletTxnId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "100"
        };
        AddSignature(query);

        var payment = new Payment 
        { 
            Id = paymentId, 
            ReferenceType = PaymentReferenceType.Deposit,
            ReferenceId = walletTxnId 
        };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _transactionRepoMock.Setup(x => x.GetByIdAsync(walletTxnId, It.IsAny<CancellationToken>())).ReturnsAsync((WalletTransaction?)null);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Contain("Wallet transaction not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPaymentIsNull()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString()
        };
        AddSignature(query);

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturn04_WhenAmountStrIsInvalid()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_Amount"] = "abc"
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Status = (int)PaymentStatus.Pending };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(query), CancellationToken.None);

        result.Value.RspCode.Should().Be("04");
    }
}
