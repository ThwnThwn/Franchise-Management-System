export enum PaymentMethod {
    Unknown = -1,
    Cash = 0,
    VNPay = 1
}

// export enum OrderStatus {
//     PaymentPending = 0,
//     Preparing = 1,
//     Ready = 2,
//     Completed = 3,
//     Cancel = 4
// }

export enum OrderStatus {
    // Auto
    PaymentPending = 0,

    // Manual
    ConfirmationPending = 1, // Thanh toán xong qua đây
    Confirmed = 2, // Nhà hàng confirm nhận đơn, nếu thanh toán online thì cần phải xem thanh toán chưa, nếu paid thì cũng ko tự chuyển sang, này là bước chủ động hoàn toàn
    Preparing = 3, // Nhà hàng chủ động chuyển
    ReadyForPickup = 4,
    Delivering = 5, // Đang đi giao

    // Terminal
    Completed = 6, // Đã hoàn thành đơn
    Cancelled = 7
}

/**
 * Định nghĩa các bước chuyển trạng thái hợp lệ
 */
export const ORDER_STATUS_TRANSITION_POLICY: Record<OrderStatus, OrderStatus[]> = {
    [OrderStatus.PaymentPending]: [OrderStatus.ConfirmationPending, OrderStatus.Cancelled],
    [OrderStatus.ConfirmationPending]: [OrderStatus.Confirmed],
    [OrderStatus.Confirmed]: [OrderStatus.Preparing],
    [OrderStatus.Preparing]: [OrderStatus.ReadyForPickup],
    [OrderStatus.ReadyForPickup]: [OrderStatus.Delivering, OrderStatus.Completed],
    [OrderStatus.Delivering]: [OrderStatus.Completed],
    [OrderStatus.Completed]: [],
    [OrderStatus.Cancelled]: []
}

export enum OrderSaleItemType {
    VOUCHER = 0,
    WALLET = 1,
    POINT = 2,
    PROMOTION = 3
}
