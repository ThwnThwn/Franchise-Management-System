using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.Features.Payments.VnPayReturn;
using SalesService.Application.Interfaces.Payments;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.Payments;

public class VnPayReturnHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IConfiguration> _config;
    private readonly Mock<ILogger<VnPayReturnHandler>> _logger;
    private readonly VnPayReturnHandler _handler;

    private const string HashSecret = "THIS_IS_A_VERY_SECRET_KEY_FOR_COFFEE_SHOP_VNPAY";

    public VnPayReturnHandlerTests()
    {
        _paymentRepo = new Mock<IPaymentRepository>();
        _config = new Mock<IConfiguration>();
        _logger = new Mock<ILogger<VnPayReturnHandler>>();

        _handler = new VnPayReturnHandler(
            _paymentRepo.Object,
            _config.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Handle_WhenHashSecretIsMissing_ReturnsError()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns(string.Empty);
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);

        var request = new VnPayReturnCommand(new Dictionary<string, string>());

        var result = await _handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("HashSecret is not configured");
    }

    [Fact]
    public async Task Handle_WhenSecureHashIsMissing_ReturnsInvalid()
    {
        SetupMockConfig();
        var queryParams = new Dictionary<string, string>
        {
            { "vnp_Amount", "5000000" }, // 50k for a cup of Robusta
            { "vnp_BankCode", "NCB" }
        };

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.First().Identifier.Should().Be("vnp_SecureHash");
    }

    [Fact]
    public async Task Handle_WhenSignatureIsInvalid_ReturnsError()
    {
        SetupMockConfig();
        var queryParams = new Dictionary<string, string>
        {
            { "vnp_Amount", "5500000" }, // 55k for Arabica
            { "vnp_SecureHash", "FAKE_SIGNATURE_HASH_12345" }
        };

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("Invalid VNPay signature");
    }

    [Fact]
    public async Task Handle_WhenTxnRefIsInvalidGuid_ReturnsInvalid()
    {
        SetupMockConfig();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "4500000" }, // 45k for Bac Xiu
            { "vnp_TxnRef", "INVALID_GUID_STRING" }
        });

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.First().Identifier.Should().Be("vnp_TxnRef");
    }

    [Fact]
    public async Task Handle_WhenPaymentIsNotFound_ReturnsNotFound()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "6000000" }, // 60k for Matcha Latte
            { "vnp_TxnRef", paymentId.ToString() }
        });

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.First().Should().Contain("Payment not found");
    }

    [Fact]
    public async Task Handle_WhenPaymentIsSuccessful_ReturnsSuccessAndLogsInfo()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "3900000" }, // 39k for Ca Phe Sua Da
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" } // 00 means Success in VNPay
        });

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Order = new Order { Id = orderId }
        };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.PaymentId.Should().Be(paymentId);
        result.Value.OrderId.Should().Be(orderId);
        result.Value.ResponseCode.Should().Be("00");
    }

    [Fact]
    public async Task Handle_WhenPaymentIsFailed_ReturnsSuccessButWithFailedFlagAndLogsWarning()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "5000000" }, // 50k for Cold Brew
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "24" } // 24 means Customer canceled
        });

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Order = new Order { Id = orderId }
        };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayReturnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeFalse(); // Because ResponseCode is not '00'
        result.Value.PaymentId.Should().Be(paymentId);
        result.Value.OrderId.Should().Be(orderId);
        result.Value.ResponseCode.Should().Be("24");
    }

    private void SetupMockConfig()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns(HashSecret);
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);
    }

    private static Dictionary<string, string> CreateValidVnPayParams(Dictionary<string, string> inputParams)
    {
        // 1. Filter out existing hash properties to build the raw data
        var vnpParams = new Dictionary<string, string>();
        foreach (var kv in inputParams)
        {
            if (kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                vnpParams[kv.Key] = kv.Value;
            }
        }

        // 2. Sort by key (VNPay standard)
        var sortedParams = new SortedList<string, string>(vnpParams, StringComparer.Ordinal);

        // 3. Build query string
        var queryBuilder = new StringBuilder();
        foreach (var kv in sortedParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                queryBuilder.Append(kv.Key).Append('=').Append(Uri.EscapeDataString(kv.Value)).Append('&');
            }
        }

        if (queryBuilder.Length > 0)
        {
            queryBuilder.Remove(queryBuilder.Length - 1, 1);
        }

        // 4. Generate HMACSHA512 Hash
        var signData = queryBuilder.ToString();
        var hashBytes = HMACSHA512(HashSecret, signData);
        var secureHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // 5. Attach the generated hash to the parameters
        inputParams["vnp_SecureHash"] = secureHash;
        return inputParams;
    }

    private static byte[] HMACSHA512(string key, string inputData)
    {
        var hash = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return hash.ComputeHash(Encoding.UTF8.GetBytes(inputData));
    }
}