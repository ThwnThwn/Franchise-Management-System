import { PaymentMethod } from "../constants"

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
    returnUrl?: string
    paymentMethod: PaymentMethod
}

export interface UpdateOrderItem {
    sku: string
    quantity: number
}
