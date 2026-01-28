using Ardalis.Result;
using MediatR;
using SalesService.Application.Interfaces.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.GetById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetOrderByIdResponse>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (query.OrderId == Guid.Empty)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(query.OrderId),
                ErrorMessage = "OrderId is required."
            });
        }

        var order = await _orderRepository.GetByIdAsync(
            query.OrderId,
            cancellationToken);

        if (order == null || order.IsDeleted)
        {
            return Result.NotFound($"Order with id '{query.OrderId}' not found.");
        }

        var response = new GetOrderByIdResponse
        {
            OrderId = order.Id,
            OrderCode = order.Code,
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            PaidAmount = order.PaidAmount,
            PaymentMethod = order.PaymentMethod.HasValue
                ? (PaymentMethod?)order.PaymentMethod.Value
                : null,
            Status = (int)order.Status,
            PaidAt = order.PaidAt,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CustomerId = order.CustomerId,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ProductName = item.ProductName,
                Sku = item.Sku,
                ImageUrl = item.ImageUrl,
                ProductId = item.ProductId,
                VariantId = item.VariantId
            }).ToList()
        };

        return Result.Success(response);
    }
}
