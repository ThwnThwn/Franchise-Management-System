namespace SalesService.Application.Features.Orders.Queries.GetMyOrders
{
    public record GetMyOrdersResponse(List<OrderSummaryDto> Orders, int TotalCount);

    public record OrderSummaryDto(
        Guid Id,
        string Code,
        decimal Subtotal,
        decimal? Tax,
        decimal TotalAmount,
        decimal? PaidAmount,
        int? PaymentMethod,
        int Status,
        DateTimeOffset PaidAt,
        string? Notes,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );
}
