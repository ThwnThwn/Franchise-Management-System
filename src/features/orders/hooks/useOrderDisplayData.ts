import { useMemo } from "react"
import { Order } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherItemType } from "@/features/vouchers"
import { useGetProductVariants } from "@/features/products"
import { splitOrderItems, calculateTemporaryDiscount } from "@/features/orders/utils"
import { OrderStatus } from "@/features/orders/constants"
import { ProductVariantDetail } from "@/features/products/models/products/respone"
import { OrderItemDisplay } from "@/features/orders/types"

// 1. ĐỊNH NGHĨA KIỂU DỮ LIỆU ĐẦU VÀO
export interface UseOrderDisplayDataArgs {
    order?: Order
    appliedDiscountVoucher?: Voucher
    appliedShippingVoucher?: Voucher
    discountRate?: number
    requestedPoints?: number
    requestedWallet?: number
    availablePointBalance?: number
    availableWalletBalance?: number
    valuePerPoint?: number
}

// 2. ĐỊNH NGHĨA KIỂU DỮ LIỆU TRẢ VỀ
export interface UseOrderDisplayDataReturn {
    purchasedItems: OrderItemDisplay[]
    giftItems: OrderItemDisplay[]
    discounts: {
        productDiscount: number
        shippingDiscount: number
        tierDiscount: number
        totalDiscount: number
    }
    calculatedTax: number
    safePoints: number
    safeWallet: number
    pointValueDeduction: number
    totalAfterTax: number
    finalAmount: number
    isPending: boolean
}

// 3. HOOK CHÍNH
export const useOrderDisplayData = ({
    order,
    appliedDiscountVoucher,
    appliedShippingVoucher,
    discountRate = 0,
    requestedPoints = 0,
    requestedWallet = 0,
    availablePointBalance = 0,
    availableWalletBalance = 0,
    valuePerPoint = 1000
}: UseOrderDisplayDataArgs): UseOrderDisplayDataReturn => {
    const giftSkus = useMemo(() => {
        return (
            appliedDiscountVoucher?.voucherItems
                ?.filter((i) => i.type === VoucherItemType.ITEM_GIFT)
                ?.map((i) => i.sku) || []
        )
    }, [appliedDiscountVoucher])

    const { data: variants } = useGetProductVariants(giftSkus)

    const variantMap = useMemo(() => {
        const map: Record<string, ProductVariantDetail> = {}
        variants?.forEach((v) => {
            map[v.sku.toUpperCase()] = v
        })
        return map
    }, [variants])

    const { purchasedItems, giftItems } = useMemo(() => {
        return splitOrderItems(order, appliedDiscountVoucher, variantMap)
    }, [order, appliedDiscountVoucher, variantMap])

    const isPending = order?.status === OrderStatus.PaymentPending

    const pricing = useMemo(() => {
        const subtotal = order?.subtotal || 0

        // Giảm Hạng & Voucher
        const tierDiscount = subtotal * discountRate

        const voucherDiscounts = isPending
            ? calculateTemporaryDiscount(subtotal, appliedShippingVoucher, appliedDiscountVoucher)
            : { productDiscount: 0, shippingDiscount: 0, totalDiscount: 0 }

        // Tính Trần cho Điểm (Tiền còn lại trước Thuế)
        const amountToClearPreTax = Math.max(
            subtotal -
                tierDiscount -
                voucherDiscounts.productDiscount -
                voucherDiscounts.shippingDiscount,
            0
        )

        // Xử lý ĐIỂM (Auto Adjust)
        const maxPointsByMoney = Math.floor(amountToClearPreTax / valuePerPoint)
        const safePoints = isPending
            ? Math.min(requestedPoints, availablePointBalance, maxPointsByMoney)
            : 0
        const pointValueDeduction = safePoints * valuePerPoint

        // Tính số tiền chịu thuế
        const taxableAmount = Math.max(amountToClearPreTax - pointValueDeduction, 0)

        // Tính TỔNG TIỀN TRƯỚC (Làm tròn chốt hạ)
        const totalAfterTax = isPending
            ? Math.round(taxableAmount * 1.1) // 100% gốc + 10% thuế
            : order?.totalAmount || 0

        // TÍNH NGƯỢC LẠI THUẾ ĐỂ HIỂN THỊ UI (Để đảm bảo Tax + Taxable = Total)
        const calculatedTax = isPending ? totalAfterTax - taxableAmount : order?.tax || 0

        // Xử lý VÍ (Trần là tiền sau Thuế đã làm tròn)
        const safeWallet = isPending
            ? Math.min(requestedWallet, availableWalletBalance, totalAfterTax)
            : 0

        // Tiền mặt cuối cùng
        const finalAmount = isPending
            ? Math.max(totalAfterTax - safeWallet, 0)
            : order?.totalAmount || 0

        return {
            tierDiscount,
            calculatedTax,
            productDiscount: voucherDiscounts.productDiscount,
            shippingDiscount: voucherDiscounts.shippingDiscount,
            totalDiscount: tierDiscount + voucherDiscounts.totalDiscount,
            safePoints,
            safeWallet,
            pointValueDeduction,
            totalAfterTax,
            finalAmount
        }
    }, [
        isPending,
        order?.subtotal,
        order?.tax,
        order?.totalAmount,
        discountRate,
        appliedShippingVoucher,
        appliedDiscountVoucher,
        requestedPoints,
        requestedWallet,
        availablePointBalance,
        availableWalletBalance,
        valuePerPoint
    ])

    return {
        purchasedItems,
        giftItems,
        discounts: {
            productDiscount: pricing.productDiscount,
            shippingDiscount: pricing.shippingDiscount,
            tierDiscount: pricing.tierDiscount,
            totalDiscount: pricing.totalDiscount
        },
        calculatedTax: pricing.calculatedTax,
        safePoints: pricing.safePoints,
        safeWallet: pricing.safeWallet,
        pointValueDeduction: pricing.pointValueDeduction,
        totalAfterTax: pricing.totalAfterTax,
        finalAmount: pricing.finalAmount,
        isPending
    }
}
