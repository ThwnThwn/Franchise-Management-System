import { OrderStatus, PaymentMethod } from "@/features/orders/constants"

export interface CreateOrderReply {
    orderId: string
    orderCode: string
    totalAmount: number
}

export interface OrderItem {
    id: string
    quantity: number
    unitPrice: number
    productName: string
    sku: string
    imageUrl: string | undefined

    productId: string
    variantId: string
}

export interface OrderHeader {
    id: string
    code: string

    subtotal: number
    tax: number
    totalAmount: number
    paidAmount: number | undefined
    paymentMethod: PaymentMethod | undefined
    status: OrderStatus

    paidAt: string | undefined
    notes: string | undefined

    createdAt: string
    updatedAt: string

    customerId: string | undefined

    items: OrderItem[]
}

export interface Order extends OrderHeader {
    items: OrderItem[]
}

export interface OrderSummary extends OrderHeader {}
