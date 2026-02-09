using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace SalesService.Application.Features.Orders.Commands.Update
{
    public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, UpdateOrderRes>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxSignal _outboxSignal;
        private readonly ILogger<UpdateOrderHandler> _logger;

        public UpdateOrderHandler(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        ILogger<UpdateOrderHandler> logger)
        {
            _orderRepository = orderRepository;
            _productServiceClient = productServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _outboxRepository = outboxRepository;
            _outboxSignal = outboxSignal;
            _logger = logger;
        }
        public async Task<UpdateOrderRes> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if (request.Items == null || !request.Items.Any())
                throw new BusinessException("Order must contain at least one item.");

            //Load existing order
            var order = await _orderRepository.GetByIdAsync(request.OrderId!, cancellationToken);
            if (order == null)
                throw new BusinessException("Order not found.");

            //Check if currentUser is the onwer or staff
            var isStaff = request.RoleSlug != "customer";
            if (request.UserId != order.CustomerId && !isStaff)
                throw new BusinessException("Olny staff/admin or the owner can update order");


            //Only allow update when still Ordered
            if (order.Status != OrderStatus.PaymentPending)
                throw new BusinessException("Order cannot be updated after preparation has started.");

            //Get SKUs
            var skus = request.Items.Select(x => x.Sku).ToList();

            // Remove items that are no longer present 
            var itemsToRemove = order.OrderItems
                .Where(x => !skus.Contains(x.Sku))
                .ToList();

            foreach (var removed in itemsToRemove)
            {
                order.OrderItems.Remove(removed);
            }

            //Validate variants exist
            var variants = await _productServiceClient.GetListProductVariantsBySkusAsync(skus);

            if (variants.Count != skus.Count)
                throw new BusinessException("One or more product variants not found.");

            //Build new reservation list
            var newReserveItems = variants.Select(v => new AmountVariant
            {
                VariantId = v.VariantId,
                Amount = request.Items.First(i => i.Sku == v.Sku).Quantity
            }).ToList();

            //Update stock reservation
            //Release old reservation
            await _inventoryServiceClient.ReleaseStockAsync(new ReleaseStockReq
            {
                OrderId = order.Id,
                StoreId = order.StoreId!.Value
            });

            //Reserve new stock
            var reserveRes = await _inventoryServiceClient.ReserveStockAsync(new ReserveStockReq
            {
                OrderId = order.Id,
                StoreId = order.StoreId!.Value,
                Items = newReserveItems
            });

            if (!reserveRes.Success)
                throw new BusinessException($"Stock update failed: {reserveRes.Message}");

            // Remove orderItem - save change order
            decimal subtotal = 0;
            foreach (var item in request.Items)
            {
                var existing = order.OrderItems
                    .FirstOrDefault(x => x.Sku == item.Sku);

                if (existing != null)
                {
                    // Update quantity
                    existing.Quantity = item.Quantity;
                }
                else
                {
                    // Add new item
                    var variant = variants.First(v => v.Sku == item.Sku);
                    decimal unitPrice = variant.BasePrice.Value;
                    subtotal += unitPrice * item.Quantity;
                    order.OrderItems.Add(new Domain.Entities.OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = variant.ProductId,
                        VariantId = variant.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = variant.BasePrice.Value,
                        ProductName = variant.ProductName,
                        Sku = variant.Sku,
                        ImageUrl = variant.ImageUrl,
                        IsDeleted = false
                    });
                }
            }

            // Tax = 10%
            const decimal taxRate = 0.1m;
            var tax = subtotal * taxRate;
            var totalAmount = subtotal + tax;

            order.Subtotal = subtotal;
            order.Tax = tax;
            order.TotalAmount = totalAmount;

            // 10. Save update
            await _orderRepository.SaveAsync(cancellationToken);
            _logger.LogInformation("Order {OrderCode} updated successfully.", order.Code);

            return Result.Success(new UpdateOrderRes
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                OrderCode = order.Code,
                TotalAmount = order.TotalAmount,
                CheckEnoughStockRes = new CheckEnoughStockRes
                {
                    IsEnough = true
                }
            });
        }
    }
}
