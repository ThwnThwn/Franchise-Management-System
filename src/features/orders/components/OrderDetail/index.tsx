"use client"

import React, { useMemo } from "react"
import { Accordion, AccordionItem, Divider } from "@heroui/react"
import { Order, OrderSaleItem, OrderSaleItemType } from "@/features/orders/models/response"
import {
    formatCurrency,
    formatCurrencyWithSymbol,
    normalizeCurrency
} from "@/utils/helpers/currency"
import { ORDER_TEXT, OrderStatus, PaymentMethodLabels } from "@/features/orders/constants"
import { RecipeViewRes } from "@/features/products/models/recipes/response"
import { RecipeIngredientList } from "@/features/products"
import { OrderItemRow } from "./OrderItemRow"
import { Voucher, VoucherType } from "../../models/voucher"
import { Button } from "@heroui/react"
import { Ticket } from "lucide-react"

export const OrderDetail = ({
    order,
    recipes = [],
    isCustomer,
    appliedShippingVoucher,
    appliedDiscountVoucher,
    onOpenVoucherSelector
}: {
    order: Order
    recipes?: RecipeViewRes[]
    isCustomer?: boolean
    appliedShippingVoucher?: Voucher | null
    appliedDiscountVoucher?: Voucher | null
    onOpenVoucherSelector?: () => void
}) => {
    const defaultKeys = useMemo(() => {
        if (order.status === OrderStatus.Preparing) {
            return order.items.map((item) => item.id)
        }
        return []
    }, [order])

    const voucherMap = new Map<string, OrderSaleItem>(
        (order.saleItems ?? []).map((item) => [item.name, item])
    )

    // Calculate Discounts
    const calculateDiscount = () => {
        let discount = 0
        let shippingDiscount = 0

        const orderTotal = order.totalAmount || 0

        if (appliedDiscountVoucher) {
            if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_FIXED_AMOUNT) {
                discount += normalizeCurrency(appliedDiscountVoucher.discount_value || 0)
            } else if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_PERCENTAGE) {
                let calculated =
                    orderTotal *
                    (normalizeCurrency(appliedDiscountVoucher.discount_value || 0) / 100)
                if (appliedDiscountVoucher.max_discount_value) {
                    calculated = Math.min(
                        calculated,
                        normalizeCurrency(appliedDiscountVoucher.max_discount_value)
                    )
                }
                discount += calculated
            }
        }

        if (appliedShippingVoucher) {
            if (appliedShippingVoucher.type === VoucherType.SHIPPING_FIXED_AMOUNT) {
                shippingDiscount += normalizeCurrency(appliedShippingVoucher.discount_value || 0)
            } else if (appliedShippingVoucher.type === VoucherType.SHIPPING_PERCENTAGE) {
                let calculated =
                    orderTotal *
                    (normalizeCurrency(appliedShippingVoucher.discount_value || 0) / 100)
                if (appliedShippingVoucher.max_discount_value) {
                    calculated = Math.min(
                        calculated,
                        normalizeCurrency(appliedShippingVoucher.max_discount_value)
                    )
                }
                shippingDiscount += calculated
            }
        }

        return {
            productDiscount: discount,
            shippingDiscount: shippingDiscount,
            totalDiscount: discount + shippingDiscount
        }
    }

    const { totalDiscount, productDiscount, shippingDiscount } = calculateDiscount()
    const finalAmount = Math.max((order.totalAmount || 0) - totalDiscount, 0)

    return (
        <div className="space-y-8">
            {/* 1. Thông tin khách hàng & Thanh toán */}
            <div className="flex flex-wrap justify-between gap-6 p-6 rounded-2xl border border-default-100 bg-default-50/30">
                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                        {ORDER_TEXT.labels.CUSTOMER}
                    </p>
                    <p className="font-bold text-slate-800">
                        {order.customerId ? ORDER_TEXT.labels.MEMBER : ORDER_TEXT.labels.GUEST}
                    </p>
                </div>
                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                        Phương thức thanh toán
                    </p>
                    <p className="font-bold text-slate-800">
                        {order.paymentMethod !== undefined
                            ? PaymentMethodLabels[order.paymentMethod]
                            : "-"}
                    </p>
                </div>
                {order.notes && (
                    <div className="space-y-1 min-w-50">
                        <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                            {ORDER_TEXT.labels.NOTES}
                        </p>
                        <p className="italic text-xs text-default-600 line-clamp-2">
                            {order.notes}
                        </p>
                    </div>
                )}
            </div>

            {/* 2. Danh sách sản phẩm */}
            <div className="space-y-4">
                <div className="grid grid-cols-12 px-2 pb-2 mb-0">
                    <div className="col-span-6 text-[10px] uppercase font-black text-default-400">
                        Sản phẩm
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-center">
                        Đơn giá
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-center">
                        Số lượng
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-right">
                        Thành tiền
                    </div>
                </div>

                <Divider className="mb-0" />

                <div className="hidden print:flex flex-col">
                    {/* Danh sách SP khách mua */}
                    {order.items.map((item) => {
                        const giftQty =
                            order.saleItems?.filter(
                                (s) =>
                                    s.type === OrderSaleItemType.VOUCHER &&
                                    s.amount <= 1 &&
                                    (s.name
                                        .toUpperCase()
                                        .includes(item.productName.toUpperCase()) ||
                                        s.name.toUpperCase().includes(item.sku.toUpperCase()))
                            ).length || 0
                        const buyQty = Math.max(0, item.quantity - giftQty)

                        if (buyQty <= 0) return null
                        return (
                            <div
                                key={`buy-${item.id}`}
                                className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                            >
                                <OrderItemRow
                                    item={item}
                                    sale={voucherMap.get(item.sku)}
                                    overrideQuantity={buyQty}
                                />
                            </div>
                        )
                    })}
                    {/* Danh sách SP tặng */}
                    {order.items.map((item) => {
                        const giftQty =
                            order.saleItems?.filter(
                                (s) =>
                                    s.type === OrderSaleItemType.VOUCHER &&
                                    s.amount <= 1 &&
                                    (s.name
                                        .toUpperCase()
                                        .includes(item.productName.toUpperCase()) ||
                                        s.name.toUpperCase().includes(item.sku.toUpperCase()))
                            ).length || 0

                        if (giftQty <= 0) return null
                        return (
                            <div
                                key={`gift-${item.id}`}
                                className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                            >
                                <OrderItemRow
                                    item={item}
                                    sale={voucherMap.get(item.sku)}
                                    isGift={true}
                                    overrideQuantity={giftQty}
                                />
                            </div>
                        )
                    })}
                </div>

                {isCustomer ? (
                    <div className="print-hidden flex flex-col">
                        {/* Danh sách SP khách mua */}
                        {order.items.map((item) => {
                            const giftQty =
                                order.saleItems?.filter(
                                    (s) =>
                                        s.type === OrderSaleItemType.VOUCHER &&
                                        s.amount <= 0 &&
                                        (s.name.includes(item.productName) ||
                                            s.name.includes(item.sku))
                                ).length || 0
                            const buyQty = Math.max(0, item.quantity - giftQty)

                            if (buyQty <= 0) return null
                            return (
                                <div
                                    key={`buy-${item.id}`}
                                    className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                                >
                                    <OrderItemRow
                                        item={item}
                                        sale={voucherMap.get(item.sku)}
                                        overrideQuantity={buyQty}
                                    />
                                </div>
                            )
                        })}
                        {/* Danh sách SP tặng */}
                        {order.items.map((item) => {
                            const giftQty =
                                order.saleItems?.filter(
                                    (s) =>
                                        s.type === OrderSaleItemType.VOUCHER &&
                                        s.amount <= 0 &&
                                        (s.name.includes(item.productName) ||
                                            s.name.includes(item.sku))
                                ).length || 0

                            if (giftQty <= 0) return null
                            return (
                                <div
                                    key={`gift-${item.id}`}
                                    className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                                >
                                    <OrderItemRow
                                        item={item}
                                        sale={voucherMap.get(item.sku)}
                                        isGift={true}
                                        overrideQuantity={giftQty}
                                    />
                                </div>
                            )
                        })}
                    </div>
                ) : (
                    <Accordion
                        selectionMode="multiple"
                        variant="light"
                        className="px-0 print-hidden"
                        defaultExpandedKeys={defaultKeys}
                        hideIndicator
                        itemClasses={{
                            base: "border-b border-default-100 last:border-none",
                            title: "p-0",
                            trigger: "px-2 py-4 rounded-xl hover:bg-default-50 transition-colors",
                            content: "px-4 pt-0"
                        }}
                    >
                        {
                            order.items.flatMap((item) => {
                                const giftQty =
                                    order.saleItems?.filter(
                                        (s) =>
                                            s.type === OrderSaleItemType.VOUCHER &&
                                            s.amount <= 1 &&
                                            (s.name
                                                .toUpperCase()
                                                .includes(item.productName.toUpperCase()) ||
                                                s.name
                                                    .toUpperCase()
                                                    .includes(item.sku.toUpperCase()))
                                    ).length || 0
                                const buyQty = Math.max(0, item.quantity - giftQty)

                                const items = []
                                if (buyQty > 0) {
                                    items.push(
                                        <AccordionItem
                                            key={`buy-${item.id}`}
                                            aria-label={item.productName}
                                            title={
                                                <OrderItemRow
                                                    item={item}
                                                    sale={voucherMap.get(item.sku)}
                                                    overrideQuantity={buyQty}
                                                />
                                            }
                                        >
                                            <RecipeIngredientList
                                                variantId={item.variantId}
                                                recipes={recipes}
                                            />
                                        </AccordionItem>
                                    )
                                }
                                if (giftQty > 0) {
                                    items.push(
                                        <AccordionItem
                                            key={`gift-${item.id}`}
                                            aria-label={`${item.productName} (Quà tặng)`}
                                            title={
                                                <OrderItemRow
                                                    item={item}
                                                    sale={voucherMap.get(item.sku)}
                                                    isGift={true}
                                                    overrideQuantity={giftQty}
                                                />
                                            }
                                        >
                                            <RecipeIngredientList
                                                variantId={item.variantId}
                                                recipes={recipes}
                                            />
                                        </AccordionItem>
                                    )
                                }
                                return items
                            }) as any
                        }
                    </Accordion>
                )}
            </div>

            {/* 3. Phần tổng kết (Summary) */}
            <div className="min-w-full pt-4">
                <div className="w-full space-y-3 bg-default-50/50 p-6 rounded-3xl border border-default-100">
                    <div className="flex justify-between text-xs font-medium">
                        <span className="text-default-400 uppercase tracking-widest">
                            {ORDER_TEXT.labels.SUBTOTAL}
                        </span>
                        <span className="text-slate-700">
                            {formatCurrencyWithSymbol(order.subtotal)}
                        </span>
                    </div>
                    <div className="flex justify-between text-xs font-medium">
                        <span className="text-default-400 uppercase tracking-widest">
                            {ORDER_TEXT.labels.TAX} (10%)
                        </span>
                        <span className="text-slate-700">
                            {formatCurrencyWithSymbol(order.tax)}
                        </span>
                    </div>

                    {/* Voucher Section for PaymentPending */}
                    {isCustomer &&
                        order.status === OrderStatus.PaymentPending &&
                        onOpenVoucherSelector && (
                            <div className="pt-2">
                                <Button
                                    className="w-full bg-primary/10 text-primary border border-primary/20 font-bold"
                                    onPress={onOpenVoucherSelector}
                                    startContent={<Ticket size={18} />}
                                >
                                    {appliedShippingVoucher || appliedDiscountVoucher
                                        ? "Đã chọn tối đa voucher"
                                        : "Chọn hoặc nhập mã Khuyến mãi"}
                                </Button>

                                {/* Discount lines currently applying */}
                                {(appliedShippingVoucher || appliedDiscountVoucher) && (
                                    <div className="mt-3 space-y-2">
                                        {appliedDiscountVoucher && productDiscount > 0 && (
                                            <div className="flex justify-between text-xs font-medium text-success">
                                                <span>
                                                    Giảm giá ({appliedDiscountVoucher.code})
                                                </span>
                                                <span>
                                                    -{formatCurrencyWithSymbol(productDiscount)}
                                                </span>
                                            </div>
                                        )}
                                        {appliedShippingVoucher && shippingDiscount > 0 && (
                                            <div className="flex justify-between text-xs font-medium text-success">
                                                <span>
                                                    Hỗ trợ phí ship ({appliedShippingVoucher.code})
                                                </span>
                                                <span>
                                                    -{formatCurrencyWithSymbol(shippingDiscount)}
                                                </span>
                                            </div>
                                        )}
                                        {appliedDiscountVoucher?.type === VoucherType.GIFT && (
                                            <div className="flex justify-between items-center text-xs font-medium text-success p-2 bg-success/10 rounded-lg">
                                                <span>
                                                    Quà tặng ({appliedDiscountVoucher.code})
                                                </span>
                                                <span className="px-2 py-0.5 bg-success text-white rounded-full text-[10px]">
                                                    Đã áp dụng quà tặng
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                )}
                            </div>
                        )}

                    {/* Applied Discounts from Backend (For completed orders) */}
                    {order.saleItems &&
                        order.saleItems.length > 0 &&
                        order.status !== OrderStatus.PaymentPending && (
                            <div className="pt-2">
                                <div className="mt-3 space-y-2">
                                    {order.saleItems.map((saleItem, idx) => (
                                        <div
                                            key={saleItem.id || idx}
                                            className="flex justify-between items-center text-xs font-medium text-success"
                                        >
                                            <div className="flex flex-col">
                                                <span>
                                                    {saleItem.type === OrderSaleItemType.VOUCHER
                                                        ? "Voucher"
                                                        : saleItem.type ===
                                                            OrderSaleItemType.PROMOTION
                                                          ? "Khuyến mãi"
                                                          : saleItem.type ===
                                                              OrderSaleItemType.POINT
                                                            ? "Dùng điểm"
                                                            : saleItem.type ===
                                                                OrderSaleItemType.WALLET
                                                              ? "Ví CFMS"
                                                              : "Giảm giá"}
                                                    {saleItem.name ? ` (${saleItem.name})` : ""}
                                                </span>
                                            </div>
                                            {saleItem.amount > 0 ? (
                                                <span>-{formatCurrency(saleItem.amount)}</span>
                                            ) : (
                                                <span className="px-2 py-0.5 bg-success text-white rounded-full text-[10px]">
                                                    Quà tặng
                                                </span>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                    <div className="pt-2">
                        <div className="border-t border-dashed border-default-200 my-2" />
                        <div className="flex justify-between items-center">
                            <span className="text-sm font-black uppercase tracking-tighter text-primary">
                                Tổng cộng
                            </span>
                            <span className="text-xl font-black text-primary flex flex-col items-end">
                                {totalDiscount > 0 && (
                                    <span className="text-xs text-default-400 line-through font-normal">
                                        {formatCurrencyWithSymbol(order.totalAmount)}
                                    </span>
                                )}
                                {formatCurrencyWithSymbol(finalAmount)}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}
