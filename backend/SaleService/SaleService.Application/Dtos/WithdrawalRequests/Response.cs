using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.WithdrawalRequests;

public record WithdrawalSummaryDto
{
    public Guid Id { get; init; }
    public string RequestCode { get; init; } = null!;
    public Guid UserId { get; init; }
    public string Amount { get; set; } = "0.00";
    public string FeeAmount { get; set; } = "0.00";
    public string? BankCode { get; init; }
    public string BankName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string MaskedAccountNumber { get; init; } = null!;
    public WithdrawalStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record WithdrawalDetailDto : WithdrawalSummaryDto
{
    public string RawAccountNumber { get; init; } = null!;
    public string? Notes { get; init; }
    public string? InternalNotes { get; init; }
    public string? ApprovalNotes { get; init; }
    public string? RejectionReason { get; init; }
    public string? CancellationReason { get; init; }
    public string? ReceiptImageUrl { get; init; }
    public Guid? TransactionId { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public Guid? ProcessedById { get; init; }      // ID Admin/Kế toán xử lý
    public string? ProcessedByName { get; init; }  // Tên Admin (Join từ bảng User/Staff)
    public string? BankTransactionId { get; init; } // Mã GD từ Ngân hàng (Trace number)
    public DateTimeOffset? CompletedAt { get; init; } // Thời điểm thực sự hoàn tất
}