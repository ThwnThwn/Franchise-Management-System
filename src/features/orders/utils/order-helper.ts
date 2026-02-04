import { OrderStatus, PaymentMethod } from "@/features/orders/constants"

export const getOrderStatusColor = (status: OrderStatus) => {
    switch (status) {
        case OrderStatus.Paid:
            return "success"
        case OrderStatus.Pending:
            return "warning"
        case OrderStatus.Cancelled:
            return "danger"
        default:
            return "default"
    }
}

export const getOrderStatusLabel = (status: OrderStatus) => {
    switch (status) {
        case OrderStatus.Pending:
            return "Chưa thanh toán"
        case OrderStatus.Paid:
            return "Đã thanh toán"
        case OrderStatus.Cancelled:
            return "Đã hủy"
        default:
            return "Không xác định"
    }
}

export const getPaymentMethodLabel = (method: PaymentMethod | undefined) => {
    if (method === undefined) return "Chưa chọn"
    switch (method) {
        case PaymentMethod.Cash:
            return "Tiền mặt"
        case PaymentMethod.VNPay:
            return "VNPay"
        default:
            return "Khác"
    }
}
