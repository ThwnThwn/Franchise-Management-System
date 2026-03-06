using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.WithdrawalRequests;

public record WithdrawalSummaryDto
{
    public Guid Id { get; init; }
    public string RequestCode { get; init; } = null!;
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal FeeAmount { get; init; }
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
}