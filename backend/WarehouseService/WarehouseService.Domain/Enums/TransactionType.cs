namespace WarehouseService.Domain.Enums
{
    public enum TransactionType
    {
        StockIn = 1,        // Nhập kho (từ supplier)
        StockOut = 2,       // Xuất kho (bán / tiêu thụ)
        TransferIn = 3,     // Nhập từ chuyển kho
        TransferOut = 4,    // Xuất do chuyển kho
        Adjustment = 5,     // Điều chỉnh tồn kho
        Return = 6          // Hàng trả về
    }
}
