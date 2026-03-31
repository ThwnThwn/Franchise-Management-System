using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.Features.Payments.VnPayIpn;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;

namespace SalesService.Application.Tests.Features.Payments;

public class VnPayIpnHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IWalletRepository> _walletRepo;
    private readonly Mock<IWalletTransactionRepository> _walletTxnRepo;
    private readonly Mock<IIdentityServiceClient> _identityClient;
    private readonly Mock<IConfiguration> _config;
    private readonly Mock<IOutboxRepository> _outboxRepo;
    private readonly Mock<IOutboxSignal> _outboxSignal;
    private readonly Mock<ILogger<VnPayIpnHandler>> _logger;
    private readonly VnPayIpnHandler _handler;

    private const string HashSecret = "SECRET_COFFEE_SHOP_HASH_KEY_1234567890";

    public VnPayIpnHandlerTests()
    {
        _paymentRepo = new Mock<IPaymentRepository>();
        _walletRepo = new Mock<IWalletRepository>();
        _walletTxnRepo = new Mock<IWalletTransactionRepository>();
        _identityClient = new Mock<IIdentityServiceClient>();
        _config = new Mock<IConfiguration>();
        _outboxRepo = new Mock<IOutboxRepository>();
        _outboxSignal = new Mock<IOutboxSignal>();
        _logger = new Mock<ILogger<VnPayIpnHandler>>();

        _handler = new VnPayIpnHandler(
            _paymentRepo.Object,
            _walletRepo.Object,
            _walletTxnRepo.Object,
            _identityClient.Object,
            _config.Object,
            _outboxRepo.Object,
            _outboxSignal.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Handle_WhenHashSecretIsMissing_ReturnsError()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns(string.Empty);
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);

        var result = await _handler.Handle(new VnPayIpnCommand(new Dictionary<string, string>()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Contain("HashSecret is not configured");
    }

    [Fact]
    public async Task Handle_WhenSecureHashIsMissing_Returns97()
    {
        SetupMockConfig();
        var queryParams = new Dictionary<string, string> { { "vnp_Amount", "5000000" } };

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("97");
        result.Value.Message.Should().Be("Invalid signature");
    }

    [Fact]
    public async Task Handle_WhenSignatureIsInvalid_Returns97()
    {
        SetupMockConfig();
        var queryParams = new Dictionary<string, string>
        {
            { "vnp_Amount", "4500000" }, // 45k cho Bạc Xỉu
            { "vnp_SecureHash", "FAKE_HASH_SIGNATURE_INVALID" }
        };

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("97");
    }

    [Fact]
    public async Task Handle_WhenTxnRefIsInvalid_Returns01()
    {
        SetupMockConfig();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "6500000" },
            { "vnp_TxnRef", "INVALID_GUID_STRING" }
        });

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task Handle_WhenPaymentIsNull_ThrowsException()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "10000000" },
            { "vnp_TxnRef", paymentId.ToString() }
        });

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var action = async () => await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("payment is null |VnpayHandler");
    }

    [Fact]
    public async Task Handle_WhenWalletDepositTxnNotFound_Returns01()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "50000000" }, // Nạp 500k vào ví
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" }
        });

        var payment = new Payment { Id = paymentId, ReferenceType = PaymentReferenceType.Deposit, ReferenceId = Guid.NewGuid() };
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletTxnRepo.Setup(x => x.GetByIdAsync(payment.ReferenceId.Value, It.IsAny<CancellationToken>())).ReturnsAsync((WalletTransaction?)null);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Be("Wallet transaction not found");
    }

    [Fact]
    public async Task Handle_WhenWalletDepositWalletNotFound_Returns01()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "20000000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" }
        });

        var payment = new Payment { Id = paymentId, ReferenceType = PaymentReferenceType.Deposit, ReferenceId = Guid.NewGuid() };
        var walletTxn = new WalletTransaction { Id = payment.ReferenceId.Value, WalletId = Guid.NewGuid() };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletTxnRepo.Setup(x => x.GetByIdAsync(payment.ReferenceId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepo.Setup(x => x.GetByIdAsync(walletTxn.WalletId.Value, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("01");
        result.Value.Message.Should().Be("Wallet not found");
    }

    [Fact]
    public async Task Handle_WhenWalletDepositIsSuccessful_UpdatesWalletAndReturns00()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "10000000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionNo", "VNPTXN123" }
        });

        var payment = new Payment { Id = paymentId, Amount = 100000, ReferenceType = PaymentReferenceType.Deposit, ReferenceId = Guid.NewGuid() };
        var walletTxn = new WalletTransaction { Id = payment.ReferenceId.Value, WalletId = Guid.NewGuid() };
        var wallet = new Wallet { Id = walletTxn.WalletId.Value, Balance = 50000 };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletTxnRepo.Setup(x => x.GetByIdAsync(payment.ReferenceId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepo.Setup(x => x.GetByIdAsync(walletTxn.WalletId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("00");
        wallet.Balance.Should().Be(150000);
        walletTxn.Status.Should().Be(TransactionStatus.Completed);
        payment.Status.Should().Be((int)PaymentStatus.Paid);
        _paymentRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWalletDepositIsFailed_UpdatesTxnStatusAndReturns00()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "10000000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "24" }
        });

        var payment = new Payment { Id = paymentId, Amount = 100000, ReferenceType = PaymentReferenceType.Deposit, ReferenceId = Guid.NewGuid() };
        var walletTxn = new WalletTransaction { Id = payment.ReferenceId.Value, WalletId = Guid.NewGuid() };
        var wallet = new Wallet { Id = walletTxn.WalletId.Value, Balance = 50000 };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletTxnRepo.Setup(x => x.GetByIdAsync(payment.ReferenceId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(walletTxn);
        _walletRepo.Setup(x => x.GetByIdAsync(walletTxn.WalletId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("00");
        wallet.Balance.Should().Be(50000);
        walletTxn.Status.Should().Be(TransactionStatus.Failed);
        payment.Status.Should().Be((int)PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_WhenPaymentIsAlreadyProcessed_Returns02()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "5000000" },
            { "vnp_TxnRef", paymentId.ToString() }
        });

        var payment = new Payment { Id = paymentId, Status = (int)PaymentStatus.Paid };
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("02");
    }

    [Fact]
    public async Task Handle_WhenAmountParseFails_Returns04()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "NOT_A_NUMBER" },
            { "vnp_TxnRef", paymentId.ToString() }
        });

        var payment = new Payment { Id = paymentId, Status = (int)PaymentStatus.Pending };
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("04");
    }

    [Fact]
    public async Task Handle_WhenAmountMismatches_UpdatesStatusToFailedAndReturns04()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "4500000" },
            { "vnp_TxnRef", paymentId.ToString() }
        });

        var payment = new Payment { Id = paymentId, Amount = 50000, Status = (int)PaymentStatus.Pending }; // Expected: 5000000
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("04");
        payment.Status.Should().Be((int)PaymentStatus.Failed);
        _paymentRepo.Verify(x => x.AddEventAsync(It.Is<PaymentEvent>(e => e.EventType == "VNPAY_IPN_AMOUNT_MISMATCH"), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentIsFailed_CancelsOrderAndReturns00()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "6000000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "24" }
        });

        var order = new Order { Id = orderId };
        var payment = new Payment { Id = paymentId, OrderId = orderId, Amount = 60000, Status = (int)PaymentStatus.Pending, Order = order };
        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("00");
        payment.Status.Should().Be((int)PaymentStatus.Failed);

        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.OrderCancelled)), Times.Once);

        _paymentRepo.Verify(x => x.AddEventAsync(It.Is<PaymentEvent>(e => e.EventType == "VNPAY_IPN_FAILED"), It.IsAny<CancellationToken>()), Times.Once);
        _outboxSignal.Verify(x => x.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentIsSuccessful_WithVouchersPointsWallet_ProcessesAndReturns00()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var rowVersion = new byte[] { 1, 2, 3 };

        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "4000000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionNo", "VNP12345" }
        });

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Code = "ORD-CA-PHE",
            OrderSaleItems =
            [
                new() { Type = OrderSaleItemType.Voucher, ReferenceId = Guid.NewGuid() },
                new() { Type = OrderSaleItemType.LoyaltyPoint, Amount = 500 },
                new() { Type = OrderSaleItemType.Wallet, Amount = 15000 }
            ]
        };

        var payment = new Payment { Id = paymentId, OrderId = orderId, Amount = 40000, Status = (int)PaymentStatus.Pending, Order = order };

        var userWallet = new Wallet { Id = walletId, Balance = 50000, RowVersion = rowVersion };
        var user = new UserInfo { Id = customerId, Email = "customer@coffee.com", LoyaltyTier = new LoyaltyRes { CurrentTier = new MembershipTier { ValuePerPoint = 10 } } };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _walletRepo.Setup(x => x.GetByUserIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(userWallet);
        _identityClient.Setup(x => x.GetUserByIdAsync(customerId)).ReturnsAsync(user);

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("00");
        payment.Status.Should().Be((int)PaymentStatus.Paid);
        order.Status.Should().Be(OrderStatus.ConfirmationPending);
        order.PaidAmount.Should().Be(40000);

        // Đã đổi toàn bộ nameof(...) thành KafkaTopics... tương ứng
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.DeductUserVoucher)), Times.Once);
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.UpdateUserPoint)), Times.Once);
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.OrderNotifications)), Times.Once);
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.OrderPaid)), Times.Once);

        // 1. Sửa AddAsync thành Add (bỏ CancellationToken đi)
        _walletTxnRepo.Verify(x => x.Add(It.Is<WalletTransaction>(w => w.Amount == -15000)), Times.Once);

        // 2. Sửa DeductBalanceAsync thành ApplyDeductBalance cho khớp với IpnHandler
        _walletRepo.Verify(x => x.ApplyDeductBalance(userWallet, 15000, rowVersion), Times.Once);

        _paymentRepo.Verify(x => x.AddEventAsync(It.Is<PaymentEvent>(e => e.EventType == "VNPAY_IPN_SUCCESS"), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxSignal.Verify(x => x.Pulse(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIdentityThrowsException_StillProcessesPointsAndReturns00()
    {
        SetupMockConfig();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var queryParams = CreateValidVnPayParams(new Dictionary<string, string>
        {
            { "vnp_Amount", "5500000" },
            { "vnp_TxnRef", paymentId.ToString() },
            { "vnp_ResponseCode", "00" }
        });

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            OrderSaleItems =
            [
                new() { Type = OrderSaleItemType.LoyaltyPoint, Amount = 1000 }
            ]
        };

        var payment = new Payment { Id = paymentId, OrderId = orderId, Amount = 55000, Status = (int)PaymentStatus.Pending, Order = order };

        _paymentRepo.Setup(x => x.GetByIdWithOrderAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _identityClient.Setup(x => x.GetUserByIdAsync(customerId)).ThrowsAsync(new Exception("Identity service is down"));

        var result = await _handler.Handle(new VnPayIpnCommand(queryParams), CancellationToken.None);

        result.Value.RspCode.Should().Be("00");

        // Đã đổi nameof(UpdatePointEvent) thành KafkaTopics.UpdateUserPoint
        _outboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => m.Type == KafkaTopics.UpdateUserPoint)), Times.Once);
    }

    private void SetupMockConfig()
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s["HashSecret"]).Returns(HashSecret);
        _config.Setup(c => c.GetSection("VNPay")).Returns(section.Object);
    }

    private static Dictionary<string, string> CreateValidVnPayParams(Dictionary<string, string> inputParams)
    {
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

        var sortedParams = new SortedList<string, string>(vnpParams, StringComparer.Ordinal);

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
            queryBuilder.Length--;
        }

        var signData = queryBuilder.ToString();
        var hashBytes = new HMACSHA512(Encoding.UTF8.GetBytes(HashSecret)).ComputeHash(Encoding.UTF8.GetBytes(signData));
        var secureHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        inputParams["vnp_SecureHash"] = secureHash;
        return inputParams;
    }
}