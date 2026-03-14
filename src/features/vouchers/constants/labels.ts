import { VoucherItemType, VoucherType } from "./enum"

export const VoucherTypeLabels: Record<VoucherType, string> = {
    [VoucherType.DISCOUNT_PERCENTAGE]: "Giảm giá %",
    [VoucherType.DISCOUNT_FIXED_AMOUNT]: "Giảm tiền mặt",
    [VoucherType.SHIPPING_PERCENTAGE]: "Giảm phí ship %",
    [VoucherType.SHIPPING_FIXED_AMOUNT]: "Phí ship cố định",
    [VoucherType.GIFT]: "Quà tặng kèm"
}

export const VoucherItemTypeLabels: Record<VoucherItemType, string> = {
    [VoucherItemType.ITEM_GIFT]: "Quà tặng",
    [VoucherItemType.ITEM_REQUIRED]: "Sản phẩm yêu cầu"
}
