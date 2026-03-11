import { OrderStatus, PaymentMethod } from "@/features/orders/constants"

export interface CartItemCreateRequest {
    sku: string
    quantity: number
}

export interface CreateOrderRequest {
    items: CartItemCreateRequest[]
    // tax: number
    // paymentMethod: PaymentMethod
    notes: string
    storeId?: string
}

export interface CreatePaymentRequest {
    returnUrl?: string | null
    paymentMethod: PaymentMethod
    point?: number | null
    voucherShipCode?: string | null
    voucherDiscountCode?: string | null
    appliedWalletAmount?: number | null
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
    fromDate?: string   // e.g. "2024-01-01T00:00:00Z"
    toDate?: string     // e.g. "2024-01-31T23:59:59Z"
    storeId?: string
    categoryId?: string
}

/** GET /orders/reports/volume */
export interface GetOrderVolumeQuery {
    fromDate?: string   // e.g. "2024-05-20"
    toDate?: string     // e.g. "2024-05-27"
    storeId?: string
    status?: OrderStatus
}

