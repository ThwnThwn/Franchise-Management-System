
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Signals;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace SalesService.Application.Features.Payments.VnPayReturn;

public sealed class VnPayReturnHandler
    : IRequestHandler<VnPayReturnCommand, Result<VnPayReturnResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayReturnHandler> _logger;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IWalletRepository _walletRepository;

    public VnPayReturnHandler(
        IPaymentRepository paymentRepository,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<VnPayReturnHandler> logger,
        IKafkaProducer kafkaProducer,
        IWalletRepository walletRepository)
    {
        _paymentRepository = paymentRepository;
        _config = config;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _walletRepository = walletRepository;
    }

    public async Task<Result<VnPayReturnResponse>> Handle(
        VnPayReturnCommand request,
        CancellationToken cancellationToken)
    {
        var vnpSection = _config.GetSection("VNPay");
        var hashSecret = vnpSection["HashSecret"];
        if (string.IsNullOrWhiteSpace(hashSecret))
        {
            return Result.Error("VNPay HashSecret is not configured");
        }

        var query = request.QueryParams;
        if (!query.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
        {
            return Result.Invalid(new[]
            {
                new ValidationError
                {
                    Identifier = "vnp_SecureHash",
                    ErrorMessage = "Missing vnp_SecureHash"
                }
            });
        }

        // lấy tất cả tham số bắt đầu với vnp_ để kiểm tra chữ ký
        var vnpParams = new Dictionary<string, string>();
        foreach (var kv in query)
        {
            if (!kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                continue;

            vnpParams[kv.Key] = kv.Value;
        }

        // xác thực chữ ký
        var isValidSignature = VNPayHelper.VerifySignature(hashSecret, vnpParams, vnpSecureHash);
        if (!isValidSignature)
        {
            return Result.Error("Invalid VNPay signature");
        }

        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_TransactionNo", out var transactionNo);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Invalid(new[]
            {
                new ValidationError
                {
                    Identifier = "vnp_TxnRef",
                    ErrorMessage = "Invalid vnp_TxnRef"
                }
            });
        }

        var payment = await _paymentRepository.GetByIdWithOrderAsync(paymentId, cancellationToken);
        var order = payment.Order;
        if (payment == null)
        {
            return Result.NotFound("Payment not found");
        }

        // Chỉ kiểm tra logic để hiển thị cho UI (KHÔNG LƯU DB, KHÔNG BẮN KAFKA)
        // Mọi việc cập nhật dữ liệu để VnPayIpnHandler lo gánh
        var isSuccess = responseCode == "00";

        if (isSuccess)
        {
            //=================
            // Kakfa Event Voucher
            //=================
            List<Guid> voucherList = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.Voucher)
                .Select(x => x.ReferenceId.Value)
                .ToList();
            if (voucherList.Any())
            {
                UseVouchersEvent userVoucherEvent = new UseVouchersEvent
                {
                    OrderId = order.Id,
                    UserId = order.CustomerId.Value,
                    VoucherIds = voucherList
                };
                _kafkaProducer.PublishAsync(KafkaTopics.DeductUserVoucher, userVoucherEvent);
            }
            //=================
            // Kakfa Event Point
            //=================
            decimal point = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.LoyaltyPoint)
                .Select(x => x.Amount)
                .FirstOrDefault(); 
            if(point != 0 || point != null)
            {
                DeductPointEvent userPoint = new DeductPointEvent
                {
                    Id = order.CustomerId.Value,
                    OrderId = order.Id,
                    PointToDeduct = point
                };
                _kafkaProducer.PublishAsync(KafkaTopics.DedcutUserPoint, userPoint);
            }
            //=================
            // Wallet Event 
            //=================
            var userWallet = await _walletRepository.GetByUserIdAsync(order.CustomerId.Value);
            decimal walletAmount = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.Wallet)
                .Select(x => x.Amount)
                .FirstOrDefault();
            if(walletAmount != 0 || walletAmount != 0)
            {
                await _walletRepository.DeductBalanceAsync(userWallet.Id, walletAmount, userWallet.RowVersion);

            }
            //=================
            // log
            _logger.LogInformation("VNPay Return: Giao dịch thành công cho PaymentId {PaymentId}. Chờ IPN xử lý DB.", payment.Id);
        }
        else
        {
            _logger.LogWarning("VNPay Return: Giao dịch thất bại cho PaymentId {PaymentId} (Mã: {ResponseCode}).", payment.Id, responseCode);
        }

        return Result.Success(new VnPayReturnResponse
        {
            Success = isSuccess,
            PaymentId = payment.Id,
            OrderId = payment.OrderId.Value,
            ResponseCode = responseCode ?? string.Empty
        });
    }

}

