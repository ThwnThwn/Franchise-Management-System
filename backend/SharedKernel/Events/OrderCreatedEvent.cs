namespace SharedKernel.Events;
/// <summary>
/// Event phát ra khi có đơn hàng mới được tạo
/// Publisher: SaleService
/// Consumer: NotificationService, InventoryService (future)
/// </summary>
public class OrderCreatedEvent
{
    /// <summary>
    /// ID của đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Email khách hàng (để gửi xác nhận)
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;
    /// <summary>
    /// Tóm tắt đơn hàng (ví dụ: "2x Coffee, 1x Cake")
    /// </summary>
    public string OrderSummary { get; set; } = string.Empty;
    /// <summary>
    /// Tổng tiền
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// Thời điểm tạo đơn
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}