namespace SharedKernel.Constants;

/// <summary>
/// Tập trung định nghĩa tên các Kafka Topics được sử dụng trong hệ thống.
/// Tránh hardcode "magic strings" rải rác trong code.
/// </summary>
public static class KafkaTopics
{
    /// <summary>Thông báo tồn kho thấp từ InventoryService</summary>
    public const string LowStockNotifications = "low-stock-notifications";
    
    /// <summary>Thông báo OTP từ IdentityService</summary>
    public const string OtpNotifications = "otp-notifications";

    public const string OtpForgetPassword = "otp-forgetPassword";

    /// <summary>Thông báo đơn hàng mới từ SaleService</summary>
    public const string OrderNotifications = "order-notifications";
    
    /// <summary>Thông báo reset mật khẩu từ IdentityService</summary>
    public const string PasswordResetNotifications = "password-reset-notifications";

    /// <summary>Sự kiện đơn hàng đã thanh toán (Inventory listen)</summary>
    public const string OrderPaid = "order-paid";

    /// <summary>Sự kiện đơn hàng đã hủy (Inventory listen)</summary>
    public const string OrderCancelled = "order-cancelled";
}
