import { UUID } from "crypto"

export enum OrderStatus {
    PENDING = 0,
    CONFIRMED = 1,
    PREPARING = 2,
    READY = 3,
    COMPLETED = 4,
    CANCELLED = 5
}

export enum PaymentMethod {
    CASH = 0,
    CARD = 1,
    E_WALLET = 2,
    BANK_TRANSFER = 3
}

export interface OrderItem {
    id: UUID
    productId: UUID
    variantId: UUID
    quantity: number
    unitPrice: number
    productName: string
    sku: string
    imageUrl?: string
}

export interface Order {
    id: UUID
    code: string
    subtotal: number
    tax?: number
    totalAmount: number
    paidAmount: number
    paymentMethod?: PaymentMethod
    status?: OrderStatus
    paidAt: string
    notes?: string
    createdAt: string
    updatedAt: string
    items: OrderItem[]
}

export interface OrderSummary {
    id: UUID
    code: string
    subtotal: number
    tax?: number
    totalAmount: number
    paidAmount?: number
    paymentMethod?: PaymentMethod
    status?: OrderStatus
    paidAt: string
    notes?: string
    createdAt: string
    updatedAt: string
}

export interface GetMyOrdersResponse {
    orders: OrderSummary[]
    totalCount: number
}
