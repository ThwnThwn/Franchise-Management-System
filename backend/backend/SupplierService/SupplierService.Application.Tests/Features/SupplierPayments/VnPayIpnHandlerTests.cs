using System.Net;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SupplierService.Application.Features.SupplierPayments.Commands.VnPayIpn;
using SupplierService.Application.Helpers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.SupplierPayments;

public class VnPayIpnHandlerTests
{
    private readonly Mock<ISupplierPaymentRepository> _paymentRepoMock;
    private readonly Mock<IPurchaseOrderRepository> _poRepoMock;
    private readonly Mock<ILogger<VnPayIpnHandler>> _loggerMock;
    private readonly IConfiguration _validConfig;
    private readonly VnPayIpnHandler _handler;

    private const string ValidHashSecret = "CAFE_SECRET_KEY_1234567890_CAFE_SECRET";

    public VnPayIpnHandlerTests()
    {
        _paymentRepoMock = new Mock<ISupplierPaymentRepository>();
        _poRepoMock = new Mock<IPurchaseOrderRepository>();
        _loggerMock = new Mock<ILogger<VnPayIpnHandler>>();

        var vnpSectionMock = new Mock<IConfigurationSection>();
        vnpSectionMock.Setup(x => x["HashSecret"]).Returns(ValidHashSecret);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(vnpSectionMock.Object);
        _validConfig = configMock.Object;

        _handler = new VnPayIpnHandler(
            _paymentRepoMock.Object,
            _poRepoMock.Object,
            _validConfig,
            _loggerMock.Object);
    }

    private static Dictionary<string, string> CreateValidVnPayRequest(string txnRef, string amount, string responseCode)
    {
        var dict = new Dictionary<string, string>
        {
            { "vnp_Amount", amount },
            { "vnp_Command", "pay" },
            { "vnp_CreateDate", "20260328100000" },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", "Thanh toan tien nguyen lieu cafe" },
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

        var handler = new VnPayIpnHandler(_paymentRepoMock.Object, _poRepoMock.Object, configMock.Object, _loggerMock.Object);
        var command = new VnPayIpnCommand(new Dictionary<string, string>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("VNPay HashSecret is not configured");
    }

    [Fact]
    public async Task Handle_ShouldReturn97_WhenSignatureIsMissing()
    {
        var command = new VnPayIpnCommand(new Dictionary<string, string> { { "vnp_Amount", "10000" } });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("97");
        result.Value.Message.Should().Be("Invalid signature");
    }

    [Fact]
    public async Task Handle_ShouldReturn97_WhenSignatureIsInvalid()
    {
        var req = CreateValidVnPayRequest(Guid.NewGuid().ToString(), "1000000", "00");
        req["vnp_SecureHash"] = "chuoi-hash-gia-mao-123";
        var command = new VnPayIpnCommand(req);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("97");
        result.Value.Message.Should().Be("Invalid signature");
    }

    [Fact]
    public async Task Handle_ShouldReturn01_WhenTxnRefIsNotGuid()
    {
        var req = CreateValidVnPayRequest("ma-don-hang-khong-hop-le", "50000000", "00");
        var command = new VnPayIpnCommand(req);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task Handle_ShouldReturn01_WhenPaymentNotFound()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "50000000", "00");
        var command = new VnPayIpnCommand(req);

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync((SupplierPayment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Be("Order not found");
    }

    [Theory]
    [InlineData((int)PaymentStatus.Paid)]
    [InlineData((int)PaymentStatus.Failed)]
    public async Task Handle_ShouldReturn02_WhenPaymentIsAlreadyProcessed(int existingStatus)
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "100000000", "00");
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, Status = existingStatus };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("02");
        result.Value.Message.Should().Be("Order already confirmed");
    }

    [Fact]
    public async Task Handle_ShouldReturn04_WhenAmountFormatIsInvalid()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "chuoi-tien-loi", "00");
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, Status = (int)PaymentStatus.Pending, Amount = 500000m };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("04");
        result.Value.Message.Should().Be("Invalid amount");
    }

    [Fact]
    public async Task Handle_ShouldReturn04_AndMarkFailed_WhenAmountMismatches()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "30000000", "00"); // 300k
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, Status = (int)PaymentStatus.Pending, Amount = 500000m }; // Thực tế 500k
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("04");
        result.Value.Message.Should().Be("Invalid amount");

        payment.Status.Should().Be((int)PaymentStatus.Failed);
        payment.Provider.Should().Be("vnpay");
        _paymentRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturn00_AndMarkFailed_WhenTransactionFailedAtVNPay()
    {
        var paymentId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "50000000", "24"); // 24 = Khách hủy giao dịch
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, Status = (int)PaymentStatus.Pending, Amount = 500000m };
        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00");
        result.Value.Message.Should().Be("Confirm Success");

        payment.Status.Should().Be((int)PaymentStatus.Failed);
        payment.ProviderTxnId.Should().Be("123456789");
        _paymentRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _poRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipNonVnPayParameters_WhenIteratingQueryParams()
    {
        // Case này ép code chạy vào dòng: if (!kv.Key.StartsWith("vnp_"...)) continue;
        var paymentId = Guid.NewGuid();

        // 1. Tạo request có chứa tham số "lạ" không bắt đầu bằng vnp_
        var dict = new Dictionary<string, string>
    {
        { "vnp_Amount", "10000000" }, // 100k
        { "vnp_TxnRef", paymentId.ToString() },
        { "vnp_ResponseCode", "00" },
        { "vnp_TransactionNo", "999888" },
        // Tham số "phá đám" để hit dòng continue
        { "utm_source", "facebook_ads" },
        { "extra_data", "coffee_voucher_code" }
    };

        // 2. Ký chữ ký (Chỉ ký những thằng vnp_)
        var vnpOnly = dict.Where(kv => kv.Key.StartsWith("vnp_")).ToDictionary(k => k.Key, v => v.Value);
        var sorted = new SortedDictionary<string, string>(vnpOnly, StringComparer.Ordinal);
        var queryStr = string.Join("&", sorted.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        var hash = VNPayHelper.HmacSHA512(ValidHashSecret, queryStr);
        dict.Add("vnp_SecureHash", hash);

        var command = new VnPayIpnCommand(dict);
        var payment = new SupplierPayment { Id = paymentId, Status = (int)PaymentStatus.Pending, Amount = 100000m };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // 3. Thực thi
        var result = await _handler.Handle(command, CancellationToken.None);

        // 4. Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00"); // Signature vẫn valid vì đã skip các key lạ

        // Verify: Đảm bảo signature được tính toán chính xác dù có tham số lạ trong QueryParams
        _paymentRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldProcessSuccess_AndSetPoToPartial_WhenPaidAmountIsLessThanNetAmount()
    {
        var paymentId = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "150000000", "00"); // 1.5tr
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = poId, Status = (int)PaymentStatus.Pending, Amount = 1500000m };
        var po = new PurchaseOrder { Id = poId, NetAmount = 4000000m, PaidAmount = 0m, Items = [] }; // Cần trả 4tr

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _poRepoMock.Setup(x => x.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync(po);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00");

        payment.Status.Should().Be((int)PaymentStatus.Paid);
        po.PaidAmount.Should().Be(1500000m);
        po.Status.Should().Be(PurchaseOrderStatus.ConfirmPending);
        po.PaymentStatus.Should().Be(PaymentStatus.Partial);

        _poRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldProcessSuccess_AndSetPoToPaid_WhenFullyPaid()
    {
        var paymentId = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "250000000", "00"); // 2.5tr
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = poId, Status = (int)PaymentStatus.Pending, Amount = 2500000m };
        var po = new PurchaseOrder { Id = poId, NetAmount = 2500000m, PaidAmount = 0m, Items = [] }; // Cần trả 2.5tr

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _poRepoMock.Setup(x => x.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync(po);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        po.PaidAmount.Should().Be(2500000m);
        po.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Handle_ShouldProcessSuccess_EvenIfPoIsNotFound_ToAcknowledgeVNPay()
    {
        var paymentId = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var req = CreateValidVnPayRequest(paymentId.ToString(), "50000000", "00");
        var command = new VnPayIpnCommand(req);

        var payment = new SupplierPayment { Id = paymentId, PurchaseOrderId = poId, Status = (int)PaymentStatus.Pending, Amount = 500000m };

        _paymentRepoMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _poRepoMock.Setup(x => x.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseOrder?)null); // Lỗi data không tìm thấy PO

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("00");

        payment.Status.Should().Be((int)PaymentStatus.Paid);
        _paymentRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _poRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}