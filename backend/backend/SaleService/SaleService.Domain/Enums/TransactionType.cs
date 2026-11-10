namespace SalesService.Domain.Enums
{
    public enum TransactionType
    {
        Deposit = 0, // Nạp tiền
        Payment = 1, // Thanh toán đơn hàng
        Refund = 2, // Hoàn tiền
        Adjustment = 3, // InternalUser điều chỉnh
        Withdrawal = 4,  // Rút tiền 
        Transfer = 5
    }
}
