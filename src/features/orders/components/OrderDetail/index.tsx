"use client"

import { useMemo } from "react"
import { Order } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"
import { OrderStatus } from "@/features/orders/constants"
import { RecipeViewRes } from "@/features/products/models/recipes/response"

import { OrderInfoCard } from "./OrderInfoCard"
import { OrderItemsSection } from "./OrderItemsSection"
import { OrderPaymentSummary } from "./OrderPaymentSummary"

import { User } from "@/models/user"
import { WalletResponse } from "@/features/wallets/models/response"
import { OrderItemDisplay } from "@/features/orders/types"

interface Props {
    order: Order
    user?: User
    wallet?: WalletResponse
    recipes?: RecipeViewRes[]
    isCustomer?: boolean

    // Nhận State Ví/Điểm từ Component cha (Page) truyền vào
    appliedWallet: number
    setAppliedWallet: (val: number) => void
    appliedPoint: number
    setAppliedPoint: (val: number) => void

    // NHẬN DATA ĐÃ ĐƯỢC TÍNH TOÁN SẴN TỪ PAGE CHA TRUYỀN XUỐNG
    purchasedItems: OrderItemDisplay[]
    giftItems: OrderItemDisplay[]
    discounts: {
        productDiscount: number
        shippingDiscount: number
        tierDiscount: number
        totalDiscount: number
    }
    calculatedTax: number
    pointValueDeduction: number
    totalAfterTax: number
    finalAmount: number
    isPending: boolean

    appliedShippingVoucher?: Voucher
    appliedDiscountVoucher?: Voucher
    onOpenVoucherSelector?: () => void
}

export const OrderDetail = (props: Props) => {
    const {
        order,
        user,
        isCustomer,
        recipes,
        appliedPoint,
        setAppliedPoint,
        purchasedItems,
        giftItems,
        discounts,
        calculatedTax,
        pointValueDeduction,
        totalAfterTax,
        isPending,
        appliedShippingVoucher,
        appliedDiscountVoucher,
        onOpenVoucherSelector
    } = props

    const currentTier = user?.loyalty?.currentTier

    const defaultKeys = useMemo(() => {
        return order.status === OrderStatus.Preparing ? order.items.map((item) => item.id) : []
    }, [order.status, order.items])

    return (
        <div className="space-y-3">
            <OrderInfoCard order={order} />

            <OrderItemsSection
                purchasedItems={purchasedItems}
                giftItems={giftItems}
                isCustomer={isCustomer}
                recipes={recipes}
                defaultExpandedKeys={defaultKeys}
            />

            <OrderPaymentSummary
                order={order}
                discounts={discounts}
                calculatedTax={calculatedTax}
                pointValueDeduction={pointValueDeduction}
                totalAfterTax={totalAfterTax}
                isPending={isPending}
                isCustomer={isCustomer}
                //
                user={user}
                discountRate={currentTier?.discountRate}
                tierName={currentTier?.name}
                //
                appliedPoint={appliedPoint}
                setAppliedPoint={setAppliedPoint}
                //
                appliedShippingVoucher={appliedShippingVoucher}
                appliedDiscountVoucher={appliedDiscountVoucher}
                onOpenVoucherSelector={onOpenVoucherSelector}
            />
        </div>
    )
}
