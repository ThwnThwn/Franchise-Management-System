import { OrderStatus, PaymentMethod } from "@/features/orders/constants"
import { StoreDetailDto } from "@/features/stores"

export interface ShortageItem {
    variantId: string
    sku: string
    maxFeasibleQuantity: number
}

export interface CheckEnoughStockResponse {
    isEnough: boolean
    shortageItems: ShortageItem[]
}

export interface OrderReply {
    orderId: string
    orderCode: string
    totalAmount: number
    customerId: string
    storeId: string
    checkStockResponse?: CheckEnoughStockResponse
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
    paidAmount?: number
    paymentMethod?: PaymentMethod
    status: OrderStatus

    paidAt?: string
    notes?: string

    createdAt: string
    updatedAt: string

    customerId?: string
    store?: StoreDetailDto

    items: OrderItem[]
}

export interface Order extends OrderHeader {
    items: OrderItem[]
}

export interface OrderSummary extends OrderHeader {}
