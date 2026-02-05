import { OrderStatus, PaymentMethod } from "./enum"

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
    [PaymentMethod.Unknown]: "Chưa chọn",
    [PaymentMethod.Cash]: "Tiền mặt",
    [PaymentMethod.VNPay]: "VNPay"
}

export const OrderStatusLabels: Record<OrderStatus, string> = {
    [OrderStatus.PaymentPending]: "Chưa thanh toán",
    [OrderStatus.Preparing]: "Đang chuẩn bị",
    [OrderStatus.Ready]: "Đang giao",
    [OrderStatus.Completed]: "Hoàn thành",
    [OrderStatus.Cancel]: "Huỷ"
}
