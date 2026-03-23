using Ardalis.Result;
using MediatR;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.Franchise;
using SalesService.Application.Interfaces.Geocoding;
using SalesService.Application.Constants;

namespace SalesService.Application.Features.Orders.Commands.Create;

public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly IFranchiseServiceClient _franchiseServiceClient;
    private readonly IDistanceService _distanceService;
    private readonly IGeocodingService _geocodingService;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        ICartItemRepository cartItemRepository,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        IFranchiseServiceClient franchiseServiceClient,
        IDistanceService distanceService,
        IGeocodingService geocodingService)
    {
        _orderRepository = orderRepository;
        _cartItemRepository = cartItemRepository;
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _franchiseServiceClient = franchiseServiceClient;
        _distanceService = distanceService;
        _geocodingService = geocodingService;
    }

    public async Task<Result<CreateOrderResponseDto>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Items == null || request.Request.Items.Count == 0)
        {
            return Result.Error("Cart is empty. Please add items to cart.");
        }

        // Validate: Mỗi item phải có SKU
        foreach (var item in request.Request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                return Result.Error($"Item is missing SKU. Please provide SKU for each item.");
            }
        }

        // check storeId
        if (!request.Request.StoreId.HasValue || request.Request.StoreId == Guid.Empty)
        {
            return Result.Error("StoreId is required. Please select a store before placing an order.");
        }

        // Default to Pickup if Type is not provided
        request.Request.Type ??= OrderType.Pickup;

        //check orderType
        if (!Enum.IsDefined(typeof(OrderType), request.Request.Type.Value))
        {
            return Result.Error("Invalid OrderType.");
        }

        if (request.Request.ScheduledTime.HasValue && request.Request.Type != OrderType.Delivery)
        {
            throw new BusinessException("ScheduledTime can only be set for type Delivery orders.");
        }

        //if (request.Request.ScheduledTime.HasValue && request.Request.Type == OrderType.Delivery)
        //{
        //    var now = DateTime.Now;
        //    var scheduledDateTime = DateTime.Today.Add(request.Request.ScheduledTime.Value.ToTimeSpan());
        //    if (scheduledDateTime < now.AddMinutes(30))
        //    {
        //        throw new BusinessException("Scheduled time must be at least 30 minutes from now");
        //    }
        //}

        // Validate ScheduledTime
        if (request.Request.ScheduledTime.HasValue)
        {
            var scheduledTime = request.Request.ScheduledTime.Value;
            var tzId = DateConstants.GetVietnamTimeZoneId();
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var currentTime = TimeOnly.FromTimeSpan(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).TimeOfDay);
            var minPreparationTime = TimeSpan.FromMinutes(30);

            if (scheduledTime < currentTime.Add(minPreparationTime))
            {
                throw new BusinessException("Scheduled time must be at least 30 minutes in the future.");
            }
        }

        //kiểm tra xem còn đủ hàng trong kho không
        var skus = request.Request.Items.Select(i => i.Sku).ToList();

        var variantInfo = await _productServiceClient.GetListProductVariantsBySkusAsync(skus);

        if (variantInfo.Count != skus.Count)
        {
            return Result.Error("One or more product variants not found or inactive.");
        }

        IEnumerable<AmountVariant> listItemCheck = variantInfo.Select(v => new AmountVariant
        {
            VariantId = v.VariantId,
            Sku = v.Sku,
            Amount = request.Request.Items.First(i => i.Sku == v.Sku).Quantity
        });

        // Generate OrderId and Code early
        var orderId = Guid.NewGuid();
        var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        // Reserve Stock
        var reserveReq = new ReserveStockReq
        {
            StoreId = request.Request.StoreId.Value,
            OrderId = orderId,
            Items = listItemCheck.ToList()
        };
        var reserveRes = await _inventoryServiceClient.ReserveStockAsync(reserveReq);

        if (!reserveRes.IsEnough)
        {
            foreach (var item in reserveRes.ShortageItems ?? [])
            {
                item.Sku = variantInfo.FirstOrDefault(v => v.VariantId == item.VariantId)?.Sku ?? item.Sku;
            }
            return Result.Success(new CreateOrderResponseDto
            {
                OrderId = Guid.Empty,
                OrderCode = "Failed",
                TotalAmount = 0,
                CheckEnoughStockRes = reserveRes
            });
        }

        // Gọi ProductService để lấy thông tin sản phẩm từ SKU
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Request.Items)
        {
            // Gọi ProductService để lấy thông tin từ SKU
            //var productVariant = await _productServiceClient.GetProductVariantBySkuAsync(item.Sku);

            var productVariant = variantInfo.FirstOrDefault(v => v.Sku == item.Sku);

            if (productVariant == null)
            {
                return Result.Error($"Product variant with SKU '{item.Sku}' not found or inactive.");
            }

            if (!productVariant.BasePrice.HasValue || productVariant.BasePrice <= 0)
            {
                return Result.Error($"Product variant with SKU '{item.Sku}' has invalid price.");
            }

            decimal unitPrice = productVariant.BasePrice.Value;
            subtotal += unitPrice * item.Quantity;

            orderItems.Add(new OrderItem
            {
                OrderId = orderId,
                ProductId = productVariant.ProductId,
                VariantId = productVariant.VariantId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                ProductName = productVariant.ProductName,
                Sku = productVariant.Sku,
                ImageUrl = productVariant.ImageUrl,
                IsDeleted = false
            });
        }

        // shipping fee logic
        decimal shippingFee = 0;
        double? currentLat = request.Request.Latitude;
        double? currentLong = request.Request.Longitude;
        decimal? calculatedDistance = null;

        if (request.Request.Type == OrderType.Delivery)
        {
            shippingFee = 15000m; // Baseline fee for all delivery orders

            // Nếu thiếu tọa độ nhưng có địa chỉ -> Tự động Geocode
            if ((!currentLat.HasValue || !currentLong.HasValue) && !string.IsNullOrWhiteSpace(request.Request.Address))
            {
                var geocodeResult = await _geocodingService.GeocodeAsync(request.Request.Address);
                if (geocodeResult != null)
                {
                    currentLat = geocodeResult.Latitude;
                    currentLong = geocodeResult.Longitude;
                }
                else
                {
                    return Result.Error("Could not determine location from the provided address. Please check your address again.");
                }
            }

            // Ensure we have coordinates for delivery
            if (!currentLat.HasValue || !currentLong.HasValue)
            {
                return Result.Error("Delivery coordinates or a valid address are required to place a delivery order.");
            }

            // Logic bậc thang tính phí ship
            // 2km đầu = 15k
            // 3km tiếp = 3 × 4k = 12k
            // 2km tiếp = 2 × 6k = 12k
            // fee = 39k

            if (currentLat.HasValue && currentLong.HasValue)
            {
                var storeDetail = await _franchiseServiceClient.GetStoreDetailAsync(request.Request.StoreId.Value);
                if (storeDetail != null)
                {
                    var distance = await _distanceService.CalculateDistanceAsync(
                        storeDetail.Latitude, storeDetail.Longitude,
                        currentLat.Value, currentLong.Value
                    );
                    calculatedDistance = Math.Round((decimal)distance, 2);

                    // 5k 1 cây để base 15 dưới 3 cây lấy 15k hết

                    if (distance <= 2)
                    {
                        shippingFee = 15000m;
                    }
                    else if (distance <= 5)
                    {
                        // 2km đầu 15k + phần vượt ngưỡng 2km nhân 4k
                        var extraKm = Math.Ceiling(distance - 2.0);
                        shippingFee = 15000m + (decimal)extraKm * 4000m;
                    }
                    else
                    {
                        // 5km đầu là 27k (15k + 3*4k) + phần vượt ngưỡng 5km nhân 6k
                        var extraKm = Math.Ceiling(distance - 5.0);
                        shippingFee = 27000m + (decimal)extraKm * 6000m;
                    }
                }
            }
        }

        decimal preTaxAmount = subtotal + shippingFee;
        decimal totalAmount = Math.Round(preTaxAmount * 1.1m);
        decimal tax = totalAmount - preTaxAmount;

        var order = new Order
        {
            Id = orderId,
            Code = orderCode,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            Tax = tax,
            TotalAmount = totalAmount,
            StoreId = request.Request.StoreId, // đã validate khác null ở trên
            PaymentMethod = null,
            Status = OrderStatus.PaymentPending, // 0 = Ordered
            Notes = request.Request.Notes,
            CustomerId = request.Request.CustomerId,
            IsDeleted = false,
            //Type = request.Request.Type.Value,
            Type = request.Request.Type.Value,
            ScheduledTime = request.Request.ScheduledTime,
            Address = request.Request.Address,
            Latitude = currentLat,
            Longitude = currentLong,
            Distance = calculatedDistance
        };

        // Add OrderItems
        foreach (var orderItem in orderItems)
        {
            order.OrderItems.Add(orderItem);
        }

        // 1. Prepare Outbox Message (BEFORE saving order)
        // Outbox: KHÔNG publish OrderStatusEvent khi PaymentPending
        // → Chỉ notify khách khi đơn hàng đã thanh toán (ConfirmationPending trở đi)

        //Remove orderd items in cartItem
        await _cartItemRepository.RemoveItemsAsync(order.CustomerId, skus, cancellationToken);

        // 2. Save Order (and OutboxMessage) in one transaction (implicitly handled by CreateAsync -> SaveChanges)
        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

        // Optimize: Pulse Signal
        if (order.CustomerId.HasValue)
        {
            _outboxSignal.Pulse();
        }
        return Result.Success(new CreateOrderResponseDto
        {
            OrderId = createdOrder.Id,
            OrderCode = createdOrder.Code,
            Items = createdOrder.OrderItems.Select(x => new OrderItemDto
            {
                Sku = x.Sku,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList(),
            StoreId = request.Request.StoreId,
            Type = createdOrder.Type,
            ScheduledTime = createdOrder.ScheduledTime,
            TotalAmount = createdOrder.TotalAmount,
            ShippingFee = createdOrder.ShippingFee,
            Address = createdOrder.Address,
            Latitude = currentLat,
            Longitude = currentLong,
            Distance = calculatedDistance,
            CheckEnoughStockRes = { IsEnough = true, ShortageItems = { } }
        });
    }
}