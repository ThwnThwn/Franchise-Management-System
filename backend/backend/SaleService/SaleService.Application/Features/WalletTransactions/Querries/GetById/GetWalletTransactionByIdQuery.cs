using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Dtos;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetById;

public sealed record GetWalletTransactionByIdQuery(
    Guid TransactionId,
    Guid LoggedUserId
) : IRequest<Result<WalletTransactionResponse>>;

//public sealed class WalletTransactionResponse
//{
//    public Guid Id { get; set; }
//    public Guid WalletId { get; set; }
//    public Guid UserId { get; set; }
//    public Guid? OrderId { get; set; }
//    public Guid? InitiatedBy { get; set; }
//    public Guid? ProcessedBy { get; set; }
//    public Guid? ParentTransactionId { get; set; }
//    public string TransactionCode { get; set; } = null!;
//    public string? Notes { get; set; }
//    public decimal Amount { get; set; }
//    public decimal? BalanceBefore { get; set; }
//    public decimal? BalanceAfter { get; set; }
//    public TransactionType TransactionType { get; set; }
//    public TransactionStatus Status { get; set; }
//    public Guid? ReferenceId { get; set; }
//    public WalletReferenceType? ReferenceType { get; set; }
//    public string? Description { get; set; }
//    public string? ExternalTransactionId { get; set; }
//    public DateTimeOffset CreatedAt { get; set; }
//    public DateTimeOffset UpdatedAt { get; set; }
//    public DateTimeOffset? CompletedAt { get; set; }
//    public DateTimeOffset? FailedAt { get; set; }
//    public string? FailureReason { get; set; }
//    public string? Metadata { get; set; }
//    public PaymentMethod? PaymentMethod { get; set; }
//    public DateTimeOffset? ProcessedAt { get; set; }
//}