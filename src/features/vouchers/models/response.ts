import { VoucherItemType, VoucherType } from "@/features/vouchers/constants"

export interface VoucherItem {
    id: string
    sku: string
    quantity: number
    type: VoucherItemType
}

export interface Voucher {
    id: string
    code: string
    type: VoucherType
    discountValue?: string
    maxDiscountValue?: string
    minOrderValue?: string
    pointCost?: string
    initQuantity?: number
    currentQuantity?: number
    maxUsagePerUser?: number
    isActive: boolean
    endDate?: string
    createdAt?: string
    updatedAt?: string
    voucherItems?: VoucherItem[]
}
