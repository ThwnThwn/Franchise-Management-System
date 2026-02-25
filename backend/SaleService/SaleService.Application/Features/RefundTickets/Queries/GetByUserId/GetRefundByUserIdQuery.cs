using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.RefundTickets.Queries.GetByUserId;

public sealed record GetRefundByUserIdQuery(
    Guid UserIdToSearch,Guid LoggedUserId,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<GetRefundByUserIdResponse>>>;
public sealed class GetRefundByUserIdResponse
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public Guid WalletId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid OriginalTransactionId { get; set; }
    public RefundTicketStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}