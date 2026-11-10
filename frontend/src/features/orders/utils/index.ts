import { ORDER_STATUS_TRANSITION_POLICY, OrderStatus } from "@/features/orders/constants"
import { Order } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherItemType, VoucherType } from "@/features/vouchers"
import { OrderSaleItemType } from "@/features/orders/constants"
import { normalizeCurrency } from "@/utils/helpers/currency"
import { OrderItemDisplay } from "@/features/orders/types"
import { ProductVariantDetail } from "@/features/products/models/products/respone"

/**
 * Kiểm tra xem đơn hàng có nằm trong các trạng thái mà User (Staff/Admin)
 * CẦN phải thao tác thủ công để chuyển tiếp hay không.
 * * - PaymentPending (0): Chờ hệ thống tự động (Auto)
 * - Completed (6), Cancelled (7): Đã kết thúc (Terminal)
 */
export const isManualActionRequired = (status: OrderStatus): boolean => {
    // Chỉ những trạng thái từ 1 đến 5 mới cho phép Staff bấm nút chuyển tiếp
    return status >= OrderStatus.ConfirmationPending && status <= OrderStatus.Delivering
}

/**
 * Kiểm tra xem đơn hàng có đang ở trạng thái chờ Hệ thống tự động xử lý hay không.
 */
export const isAutoTransitionStatus = (status: OrderStatus): boolean => {
    return status === OrderStatus.PaymentPending
}

/**
 * Kiểm tra xem từ trạng thái hiện tại có thể thực hiện hành động "Hủy" (Cancel) hay không
 * dựa trên chính sách chuyển đổi trạng thái (Transition Policy).
 */
export const canCancelFrom = (status: OrderStatus): boolean => {
    const nextAllowedStatuses = ORDER_STATUS_TRANSITION_POLICY[status] || []

    // Check xem trong danh sách status tiếp theo có thằng Cancelled không
    return nextAllowedStatuses.includes(OrderStatus.Cancelled)
}

// 1. Tách mảng Sản phẩm Mua và Sản phẩm Tặng
export const splitOrderItems = (
    order?: Order,
    pendingVoucher?: Voucher,
    variantMap: Record<string, ProductVariantDetail> = {}
): { purchasedItems: OrderItemDisplay[]; giftItems: OrderItemDisplay[] } => {
    if (!order || !order.items) {
        return { purchasedItems: [], giftItems: [] }
    }

    const purchasedItems: OrderItemDisplay[] = []
    const giftItems: OrderItemDisplay[] = []

    const pendingGifts =
        pendingVoucher?.type === VoucherType.GIFT
            ? pendingVoucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_GIFT) || []
            : []

    // 1. Duyệt giỏ hàng hiện tại (Hàng mua + Quà đã có trong giỏ)
    order.items.forEach((item) => {
        if (!item) return

        const itemSku = item.sku.toUpperCase().trim()

        const giftQtyFromBE =
            order.saleItems?.filter(
                (sale) =>
                    sale.type === OrderSaleItemType.VOUCHER &&
                    sale.name.toUpperCase().includes(itemSku)
            ).length || 0

        const giftQtyFromPending = pendingGifts
            .filter((pg) => pg.sku.toUpperCase().trim() === itemSku)
            .reduce((sum, current) => sum + (current.quantity || 0), 0)

        const totalGiftQty = giftQtyFromBE + giftQtyFromPending
        const buyQty = Math.max(0, item.quantity - totalGiftQty)

        if (buyQty > 0) {
            purchasedItems.push({ ...item, displayQuantity: buyQty })
        }
        if (totalGiftQty > 0) {
            giftItems.push({ ...item, displayQuantity: totalGiftQty })
        }
    })

    // 2. XỬ LÝ QUÀ TẶNG (Chưa có trong giỏ hàng)
    pendingGifts.forEach((gift) => {
        const isInCart = order.items.some((i) => i.sku.toUpperCase() === gift.sku.toUpperCase())

        if (!isInCart) {
            const info = variantMap[gift.sku.toUpperCase()]

            giftItems.push({
                id: `pending-gift-${gift.sku}`,
                productName: info ? info.productName : `[Quà tặng] ${gift.sku}`,
                sku: gift.sku,
                unitPrice: info?.basePrice ?? 0,
                quantity: gift.quantity,
                displayQuantity: gift.quantity,
                imageUrl: info?.imageUrl,
                productId: info?.productId || "",
                variantId: info?.variantId || ""
            })
        }
    })

    return { purchasedItems, giftItems }
}

// 2. Tính toán Giảm giá tạm thời (Dành cho trạng thái Pending chưa gửi BE)
export const calculateTemporaryDiscount = (
    orderTotal: number,
    appliedShippingVoucher?: Voucher,
    appliedDiscountVoucher?: Voucher
) => {
    let productDiscount = 0
    let shippingDiscount = 0

    // Tính giảm giá sản phẩm
    if (appliedDiscountVoucher) {
        if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_FIXED_AMOUNT) {
            productDiscount = normalizeCurrency(appliedDiscountVoucher.discountValue || 0)
        } else if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_PERCENTAGE) {
            // Giữ nguyên số lẻ để Hook tính toán chính xác
            let calculated =
                orderTotal * (normalizeCurrency(appliedDiscountVoucher.discountValue || 0) / 100)

            if (appliedDiscountVoucher.maxDiscountValue) {
                calculated = Math.min(
                    calculated,
                    normalizeCurrency(appliedDiscountVoucher.maxDiscountValue)
                )
            }
            productDiscount = calculated
        }
    }

    // Tính giảm giá vận chuyển (Tương tự sản phẩm)
    if (appliedShippingVoucher) {
        if (appliedShippingVoucher.type === VoucherType.SHIPPING_FIXED_AMOUNT) {
            shippingDiscount = normalizeCurrency(appliedShippingVoucher.discountValue || 0)
        } else if (appliedShippingVoucher.type === VoucherType.SHIPPING_PERCENTAGE) {
            let calculated =
                orderTotal * (normalizeCurrency(appliedShippingVoucher.discountValue || 0) / 100)

            if (appliedShippingVoucher.maxDiscountValue) {
                calculated = Math.min(
                    calculated,
                    normalizeCurrency(appliedShippingVoucher.maxDiscountValue)
                )
            }
            shippingDiscount = calculated
        }
    }

    return {
        productDiscount,
        shippingDiscount,
        totalDiscount: productDiscount + shippingDiscount
    }
}
