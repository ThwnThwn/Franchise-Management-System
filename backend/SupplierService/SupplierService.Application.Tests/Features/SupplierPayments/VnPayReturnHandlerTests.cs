using System.Net;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayReturn;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.SupplierPayments;

public class VnPayReturnHandlerTests
{
    private readonly Mock<ISupplierPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILogger<VnPayReturnHandler>> _loggerMock;
    private readonly IConfiguration _validConfig;
    private readonly VnPayReturnHandler _handler;

    private const string ValidHashSecret = "CAFE_SECRET_KEY_1234567890_CAFE_SECRET";

    public VnPayReturnHandlerTests()
    {
        _paymentRepoMock = new Mock<ISupplierPaymentRepository>();
        _loggerMock = new Mock<ILogger<VnPayReturnHandler>>();

        var vnpSectionMock = new Mock<IConfigurationSection>();
        vnpSectionMock.Setup(x => x["HashSecret"]).Returns(ValidHashSecret);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(vnpSectionMock.Object);
        _validConfig = configMock.Object;

        _handler = new VnPayReturnHandler(
            _paymentRepoMock.Object,
            _validConfig,
            _loggerMock.Object);
    }

    private Dictionary<string, string> CreateValidVnPayRequest(string txnRef, string responseCode)
    {
        var dict = new Dictionary<string, string>
        {
            { "vnp_Amount", "25000000" },
            { "vnp_Command", "pay" },
            { "vnp_CreateDate", "20260328100000" },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", "Thanh toan tien ca phe rang xay" },
            { "vnp_OrderType", "other" },
            { "vnp_ResponseCode", responseCode },
            { "vnp_TmnCode", "CF_TMN" },
            { "vnp_TransactionNo", "123456789" },
            { "vnp_TxnRef", txnRef }
        };

        var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
        var query = string.Join("&", sorted.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        var hash = VNPayHelper.HmacSHA512(ValidHashSecret, query);

        dict.Add("vnp_SecureHash", hash);
        dict.Add("vnp_SecureHashType", "HmacSHA512");

        return dict;
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenHashSecretIsMissing()
    {
        var emptySectionMock = new Mock<IConfigurationSection>();
        emptySectionMock.Setup(x => x["HashSecret"]).Returns((string?)null);
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(emptySectionMock.Object);

        var handler = new VnPayReturnHandler(_paymentRepoMock.Object, configMock.Object, _loggerMock.Object);
        var command = new VnPayReturnCommand(new Dictionary<string, string>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("VNPay HashSecret is not configured");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenSecureHashIsMissing()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "vnp_TxnRef", Guid.NewGuid().ToString() }
        };
        var command = new VnPayReturnCommand(queryParams);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().ContainSingle(e => e.Identifier == "vnp_SecureHash");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenSignatureIsInvalid()
    {
        var req = CreateValidVnPayRequest(Guid.NewGuid().ToString(), "00");
        req["vnp_SecureHash"] = "chuoi-hash-gia-mao";
        var command = new VnPayReturnCommand(req);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Invalid VNPay signature");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenTxnRefIsNotGuid()
    {
        var req = CreateValidVnPayRequest("ma-don-hang-khong-hop-le", "00");
        var command = new VnPayReturnCommand(req);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().ContainSingle(e => e.Identifier == "vnp_TxnRef");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "00");
        var command = new VnPayReturnCommand(req);

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync((SupplierPayment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain("Payment not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithFalseStatus_WhenResponseCodeIsNot00()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "24");
        var command = new VnPayReturnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = Guid.NewGuid() };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeFalse();
        result.Value.ResponseCode.Should().Be("24");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithTrueStatus_WhenResponseCodeIs00()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "00");
        var command = new VnPayReturnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = Guid.NewGuid() };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.ResponseCode.Should().Be("00");
    }

    [Fact]
    public async Task Handle_ShouldSkipNonVnPayParameters_WhenBuildingVnpParams()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "00");

        // Thêm tham số không bắt đầu bằng vnp_ để hit dòng continue
        req.Add("utm_source", "coffee_ads_facebook");
        req.Add("random_key", "coffee_shop_value");

        var command = new VnPayReturnCommand(req);
        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = Guid.NewGuid() };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        // Nếu skip thành công, chữ ký vẫn Valid vì hàm CreateValidVnPayRequest chỉ tính hash dựa trên vnp_
        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResponseCode_WhenVnpResponseCodeIsMissingFromQuery()
    {
        // Case này cover nhánh: ResponseCode = responseCode ?? string.Empty
        var paymentId = Guid.NewGuid();
        var poId = Guid.NewGuid();

        // 1. Tạo request KHÔNG CÓ vnp_ResponseCode
        var dict = new Dictionary<string, string>
    {
        { "vnp_Amount", "12000000" }, // 120k VND
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", "COFFEE_SHOP_01" },
        { "vnp_TxnRef", paymentId.ToString() }
        // Thiếu vnp_ResponseCode
    };

        // 2. Ký chữ ký cho đống params thiếu thốn này
        var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
        var queryStr = string.Join("&", sorted.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        var hash = VNPayHelper.HmacSHA512(ValidHashSecret, queryStr);
        dict.Add("vnp_SecureHash", hash);

        var command = new VnPayReturnCommand(dict);
        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = poId };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // 3. Thực thi
        var result = await _handler.Handle(command, CancellationToken.None);

        // 4. Kiểm chứng
        result.IsSuccess.Should().BeTrue();
        result.Value.ResponseCode.Should().Be(string.Empty); // Đã cover được nhánh ?? string.Empty
        result.Value.Success.Should().BeFalse(); // Vì null != "00"
        result.Value.PaymentId.Should().Be(paymentId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Giao dịch thất bại")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}