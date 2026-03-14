import { VoucherItemType, VoucherType } from "@/features/vouchers/constants"

export interface CreateVoucherItemReq {
    sku: string
    quantity: number
    type: VoucherItemType
}

export interface CreateVoucherRequest {
    code: string
    type: VoucherType
    discountValue?: string
    maxDiscountValue?: string
    minOrderValue?: string
    pointCost?: string
    initQuantity?: number
    maxUsagePerUser?: number
    endDate?: string
    voucherItems?: CreateVoucherItemReq[]
}

export interface UpdateVoucherItemReq {
    id?: string
    sku?: string
    quantity?: number
    type?: VoucherItemType
}

export interface UpdateVoucherRequest {
    discountValue?: string
    maxDiscountValue?: string
    minOrderValue?: string
    pointCost?: string
    maxUsagePerUser?: number
    endDate?: string
    voucherItems?: UpdateVoucherItemReq[]
}

export interface UpdateVoucherStatusRequest {
    isActive: boolean
}
