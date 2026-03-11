export enum VoucherType {
    DISCOUNT_PERCENTAGE = 0,
    DISCOUNT_FIXED_AMOUNT = 1,
    SHIPPING_PERCENTAGE = 2,
    SHIPPING_FIXED_AMOUNT = 3,
    GIFT = 4
}

export enum VoucherItemType {
    ITEM_GIFT = 0,
    ITEM_REQUIRED = 1
}

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
    discount_value?: string
    max_discount_value?: string
    min_order_value?: string
    point_cost?: string
    init_quantity?: number
    current_quantity?: number
    max_usage_per_user?: number
    is_active: boolean
    end_date?: string
    created_at?: string
    updated_at?: string
    voucher_items?: VoucherItem[]
}

export interface CreateVoucherItemReq {
    sku: string
    quantity: number
    type: VoucherItemType
}

export interface CreateVoucherRequest {
    code: string
    type: VoucherType
    discount_value?: string
    max_discount_value?: string
    min_order_value?: string
    point_cost?: string
    init_quantity?: number
    max_usage_per_user?: number
    end_date?: string
    voucher_items?: CreateVoucherItemReq[]
}

export interface UpdateVoucherItemReq {
    id?: string
    sku?: string
    quantity?: number
    type?: VoucherItemType
}

export interface UpdateVoucherRequest {
    discount_value?: string
    max_discount_value?: string
    min_order_value?: string
    point_cost?: string
    max_usage_per_user?: number
    end_date?: string
    voucher_items?: UpdateVoucherItemReq[]
}

export interface UpdateVoucherStatusRequest {
    is_active: boolean
}
