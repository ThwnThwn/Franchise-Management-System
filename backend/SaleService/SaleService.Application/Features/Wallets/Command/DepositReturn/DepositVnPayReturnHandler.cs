using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Payments;

namespace SalesService.Application.Features.Wallets.Deposit;

/// <summary>
/// Xử lý Return URL của VNPay sau khi user thanh toán nạp ví.
/// KHÔNG update DB ở đây — mọi cập nhật DB đã được VnPayIpnHandler lo.
/// Handler này chỉ xác thực chữ ký và trả kết quả để redirect UI.
/// </summary>
public sealed class DepositVnPayReturnHandler
    : IRequestHandler<DepositVnPayReturnCommand, Result<DepositVnPayReturnRes>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _config;

    public DepositVnPayReturnHandler(
        IPaymentRepository paymentRepository,
        IConfiguration config)
    {
        _paymentRepository = paymentRepository;
        _config = config;
    }

    public async Task<Result<DepositVnPayReturnRes>> Handle(
        DepositVnPayReturnCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.QueryParams;

        // ===== STEP 1: VERIFY SIGNATURE =====
        var hashSecret = _config["VNPay:HashSecret"];
        if (!query.TryGetValue("vnp_SecureHash", out var secureHash))
            return Result.Invalid(new ValidationError
            {
                Identifier = "vnp_SecureHash",
                ErrorMessage = "Missing vnp_SecureHash"
            });

        var vnpParams = query
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                      && !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                      && !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        if (!VNPayHelper.VerifySignature(hashSecret!, vnpParams, secureHash))
            return Result.Error("Invalid VNPay signature");

        // ===== STEP 2: PARSE PARAMS =====
        query.TryGetValue("vnp_TxnRef", out var txnRef);
        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        query.TryGetValue("vnp_Amount", out var amountStr);

        if (!Guid.TryParse(txnRef, out var paymentId))
            return Result.Invalid(new ValidationError
            {
                Identifier = "vnp_TxnRef",
                ErrorMessage = "Invalid vnp_TxnRef"
            });

        // ===== STEP 3: GET PAYMENT (chỉ để lấy amount hiển thị UI) =====
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result.NotFound("Payment not found");

        // ===== STEP 4: TRẢ KẾT QUẢ CHO UI =====
        // DB sẽ được cập nhật bởi IPN (VnPayIpnHandler), không update ở đây.
        var isSuccess = responseCode == "00";
        return Result.Success(new DepositVnPayReturnRes
        {
            Success = isSuccess,
            WalletTransactionId = payment.ReferenceId ?? Guid.Empty,
            Amount = payment.Amount,
            NewBalance = 0  // IPN sẽ update thực tế, UI lấy lại từ API riêng nếu cần
        });
    }
}