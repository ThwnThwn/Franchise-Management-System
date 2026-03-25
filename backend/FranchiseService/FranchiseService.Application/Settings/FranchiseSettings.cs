namespace FranchiseService.Application.Settings;

/// <summary>
/// Cấu hình mặc định cho hợp đồng nhượng quyền.
/// Có thể thay đổi qua Environment Variables mà không cần sửa code.
/// </summary>
public class FranchiseSettings
{
    /// <summary>
    /// Tỷ lệ chia sẻ doanh thu mặc định (%). Ví dụ: 5 = 5%.
    /// Env: FRANCHISE_DEFAULT_REVENUE_SHARE
    /// </summary>
    public decimal DefaultRevenueSharePercentage { get; set; } = 5.0m;

    /// <summary>
    /// Thời hạn hợp đồng mặc định (năm). Tiêu chuẩn ngành: 3-5 năm.
    /// Env: FRANCHISE_DEFAULT_DURATION_YEARS
    /// </summary>
    public int DefaultDurationYears { get; set; } = 3;

    /// <summary>
    /// Tiền cọc mặc định (VNĐ). Mặc định: 50.000.000 VNĐ.
    /// Env: FRANCHISE_DEFAULT_DEPOSIT_AMOUNT
    /// </summary>
    public decimal DefaultDepositAmount { get; set; } = 50000000m;

    /// <summary>
    /// Phí nhượng quyền mặc định (VNĐ). Mặc định: 100.000.000 VNĐ.
    /// Env: FRANCHISE_DEFAULT_FRANCHISE_FEE
    /// </summary>
    public decimal DefaultFranchiseFee { get; set; } = 100000000m;
}
