import {
    OrderSaleItemType,
    OrderStatus,
    OrderType,
    PaymentMethod
} from "@/features/orders/constants"
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

export interface OrderSaleItem {
    id: string
    amount: number
    type: OrderSaleItemType
    referenceId?: string
    name: string
}

export interface DeliveryMan {
    id: string
    username: string
    email: string
    phone: string
    firstName: string
    lastName?: string
    avatarUrl?: string
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

    type: OrderType

    deliveryMan?: DeliveryMan
    scheduledTime?: string
    shippingFee: number
    latitude?: number
    longitude?: number
    distance?: number
    address?: string
}

export interface Order extends OrderHeader {
    items: OrderItem[]
    saleItems?: OrderSaleItem[]
}

export interface OrderSummary extends OrderHeader {}

// ── Analytics API response types ──────────────────────────────────────────────

export interface RevenueItem {
    storeId?: string | null
    productName: string
    sku: string
    quantitySold: number
    unitPrice: number
    totalRevenue: number
}

export interface GetRevenueReportReply {
    fromDate: string
    toDate: string
    categoryId?: string | null
    overallTotalRevenue: number
    items: RevenueItem[]
}

export interface OrderVolumeADay {
    date: string // e.g. "2024-05-20"
    totalOrders: number
    canceledOrder: number
    completedOrder: number
    storeId?: string | null
    statusCounts: Record<string, number>
}

export interface GetOrderVolumeReply {
    orders: OrderVolumeADay[]
}
