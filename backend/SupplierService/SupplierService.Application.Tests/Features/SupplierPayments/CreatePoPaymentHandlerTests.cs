using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SupplierService.Application.Features.SupplierPayments.Commands.CreatePoPayment;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.SupplierPayments;

public class CreatePoPaymentHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _poRepoMock;
    private readonly Mock<ISupplierPaymentRepository> _paymentRepoMock;
    private readonly IConfiguration _validConfiguration;
    private readonly CreatePoPaymentHandler _handler;

    public CreatePoPaymentHandlerTests()
    {
        _poRepoMock = new Mock<IPurchaseOrderRepository>();
        _paymentRepoMock = new Mock<ISupplierPaymentRepository>();

        var vnpSectionMock = new Mock<IConfigurationSection>();
        vnpSectionMock.Setup(x => x["TmnCode"]).Returns("CF_TMN_CODE");
        vnpSectionMock.Setup(x => x["HashSecret"]).Returns("CF_SECRET_KEY");
        vnpSectionMock.Setup(x => x["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/vpcpay.html");
        vnpSectionMock.Setup(x => x["SupplierReturnUrl"]).Returns("https://quan-ca-phe.com/return");
        vnpSectionMock.Setup(x => x["Version"]).Returns("2.1.0");
        vnpSectionMock.Setup(x => x["CurrCode"]).Returns("VND");
        vnpSectionMock.Setup(x => x["Locale"]).Returns("vn");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(vnpSectionMock.Object);

        _validConfiguration = configMock.Object;
        _handler = new CreatePoPaymentHandler(_poRepoMock.Object, _paymentRepoMock.Object, _validConfiguration);
    }

    // --- CÁC TEST CASE BỔ SUNG ĐỂ COVER 100% ---

    [Fact]
    public async Task Handle_ShouldSetOrderToPartial_WhenCashPaymentDoesNotCoverFullAmount()
    {
        var orderId = Guid.NewGuid();
        var command = new CreatePoPaymentCommand(orderId, SupplierPaymentMethod.Cash, null);

        var po = new PurchaseOrder
        {
            Id = orderId,
            Code = "PO-CF-PARTIAL",
            NetAmount = 5000000m,
            PaidAmount = 1000000m,
            PaymentStatus = PaymentStatus.Partial,
            Items = []
        };

        _poRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Chiêu thức: Khi repo gọi CreateAsync cho Payment, ta lén tăng NetAmount của PO lên
        // Điều này giả lập việc: Tiền trả tính trên giá cũ, nhưng ngay lúc đó giá PO bị đội lên
        _paymentRepoMock
            .Setup(repo => repo.CreateAsync(It.IsAny<SupplierPayment>(), It.IsAny<CancellationToken>()))
            .Callback<SupplierPayment, CancellationToken>((p, ct) =>
            {
                po.NetAmount += 1000m; // Tăng tổng tiền lên 1 ít để tiền vừa trả không bao giờ đủ
            });

        await _handler.Handle(command, CancellationToken.None);

        // Assert: Lúc này PaidAmount (5tr) sẽ nhỏ hơn NetAmount (5tr + 1000)
        po.PaymentStatus.Should().Be(PaymentStatus.Partial);

        _poRepoMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("TmnCode", "VNPay:TmnCode not configured")]
    [InlineData("HashSecret", "VNPay:HashSecret not configured")]
    [InlineData("PaymentUrl", "VNPay:PaymentUrl not configured")]
    [InlineData("SupplierReturnUrl", "VNPay:SupplierReturnUrl not configured")]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenSpecificVNPayConfigIsMissing(string configKey, string expectedMessage)
    {
        // Case này cover các dòng: var X = vnpSection["Y"] ?? throw ...
        var vnpSectionMock = new Mock<IConfigurationSection>();

        // Setup tất cả đều có giá trị trừ cái key đang test
        vnpSectionMock.Setup(x => x["TmnCode"]).Returns(configKey == "TmnCode" ? null : "VALUE");
        vnpSectionMock.Setup(x => x["HashSecret"]).Returns(configKey == "HashSecret" ? null : "VALUE");
        vnpSectionMock.Setup(x => x["PaymentUrl"]).Returns(configKey == "PaymentUrl" ? null : "VALUE");
        vnpSectionMock.Setup(x => x["SupplierReturnUrl"]).Returns(configKey == "SupplierReturnUrl" ? null : "VALUE");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(vnpSectionMock.Object);

        var handlerWithFaultyConfig = new CreatePoPaymentHandler(_poRepoMock.Object, _paymentRepoMock.Object, configMock.Object);
        var command = new CreatePoPaymentCommand(Guid.NewGuid(), SupplierPaymentMethod.VNPay, null);
        var po = new PurchaseOrder { Id = command.PurchaseOrderId, NetAmount = 1000m, PaidAmount = 0m, Items = [] };

        _poRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(command.PurchaseOrderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);

        var act = () => handlerWithFaultyConfig.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Handle_ShouldInitializeParametersDictionary_AndReturnSuccessUrl()
    {
        // Case này cover dòng: var parameters = new Dictionary<string, string>
        // và đảm bảo toàn bộ luồng VNPay thành công.
        var orderId = Guid.NewGuid();
        var command = new CreatePoPaymentCommand(orderId, SupplierPaymentMethod.VNPay, "https://return.url");
        var po = new PurchaseOrder { Id = orderId, Code = "PO-COVER-100", NetAmount = 200000m, PaidAmount = 0m, Items = [] };

        _poRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(po);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Contain("vnp_TxnRef=");
        result.Value.Url.Should().Contain("vnp_Amount=20000000"); // 200.000 * 100

        _paymentRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<SupplierPayment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- GIỮ LẠI CÁC TEST CŨ ĐỂ ĐẢM BẢO KHÔNG REGRESSION ---
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var command = new CreatePoPaymentCommand(Guid.NewGuid(), SupplierPaymentMethod.Cash, null);
        _poRepoMock.Setup(repo => repo.GetByIdWithItemsAsync(command.PurchaseOrderId, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseOrder?)null);

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultVNPayParameters_WhenOptionalConfigsAreMissing()
    {
        // Case này cover các nhánh ?? "2.1.0", ?? "VND", ?? "vn"
        var orderId = Guid.NewGuid();
        var command = new CreatePoPaymentCommand(orderId, SupplierPaymentMethod.VNPay, "https://return.url");

        var po = new PurchaseOrder
        {
            Id = orderId,
            Code = "PO-CAFE-CHON-001",
            NetAmount = 1000000m,
            PaidAmount = 0m,
            Items = []
        };

        _poRepoMock
            .Setup(repo => repo.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        // Mock một config chỉ có các key bắt buộc, khuyết các key optional
        var vnpSectionMock = new Mock<IConfigurationSection>();
        vnpSectionMock.Setup(x => x["TmnCode"]).Returns("COFFEE_TMN");
        vnpSectionMock.Setup(x => x["HashSecret"]).Returns("SECRET_KEY");
        vnpSectionMock.Setup(x => x["PaymentUrl"]).Returns("https://vnpay.vn/pay");
        vnpSectionMock.Setup(x => x["SupplierReturnUrl"]).Returns("https://cafe.com/return");

        // Ép các key optional trả về null để code nhảy vào nhánh ??
        vnpSectionMock.Setup(x => x["Version"]).Returns((string?)null);
        vnpSectionMock.Setup(x => x["CurrCode"]).Returns((string?)null);
        vnpSectionMock.Setup(x => x["Locale"]).Returns((string?)null);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetSection("VNPay")).Returns(vnpSectionMock.Object);

        var handlerWithMissingOptionalConfig = new CreatePoPaymentHandler(
            _poRepoMock.Object,
            _paymentRepoMock.Object,
            configMock.Object);

        // Act
        var result = await handlerWithMissingOptionalConfig.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Kiểm tra xem URL có chứa các giá trị mặc định không
        var url = result.Value.Url;
        url.Should().Contain("vnp_Version=2.1.0"); // Giá trị mặc định 1
        url.Should().Contain("vnp_CurrCode=VND");   // Giá trị mặc định 2
        url.Should().Contain("vnp_Locale=vn");     // Giá trị mặc định 3

        // Kiểm tra các tham số khác vẫn được truyền đúng
        url.Should().Contain("vnp_TmnCode=COFFEE_TMN");
        url.Should().Contain("vnp_OrderInfo=Thanh+toan+don+nhap+hang+PO-CAFE-CHON-001");
    }
}