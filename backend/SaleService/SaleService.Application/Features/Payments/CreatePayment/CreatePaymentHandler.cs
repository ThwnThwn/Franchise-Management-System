using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using SalesService.Application.AppExceptions;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.Helpers;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Signals;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentHandler(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IKafkaProducer kafkaProducer,
    IConfiguration config,
    IOutboxRepository outboxRepository,
    IOutboxSignal outboxSignal,
    IEngagementServiceClient engagementServiceClient,
    IProductServiceClient productServiceClient,
    IInventoryServiceClient inventoryServiceClient,
    IIdentityServiceClient identityServiceClient,
    IWalletRepository walletRepository
) : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentRes>>
{
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
                ErrorMessage = "Thiếu thông tin id của order."
            });
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null || order.IsDeleted)
            return Result.NotFound($"Không tìm thấy order với Id: '{request.OrderId}'.");

        if (order.Status == OrderStatus.Completed)
            return Result.Error("Order này đã hoàn thành.");

        // Ràng buộc: Không cho tạo thêm Payment nếu đã có Payment thành công (Paid)
        var existingPaidPayment = await paymentRepository.GetNewestByOrderIdAndStatusAsync(request.OrderId, PaymentStatus.Paid, cancellationToken);
        if (existingPaidPayment != null)
        {
            return Result.Error("Đơn hàng này đã được thanh toán thành công trước đó, không thể thanh toán lại.");
        }

        bool isGuest = order.CustomerId == null || order.CustomerId == Guid.Empty;

        decimal totalProductDiscount = 0m;
        decimal totalShippingDiscount = 0m;

        if (!isGuest)
        {
            var customerId = order.CustomerId!.Value;
            // Hứng kết quả trả về từ Tuple
            var (prodDiscount, shipDiscount) = await ProcessVouchersAsync(request, order, customerId, cancellationToken);

            totalProductDiscount += prodDiscount;
            totalShippingDiscount += shipDiscount;

            // Điểm thưởng thường trừ vào tiền hàng
            totalProductDiscount += await ProcessLoyaltyAsync(request, order, customerId);
        }

        // Tính số tiền chịu thuế (chưa làm tròn)
        decimal taxableItemAmount = Math.Max(0, order.Subtotal - totalProductDiscount);
        decimal taxableShippingAmount = Math.Max(0, order.ShippingFee - totalShippingDiscount);
        decimal taxableAmount = taxableItemAmount + taxableShippingAmount;

        // Tính TỔNG TIỀN CHỐT HẠ (nhân 1.1 để thêm 10% thuế) và Làm tròn thành số nguyên
        order.TotalAmount = Math.Round(taxableAmount * 1.1m);

        // Tính ngược lại số THUẾ (Đảm bảo chắc chắn: TaxableAmount + Tax = TotalAmount)
        order.Tax = order.TotalAmount - taxableAmount;

        // TẠO PAYMENT RECORD
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Code = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = order.TotalAmount,
            Currency = "VND",
            Method = (int)request.PaymentMethod,
            Status = (int)PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // ÁP DỤNG VÍ (NẾU CÓ)
        Wallet? fetchedWallet = null;
        if (!isGuest && request.Wallet != null && request.Wallet > 0)
        {
            fetchedWallet = await ProcessWalletAsync(request, order, payment, order.CustomerId!.Value);
        }

        // Tiến hành commit
        if (request.PaymentMethod == PaymentMethod.Cash)
        {
            order.PaidAmount = payment.Amount;
            order.PaymentMethod = PaymentMethod.Cash;
            order.Status = OrderStatus.ConfirmationPending;
            order.PaidAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await paymentRepository.CreateAsync(payment, cancellationToken);
        await orderRepository.UpdateAsync(order, cancellationToken);

        // --- CASH FLOW ---
        if (request.PaymentMethod == PaymentMethod.Cash)
        {
            await ProcessCashFlowEventsAsync(request, order, fetchedWallet, cancellationToken);
            return Result.Success(new CreatePaymentRes { Url = "" });
        }

        // --- VNPAY FLOW ---
        var vnpSection = config.GetSection("VNPay");
        var paymentUrl = VNPayHelper.BuildPaymentUrl(
            vnpSection["PaymentUrl"]!, vnpSection["HashSecret"]!,
            BuildVNPayParams(vnpSection, order, payment, request.ReturnUrl)
        );

        return Result.Success(new CreatePaymentRes { Url = paymentUrl });
    }

    // =========================================================================
    // ======================== HELPER METHODS =================================
    // =========================================================================

    private async Task<(decimal ProductDiscount, decimal ShippingDiscount)> ProcessVouchersAsync(CreatePaymentReq request, Order order, Guid customerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VoucherDiscountCode) && string.IsNullOrWhiteSpace(request.VoucherShipCode))
            return (0, 0);

        decimal productDiscount = 0;
        decimal shippingDiscount = 0;

        var vouchers = await engagementServiceClient.GetMyVoucherAsync(customerId);
        var voucherMap = vouchers.ToDictionary(v => v.Code, StringComparer.OrdinalIgnoreCase);

        // Xử lý Voucher Sản Phẩm
        if (!string.IsNullOrWhiteSpace(request.VoucherDiscountCode))
        {
            if (!voucherMap.TryGetValue(request.VoucherDiscountCode, out var voucherDiscount))
                throw new BusinessException("Không tìm thấy voucher giảm giá sản phẩm được áp dụng");

            if ((voucherDiscount.MinOrderValue ?? 0) <= order.Subtotal)
            {
                if (voucherDiscount.Type == VoucherType.GIFT && voucherDiscount.VoucherItems.Count > 0)
                {
                    await HandleVoucherGiftItemsAsync(order, voucherDiscount);
                }
                else if (voucherDiscount.Type == VoucherType.DISCOUNT_PERCENTAGE)
                {
                    decimal calc = order.Subtotal * voucherDiscount.DiscountValue.GetValueOrDefault() / 100m;
                    decimal applied = voucherDiscount.MaxDiscountValue.HasValue ? Math.Min(calc, voucherDiscount.MaxDiscountValue.Value) : calc;
                    productDiscount += applied;
                    AddOrderSaleItem(order, $"Giảm giá SP: {voucherDiscount.Code}", applied, voucherDiscount.Id, OrderSaleItemType.Voucher);
                }
                else if (voucherDiscount.Type == VoucherType.DISCOUNT_FIXED_AMOUNT)
                {
                    productDiscount += voucherDiscount.DiscountValue.GetValueOrDefault();
                    AddOrderSaleItem(order, $"Giảm giá SP: {voucherDiscount.Code}", voucherDiscount.DiscountValue.GetValueOrDefault(), voucherDiscount.Id, OrderSaleItemType.Voucher);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.VoucherShipCode))
        {
            if (!voucherMap.TryGetValue(request.VoucherShipCode, out var voucherShip))
                throw new BusinessException("Không tìm thấy mã miễn phí vận chuyển được áp dụng");

            // Điều kiện áp dụng Freeship thường xét trên Subtotal
            if ((voucherShip.MinOrderValue ?? 0) <= order.Subtotal)
            {
                // Tính toán mức giảm: Không bao giờ được giảm vượt quá tiền ship thực tế
                decimal calcFreeship = voucherShip.Type == VoucherType.DISCOUNT_PERCENTAGE // Ít khi có freeship % nhưng cứ check cho chắc
                    ? order.ShippingFee * voucherShip.DiscountValue.GetValueOrDefault() / 100m
                    : voucherShip.DiscountValue.GetValueOrDefault(); // Hoặc lấy Maximum Fix Amount (ví dụ mã Giảm 15k phí ship)

                // Nếu voucherShip có MaxDiscountValue, áp dụng giới hạn
                decimal appliedShip = voucherShip.MaxDiscountValue.HasValue
                    ? Math.Min(calcFreeship, voucherShip.MaxDiscountValue.Value)
                    : calcFreeship;

                // BÓP CHẶT: Phí ship giảm không được lớn hơn Phí ship của đơn hàng
                appliedShip = Math.Min(appliedShip, order.ShippingFee);

                shippingDiscount += appliedShip;
                AddOrderSaleItem(order, $"Miễn phí Ship: {voucherShip.Code}", appliedShip, voucherShip.Id, OrderSaleItemType.Voucher);
            }
        }

        return (productDiscount, shippingDiscount);
    }

    private async Task HandleVoucherGiftItemsAsync(Order order, Voucher voucherDiscount)
    {
        // Kiểm tra Required Items
        var requiredItems = voucherDiscount.VoucherItems.Where(v => v.Type == VoucherItemType.ITEM_REQUIRED).ToList();
        foreach (var req in requiredItems)
        {
            var orderItem = order.OrderItems.FirstOrDefault(x => x.Sku == req.Sku);
            if (orderItem == null || orderItem.Quantity < req.Quantity)
                throw new BusinessException($"Voucher {voucherDiscount.Code} yêu cầu {req.Quantity} {req.Sku}");
        }

        // Cập nhật Inventory cho Gift Items
        var giftItems = voucherDiscount.VoucherItems.Where(v => v.Type == VoucherItemType.ITEM_GIFT).ToList();
        // List co thi moi chay
        if (giftItems.Count == 0) return;

        var skus = giftItems.Select(x => x.Sku ?? "").ToList();
        var variants = await productServiceClient.GetListProductVariantsBySkusAsync(skus);

        if (variants.Count != skus.Count)
            throw new Exception("Không tìm thấy đủ sản phẩm được tặng.");

        var newReserveItems = variants.Select(v => new AmountVariant
        {
            VariantId = v.VariantId,
            Sku = v.Sku,
            Amount = giftItems.First(i => i.Sku == v.Sku).Quantity
        }).ToList();

        // Inventory: Release & Reserve
        await inventoryServiceClient.ReleaseStockAsync(new ReleaseStockReq
        {
            OrderId = order.Id,
            StoreId = order.StoreId!.Value
        });

        var reserveRes = await inventoryServiceClient.ReserveStockAsync(new ReserveStockReq
        {
            OrderId = order.Id,
            StoreId = order.StoreId!.Value,
            Items = newReserveItems
        });
        if (!reserveRes.IsEnough) throw new Exception($"Cập nhật kho thất bại: {reserveRes.ShortageItems}");

        // Add OrderItems & SaleItems
        foreach (var item in giftItems)
        {
            var existing = order.OrderItems
                .FirstOrDefault(x => x.Sku == item.Sku);

            if (existing != null) existing.Quantity += item.Quantity;
            else
            {
                var variant = variants.First(v => v.Sku == item.Sku);
                order.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = variant.ProductId,
                    VariantId = variant.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = variant.BasePrice ?? 0,
                    ProductName = variant.ProductName,
                    Sku = variant.Sku,
                    ImageUrl = variant.ImageUrl,
                    IsDeleted = false
                });
            }
            AddOrderSaleItem(order, item.Sku ?? "", item.Quantity, item.VoucherId, OrderSaleItemType.Voucher);
        }
    }

    private async Task<decimal> ProcessLoyaltyAsync(CreatePaymentReq request, Order order, Guid customerId)
    {
        decimal loyaltyDiscount = 0;
        var user = await identityServiceClient.GetUserByIdAsync(customerId);
        if (user == null || user.LoyaltyTier == null) return 0;

        // 1. Point
        if (request.Point != null && request.Point > 0)
        {
            var pointBalance = await engagementServiceClient.GetMyPointByEmailAsync(user.Email);
            if (pointBalance < request.Point)
                throw new BusinessException("Điểm còn lại trong tài khoản không đủ.");

            decimal pointValue = request.Point.Value * user.LoyaltyTier.CurrentTier.ValuePerPoint;
            loyaltyDiscount += pointValue;
            AddOrderSaleItem(order, $"Dùng {request.Point} điểm", pointValue, user.LoyaltyTier.CurrentTier.Id, OrderSaleItemType.LoyaltyPoint);
        }

        // 2. Tier Discount
        if (user.LoyaltyTier?.CurrentTier != null)
        {
            decimal tierDiscount = order.Subtotal * (decimal)user.LoyaltyTier.CurrentTier.DiscountRate;
            if (tierDiscount > 0)
            {
                loyaltyDiscount += tierDiscount;
                AddOrderSaleItem(order, user.LoyaltyTier.CurrentTier.Name, tierDiscount, user.LoyaltyTier.CurrentTier.Id, OrderSaleItemType.DiscountRate);
            }
        }
        return loyaltyDiscount;
    }

    private async Task<Wallet> ProcessWalletAsync(CreatePaymentReq request, Order order, Payment payment, Guid customerId)
    {
        var userWallet = await walletRepository.GetByUserIdAsync(customerId)
            ?? throw new BusinessException("Không tìm thấy ví.");

        if (userWallet.Balance < request.Wallet)
            throw new BusinessException("Số dư ví không đủ.");

        payment.Amount -= request.Wallet!.Value;
        AddOrderSaleItem(order, "Dùng số dư ví", request.Wallet!.Value, userWallet.Id, OrderSaleItemType.Wallet);

        return userWallet;
    }

    private async Task ProcessCashFlowEventsAsync(CreatePaymentReq request,
        Order order, Wallet? userWallet,
        CancellationToken cancellationToken
    )
    {
        if (order.OrderItems.Count != 0)
        {
            var orderStatusEvent = OrderToKafka.MapToOrderStatusEvent(order);
            outboxRepository.AddEvent(orderStatusEvent, KafkaTopics.OrderNotifications);
            await kafkaProducer.PublishAsync(KafkaTopics.OrderNotifications, orderStatusEvent);

            var orderPaidEvent = OrderToKafka.MapToOrderPaidEvent(order);
            outboxRepository.AddEvent(orderPaidEvent, KafkaTopics.OrderPaid);
        }

        if (order.OrderSaleItems.Count != 0)
        {
            // Kafka Voucher
            var voucherIds = order.OrderSaleItems
                .Where(x => x.Type == OrderSaleItemType.Voucher && x.ReferenceId.HasValue)
                .Select(x => x.ReferenceId!.Value)
                .ToList();

            if (voucherIds.Count != 0)
            {
                await kafkaProducer.PublishAsync(KafkaTopics.DeductUserVoucher, new UseVouchersEvent
                {
                    OrderId = order.Id,
                    UserId = order.CustomerId!.Value,
                    VoucherIds = voucherIds
                });
            }

            // Kafka Point (Lấy thẳng từ request, không đọc nhầm Amount)
            if (request.Point != null && request.Point > 0)
            {
                await kafkaProducer.PublishAsync(KafkaTopics.UpdateUserPoint, new UpdatePointEvent
                {
                    Id = order.CustomerId!.Value,
                    OrderId = order.Id,
                    Point = request.Point.Value,
                    ActionType = UpdatePointType.Minus
                });
            }

            // Wallet Deduct
            var walletItem = order.OrderSaleItems.FirstOrDefault(x => x.Type == OrderSaleItemType.Wallet);
            if (userWallet != null && walletItem != null && walletItem.Amount > 0)
            {
                var rowVersion = userWallet.RowVersion
                    ?? throw new Exception("Thiếu row version.");

                await walletRepository.DeductBalanceAsync(userWallet.Id, walletItem.Amount, rowVersion, cancellationToken);
            }
        }
        outboxSignal.Pulse();
    }

    private static void AddOrderSaleItem(Order order, string name, decimal amount, Guid? refId, OrderSaleItemType type)
    {
        order.OrderSaleItems.Add(new OrderSaleItem
        {
            OrderId = order.Id,
            Name = name,
            Amount = amount,
            ReferenceId = refId,
            Type = type,
            IsDeleted = false
        });
    }

    private static Dictionary<string, string> BuildVNPayParams(
        IConfigurationSection vnpSection,
        Order order,
        Payment payment,
        string? returnUrl
    )
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
            ["vnp_Amount"] = ((long)(payment.Amount * 100)).ToString(),
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