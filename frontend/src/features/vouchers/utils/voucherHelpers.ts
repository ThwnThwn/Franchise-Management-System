import { Order } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherItemType, VoucherType } from "@/features/vouchers/constants"
import { formatCurrencyWithSymbol, normalizeCurrency } from "@/utils/helpers/currency"

export const checkVoucherEligibility = (
    voucher: Voucher,
    order?: Order
): { isEligible: boolean; reason?: string } => {
    if (!order) return { isEligible: false, reason: "Không có thông tin đơn hàng" }

    const orderTotal = order.totalAmount || 0

    // 1. Kiểm tra giá trị đơn tối thiểu
    if (voucher.minOrderValue && orderTotal < normalizeCurrency(voucher.minOrderValue)) {
        return {
            isEligible: false,
            reason: `Đơn tối thiểu ${formatCurrencyWithSymbol(voucher.minOrderValue)}`
        }
    }

    // 2. Kiểm tra SP yêu cầu đối với quà tặng
    if (
        voucher.type === VoucherType.GIFT &&
        voucher.voucherItems &&
        voucher.voucherItems.length > 0
    ) {
        const requiredItems = voucher.voucherItems.filter(
            (i) => i.type === VoucherItemType.ITEM_REQUIRED
        )

        for (const requiredItem of requiredItems) {
            const orderItem = order.items.find((i) => i.sku === requiredItem.sku)
            if (!orderItem || orderItem.quantity < requiredItem.quantity) {
                return {
                    isEligible: false,
                    reason: `Cần mua tối thiểu ${requiredItem.quantity} sản phẩm quy định`
                }
            }
        }
    }

    return { isEligible: true }
}
