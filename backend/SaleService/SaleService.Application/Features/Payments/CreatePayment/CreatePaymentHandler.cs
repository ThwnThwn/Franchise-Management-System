using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Features.Orders.Commands.Update;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentHandler
    : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentRes>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _config;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxSignal _outboxSignal;
    private readonly IEngagementServiceClient _engagementServiceClient;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IIdentityServiceClient _identityServiceClient;

    public CreatePaymentHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IKafkaProducer kafkaProducer,
        IConfiguration config,
        IOutboxRepository outboxRepository,
        IOutboxSignal outboxSignal,
        IEngagementServiceClient engagementServiceClient,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IIdentityServiceClient identityServiceClient)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _kafkaProducer = kafkaProducer;
        _config = config;
        _outboxRepository = outboxRepository;
        _outboxSignal = outboxSignal;
        _engagementServiceClient = engagementServiceClient;
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<Result<CreatePaymentRes>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Requets;
        if (request.OrderId == Guid.Empty)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(request.OrderId),
                ErrorMessage = "OrderId is required."
            });
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        decimal? voucherDiscountAmount = 0;
        if (order == null || order.IsDeleted)
            return Result.NotFound($"Order '{request.OrderId}' not found.");
        if (order.Status == OrderStatus.Completed)
            return Result.Error("Order already paid.");
        // Check CusomterID in Order
        if(order.CustomerId == Guid.Empty)
        {
            return Result.Error("CustomerID is null");
        }
        // ================= Sale Order Items =================
        // Get User's Vouchers
        if (!string.IsNullOrWhiteSpace(request.VoucherDiscountCode) || !string.IsNullOrWhiteSpace(request.VoucherShipCode))
        {
            var vouchers = await _engagementServiceClient.GetMyVoucherAsync(order.CustomerId.Value);
            var voucherMap = vouchers.ToDictionary(v => v.Code, StringComparer.OrdinalIgnoreCase);
            Voucher? voucherDiscount = null;
            Voucher? voucherShip = null;
            if (!string.IsNullOrWhiteSpace(request.VoucherDiscountCode) &&
                !voucherMap.TryGetValue(request.VoucherDiscountCode, out voucherDiscount))
            {
                return Result.NotFound("Voucher Discount not found or not owned by user");
            }

            if (!string.IsNullOrWhiteSpace(request.VoucherShipCode) &&
                !voucherMap.TryGetValue(request.VoucherShipCode, out voucherShip))
            {
                return Result.NotFound("Voucher Ship not found or not owned by user");
            }

            // Voucher Discount 
            if (voucherDiscount.MinOrderValue <= order.Subtotal)
            {
                
                if(voucherDiscount.VoucherItems.Count > 0)
                {
                    var voucherItem = voucherDiscount.VoucherItems;
                    var requiredItemsVoucher = voucherDiscount.VoucherItems
                        .Where(v => v.Type == VoucherItemType.ITEM_REQUIRED)
                        .ToList();

                    foreach ( var requiredItem in requiredItemsVoucher)
                    {
                        var orderItem = order.OrderItems.Where(x => x.Sku == requiredItem.Sku).FirstOrDefault();
                        if(orderItem.Quantity < requiredItem.Quantity)
                        {
                            throw new Exception($"Voucher {voucherDiscount.Code} require {requiredItem.Quantity} {requiredItem.Sku}");
                        }
                    }

                    var giftItems = voucherDiscount.VoucherItems
                        .Where(v => v.Type == VoucherItemType.ITEM_GIFT)
                        .ToList();
                    var skus = giftItems.Select(x => x.Sku).ToList();
                    //Validate variants exist
                    var variants = await _productServiceClient.GetListProductVariantsBySkusAsync(skus);

                    if (variants.Count != skus.Count)
                        throw new BusinessException("One or more product variants not found.");

                    //Build new reservation list
                    var newReserveItems = variants.Select(v => new AmountVariant
                    {
                        VariantId = v.VariantId,
                        Sku = v.Sku,
                        Amount = giftItems.First(i => i.Sku == v.Sku).Quantity
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

                    if (!reserveRes.IsEnough)
                        throw new BusinessException($"Stock update failed: {reserveRes.ShortageItems}");

                    // Add-Update orderItem - save change order
                    foreach (var item in giftItems)
                    {
                        var existing = order.OrderItems
                            .FirstOrDefault(x => x.Sku == item.Sku);

                        if (existing != null)
                        {
                            // Update quantity
                            existing.Quantity += item.Quantity;
                            var variant = variants.First(v => v.Sku == item.Sku);
                            decimal unitPrice = variant.BasePrice.Value;
                            order.OrderSaleItems.Add(new Domain.Entities.OrderSaleItem
                            {
                                OrderId = order.Id,
                                Name = item.Sku,
                                Amount = item.Quantity,
                                ReferenceId = item.VoucherId,
                                Type = OrderSaleItemType.Voucher,
                                IsDeleted = false
                            });
                        }
                        else
                        {
                            // Add new item
                            var variant = variants.First(v => v.Sku == item.Sku);
                            decimal unitPrice = variant.BasePrice.Value;
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
                }

                else if (voucherDiscount.Type.Equals(VoucherType.DISCOUNT_PERCENTAGE))
                {
                    voucherDiscountAmount = order.Subtotal * voucherDiscount.DiscountValue/100;
                    voucherDiscountAmount = voucherDiscountAmount > voucherDiscount.MaxDiscountValue ? voucherDiscount.MaxDiscountValue : voucherDiscountAmount;
                    order.OrderSaleItems.Add(new Domain.Entities.OrderSaleItem
                    {
                        OrderId = order.Id,
                        Name = voucherDiscount.Code,
                        Amount = voucherDiscount.DiscountValue.Value,
                        ReferenceId = voucherDiscount.Id,
                        Type = OrderSaleItemType.Voucher,
                        IsDeleted = false
                    });
                }
                else if (voucherDiscount.Type.Equals(VoucherType.DISCOUNT_FIXED_AMOUNT))
                {
                    voucherDiscountAmount = voucherDiscount.DiscountValue;
                    order.OrderSaleItems.Add(new Domain.Entities.OrderSaleItem
                    {
                        OrderId = order.Id,
                        Name = voucherDiscount.Code,
                        Amount = voucherDiscount.DiscountValue.Value,
                        ReferenceId = voucherDiscount.Id,
                        Type = OrderSaleItemType.Voucher,
                        IsDeleted = false
                    });
                }
            }
        }

        // Loyalty 
        if(request.Point != null)
        {
            // Point Engagement
            var user = await _identityServiceClient.GetUserByIdAsync(order.CustomerId.Value);

            if(user != null)
            {
                var point = await _engagementServiceClient.GetMyPointByEmailAsync(user.Email);
                if (point < request.Point)
                {
                    return Result.NotFound("User point is lower then request Point");
                }
                voucherDiscountAmount += request.Point * user.LoyaltyTier.CurrentTier.ValuePerPoint;
                order.OrderSaleItems.Add(new Domain.Entities.OrderSaleItem
                {
                    OrderId = order.Id,
                    Name = user.LoyaltyTier.CurrentTier.Name,
                    Amount = voucherDiscountAmount.Value,
                    ReferenceId = user.LoyaltyTier.CurrentTier.Id,
                    Type = OrderSaleItemType.LoyaltyPoint,
                    IsDeleted = false
                });
            }
        }

        // ================= CREATE PAYMENT =================
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Code = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = order.TotalAmount - voucherDiscountAmount.Value < 0 ? 0 : order.TotalAmount - voucherDiscountAmount.Value,
            Currency = "VND",
            Method = (int)request.PaymentMethod,// 0 = Cash | 1 = VNPay
            Status = (int)PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        // ==========================================================
        // ================= CASH FLOW ==============================
        // ==========================================================
        if (request.PaymentMethod == PaymentMethod.Cash)
        {
            // ===== UPDATE ORDER =====
            order.PaidAmount = payment.Amount;
            order.PaymentMethod = PaymentMethod.Cash;
            order.Status = OrderStatus.ConfirmationPending;
            order.PaidAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            // ===== OUTBOX EVENTS =====
            if (order.OrderItems.Any())
            {
                var orderStatusEvent = OrderToKafka.MapToOrderStatusEvent(order);
                _outboxRepository.AddEvent(orderStatusEvent, KafkaTopics.OrderNotifications);
                await _kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderStatusEvent);

                // 2. OrderPaidEvent → InventoryService (trừ kho)
                var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(order);
                _outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);
            }

            _outboxSignal.Pulse();

            return Result.Success(new CreatePaymentRes { Url = "" });
        }

        // ==========================================================
        // ================= VNPAY FLOW =============================
        // ==========================================================

        var vnpSection = _config.GetSection("VNPay");

        var paymentUrl = VNPayHelper.BuildPaymentUrl(
            vnpSection["PaymentUrl"]!,
            vnpSection["HashSecret"]!,
            BuildVNPayParams(vnpSection, order, payment, request.ReturnUrl)
        );

        return Result.Success(new CreatePaymentRes
        {
            Url = paymentUrl
        });
    }

    private Dictionary<string, string> BuildVNPayParams(
        IConfigurationSection vnpSection,
        Order order,
        Payment payment,
        string? returnUrl)
    {
        var callbackUrl = vnpSection["ReturnUrl"]!;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            callbackUrl += $"?redirectTo={Uri.EscapeDataString(returnUrl)}";
        }

        return new Dictionary<string, string>
        {
            ["vnp_Version"] = vnpSection["Version"] ?? "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnpSection["TmnCode"]!,
            ["vnp_Amount"] = ((long)(order.TotalAmount * 100)).ToString(),
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = "127.0.0.1",
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {order.Code}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = callbackUrl,
            ["vnp_TxnRef"] = payment.Id.ToString()
        };
    }
}