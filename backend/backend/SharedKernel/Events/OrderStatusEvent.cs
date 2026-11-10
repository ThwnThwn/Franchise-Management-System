 namespace SharedKernel.Events;
/// <summary>
/// Event phát ra khi có đơn hàng thay đổi trạng thái
/// Publisher: SaleService
/// Consumer: NotificationService, InventoryService (future)
/// </summary>
public class OrderStatusEvent
{
    /// <summary>
    /// ID của đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// OrderCode 
    /// </summary>
    public string OrderCode { get; set; } = default!;
    public string? Title { get; set; }
    public Guid? CustomerId { get; set; }
    /// <summary>
    /// Email khách hàng (để gửi xác nhận)
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;
    /// <summary>
    /// Tóm tắt đơn hàng (ví dụ: "2x Coffee, 1x Cake")
    /// </summary>
    public string OrderSummary { get; set; } = string.Empty;
    /// <summary>
    /// Lưu trạng thái :  
    /// 0. PaymentPending
    /// 1. ConfirmationPending 
    /// 2. Confirmed 
    /// 3. Preparing 
    /// 4. ReadyForPickup 
    /// 5. Delivering 
    /// 6. Completed 
    /// 7. Cancelled 
    /// </summary>
    public OrderStatus NewOrderStatus { get; set; }

    public List<CartItemDto> Items { get; set; } = new();
    public sealed class CartItemDto
    {
        public string Sku { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    /// <summary>
    /// Thue
    /// </summary>
    public decimal Tax { get; set; }
    /// <summary>
    /// Tổng tiền
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Thời điểm tạo đơn
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    public enum OrderStatus
    {
        PaymentPending = 0,
        ConfirmationPending = 1,
        Confirmed = 2,
        Preparing = 3,
        ReadyForPickup = 4,
        Delivering = 5,
        Completed = 6,
        Cancelled = 7
    }
}