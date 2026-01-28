import { OrderStatus, PaymentMethod } from "./enum"

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
    [PaymentMethod.Cash]: "Tiền mặt",
    [PaymentMethod.VNPay]: "VNPay"
}

export const OrderStatusLabels: Record<OrderStatus, string> = {
    [OrderStatus.Pending]: "Chờ thanh toán",
    [OrderStatus.Paid]: "Đã thanh toán",
    [OrderStatus.Cancelled]: "Đã hủy"
}
