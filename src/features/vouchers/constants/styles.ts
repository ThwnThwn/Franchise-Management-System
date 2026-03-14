import { ChipProps } from "@heroui/react"
import { VoucherItemType, VoucherType } from "./enum"

export const VoucherTypeColors: Record<VoucherType, ChipProps["color"]> = {
    [VoucherType.DISCOUNT_PERCENTAGE]: "warning", // Màu Amber/Cam
    [VoucherType.DISCOUNT_FIXED_AMOUNT]: "danger", // Màu Đỏ đậm (Espresso Roast)
    [VoucherType.SHIPPING_PERCENTAGE]: "primary", // Màu Xanh (Blueberry)
    [VoucherType.SHIPPING_FIXED_AMOUNT]: "secondary", // Màu Tím (Lavender)
    [VoucherType.GIFT]: "success" // Màu Xanh lá (Matcha)
}

export const VoucherTypeStyles: Record<VoucherType, string> = {
    [VoucherType.DISCOUNT_PERCENTAGE]: "bg-[#fef3c7] text-[#b45309] border-[#fde68a]", // Caramel Latte
    [VoucherType.DISCOUNT_FIXED_AMOUNT]: "bg-[#fee2e2] text-[#991b1b] border-[#fecaca]", // Red Velvet
    [VoucherType.SHIPPING_PERCENTAGE]: "bg-[#e0f2fe] text-[#075985] border-[#bae6fd]", // Blue Sky
    [VoucherType.SHIPPING_FIXED_AMOUNT]: "bg-[#f5f3ff] text-[#5b21b6] border-[#ddd6fe]", // Taro Tea
    [VoucherType.GIFT]: "bg-[#f0fdf4] text-[#166534] border-[#dcfce7]" // Matcha Green
}

export const VoucherItemTypeColors: Record<VoucherItemType, ChipProps["color"]> = {
    [VoucherItemType.ITEM_GIFT]: "success",
    [VoucherItemType.ITEM_REQUIRED]: "default"
}

export const VoucherItemTypeStyles: Record<VoucherItemType, string> = {
    [VoucherItemType.ITEM_GIFT]: "bg-success-50 text-success-700 font-bold border-success-200",
    [VoucherItemType.ITEM_REQUIRED]: "bg-default-100 text-default-600 italic border-default-200"
}
