using Moq;
using FluentAssertions;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Wallets;
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
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Constants;
using SharedKernel.Events;

namespace SalesService.Application.Tests.Features.Payments.VnPayReturn;

public class VnPayReturnHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepoMock = new();
    private readonly Mock<IOutboxSignal> _outboxSignalMock = new();
    private readonly Mock<ILogger<VnPayReturnHandler>> _loggerMock = new();
    private readonly Mock<IKafkaProducer> _kafkaProducerMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();

    private readonly VnPayReturnHandler _handler;
    private const string HashSecret = "my_secret";

    public VnPayReturnHandlerTests()
    {
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x["HashSecret"]).Returns(HashSecret);
        _configMock.Setup(x => x.GetSection("VNPay")).Returns(configSectionMock.Object);

        _handler = new VnPayReturnHandler(
            _paymentRepoMock.Object,
            _configMock.Object,
            _outboxRepoMock.Object,
            _outboxSignalMock.Object,
            _loggerMock.Object,
            _kafkaProducerMock.Object,
            _walletRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenVnPayReturnSuccess()
    {
        var paymentId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "1000"
        };
        AddSignature(query);

        var order = new Order 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = customerId,
            OrderSaleItems = [
                new OrderSaleItem { Type = OrderSaleItemType.Voucher, ReferenceId = Guid.NewGuid() },
                new OrderSaleItem { Type = OrderSaleItemType.LoyaltyPoint, Amount = 10 }
            ] 
        };
        var payment = new Payment { Id = paymentId, OrderId = order.Id, Order = order };
        _paymentRepoMock.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        _kafkaProducerMock.Verify(x => x.PublishAsync(KafkaTopics.DeductUserVoucher, It.IsAny<UseVouchersEvent>()), Times.Once());
        _kafkaProducerMock.Verify(x => x.PublishAsync(KafkaTopics.UpdateUserPoint, It.IsAny<UpdatePointEvent>()), Times.Once());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithFailure_WhenResponseCodeNot00()
    {
        var paymentId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "24", // Cancelled by user
            ["vnp_Amount"] = "1000"
        };
        AddSignature(query);

        var payment = new Payment { Id = paymentId, Order = new Order() };
        _paymentRepoMock.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeFalse();
        _kafkaProducerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldDeductWallet_WhenVnPayReturnSuccessWithWalletSaleItem()
    {
        var paymentId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var query = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = paymentId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_Amount"] = "1000"
        };
        AddSignature(query);

        var order = new Order 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = customerId,
            OrderSaleItems = [
                new OrderSaleItem { Type = OrderSaleItemType.Wallet, Amount = 50 }
            ] 
        };
        var payment = new Payment { Id = paymentId, OrderId = order.Id, Order = order };
        var wallet = new Wallet { Id = Guid.NewGuid(), RowVersion = new byte[] { 1 } };

        _paymentRepoMock.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new VnPayReturnCommand(query), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _walletRepoMock.Verify(x => x.DeductBalanceAsync(wallet.Id, 50, wallet.RowVersion, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenHashSecretMissing()
    {
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x["HashSecret"]).Returns((string?)null); // Missing
        _configMock.Setup(x => x.GetSection("VNPay")).Returns(configSectionMock.Object);

        var result = await _handler.Handle(new VnPayReturnCommand(new Dictionary<string, string>()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("HashSecret"));
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenSignatureMissing()
    {
        var result = await _handler.Handle(new VnPayReturnCommand(new Dictionary<string, string>()), CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
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
}
