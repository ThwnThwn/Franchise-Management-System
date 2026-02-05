import { OrderStatus, PaymentMethod } from "@/features/orders/constants"

export const getOrderStatusColor = (status: OrderStatus) => {
    switch (status) {
        case OrderStatus.PaymentPending:
            return "warning"
        case OrderStatus.Preparing:
            return "secondary"
        case OrderStatus.Ready:
            return "success"
        case OrderStatus.Completed:
            return "primary"
        case OrderStatus.Cancel:
            return "danger"
        default:
            return "default"
    }
}

export const getOrderStatusLabel = (status: OrderStatus) => {
    switch (status) {
        case OrderStatus.PaymentPending:
            return "Chưa thanh toán"
        case OrderStatus.Preparing:
            return "Đang chuẩn bị"
        case OrderStatus.Ready:
            return "Đang giao"
        case OrderStatus.Completed:
            return "Hoàn thành"
        case OrderStatus.Cancel:
            return "Huỷ"
        default:
            return "default"
    }
}

export const getPaymentMethodLabel = (method: PaymentMethod | undefined) => {
    if (method === undefined) return "Chưa chọn"
    switch (method) {
        case PaymentMethod.Cash:
            return "Tiền mặt"
        case PaymentMethod.VNPay:
            return "VNPay"
        case PaymentMethod.Unknown:
        default:
            return "Chưa chọn"
    }
}
