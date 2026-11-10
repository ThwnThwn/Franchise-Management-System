import { OrderStatus, OrderType, PaymentMethod } from "@/features/orders/constants"

export interface CartItemCreateRequest {
    sku: string
    quantity: number
}

export interface CreateOrderRequest {
    items: CartItemCreateRequest[]
    notes: string
    storeId?: string

    type: OrderType

    /**
     * Chỉ lấy trong ngày nên chỉ nhận "HH:mm", dùng kết hợp dayjs
     */
    scheduledTime?: string
    address?: string // bắt buộc nếu OrderType.Delivery
}

export interface AssignStaffToDeliveryRequest {
    staffId?: string
}

export interface CreatePaymentRequest {
    returnUrl?: string
    paymentMethod: PaymentMethod
    point?: number
    voucherShipCode?: string
    voucherDiscountCode?: string
    appliedWalletAmount?: number
}

export interface UpdateOrderItem {
    sku: string
    quantity: number
}

export interface GetOrdersQuery {
    storeId?: string
    code?: string
    status?: OrderStatus
    paymentMethod?: number
    fromDate?: string
    toDate?: string
}

// ── Analytics request types ───────────────────────────────────────────────────

/** GET /orders/reports/revenue */
export interface GetRevenueReportQuery {
    fromDate?: string // e.g. "2024-01-01T00:00:00Z"
    toDate?: string // e.g. "2024-01-31T23:59:59Z"
    storeId?: string
    categoryId?: string
}

/** GET /orders/reports/volume */
export interface GetOrderVolumeQuery {
    fromDate?: string // e.g. "2024-05-20"
    toDate?: string // e.g. "2024-05-27"
    storeId?: string
    status?: OrderStatus
}
