using MediatR;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Features.Payments.GetAll;

public sealed class GetAllPaymentsQuery
    : PaginationRequest, IRequest<BuildingBlocks.Core.Pagination.PagedResult<PaymentListItemDto>>
{
}

public sealed class PaymentListItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Code { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public int Method { get; set; }
    public int Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

