using BuildingBlocks.Core.Pagination;
using MediatR;

namespace SalesService.Application.Features.Payments.GetMyPaymentHistory;

public sealed class GetMyPaymentHistoryQuery
    : PaginationRequest, IRequest<PagedResult<PaymentHistoryItemDto>>
{
    public Guid CustomerId { get; init; }

    /// <summary>Lọc từ ngày (inclusive). Null = không lọc.</summary>
    public DateTime? StartDate { get; init; }

    /// <summary>Lọc đến ngày (inclusive). Null = không lọc.</summary>
    public DateTime? EndDate { get; init; }

    /// <summary>Lọc theo trạng thái (0=Pending, 1=Paid, 2=Failed). Null = tất cả.</summary>
    public int? Status { get; init; }
}

public sealed class PaymentHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public int Status { get; set; }
    public DateTime? PaymentDate { get; set; }
}
