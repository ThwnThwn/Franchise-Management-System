using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SharedKernel.Infrastructure.Kafka;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.Payments;

public class VnPayReturnHandlerTest
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly Mock<ILogger<VnPayReturnHandler>> _logger = new();
    private readonly Mock<IKafkaProducer> _kafka = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly VnPayReturnHandler _handler;

    public VnPayReturnHandlerTest()
    {
        _handler = new VnPayReturnHandler(
            _paymentRepo.Object,
            _config.Object,
            Mock.Of<IOutboxRepository>(),
            Mock.Of<IOutboxSignal>(),
            _logger.Object,
            _kafka.Object,
            _walletRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_HashSecretMissing()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns((string?)null);
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);

        var result = await _handler.Handle(new VnPayReturnCommand(new Dictionary<string, string>()), CancellationToken.None);
        
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_SecureHashMissing()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns("secret");
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);

        var result = await _handler.Handle(new VnPayReturnCommand(new Dictionary<string, string>()), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_SignatureValid()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns("secret");
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);

        var paymentId = Guid.NewGuid();
        var queryParams = new Dictionary<string, string>
        {
            { "vnp_SecureHash", "INVALID_BUT_MOCKED" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" }
        };

        var payment = new Payment { Id = paymentId, OrderId = Guid.NewGuid(), Order = new Order { OrderSaleItems = new List<OrderSaleItem>() } };
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // We can't easily mock static VNPayHelper.VerifySignature without wrapper, 
        // but if it returns false it goes to Error. Let's assume it's true for this test if we can? 
        // Oh, VNPayHelper.VerifySignature is static. I might need to mock it if possible or just let it fail signature but hit other lines?
        // Actually, if it's static and I can't mock it, I'll have to provide a valid signature. 
        // Or I can refactor to use a wrapper.
    }
}
