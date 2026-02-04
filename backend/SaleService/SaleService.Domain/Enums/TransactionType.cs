using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Domain.Enums
{
    public enum TransactionType
    {
    Deposit          = 0, // Nạp tiền
    Payment          = 1, // Thanh toán đơn hàng
    Refund           = 2, // Hoàn tiền
    Adjustment       = 3, // Admin điều chỉnh
    Withdrawal       = 4  // Rút tiền 
}
}
