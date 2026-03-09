"use client"

import React, { useMemo, useState } from "react"
import { Accordion, AccordionItem, Divider } from "@heroui/react"
import { Order } from "@/features/orders/models/response"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { ORDER_TEXT, OrderStatus, PaymentMethodLabels } from "@/features/orders/constants"
import { RecipeViewRes } from "@/features/products/models/recipes/response"
import { RecipeIngredientList } from "@/features/products"
import { OrderItemRow } from "./OrderItemRow"
import { VoucherSelector } from "../VoucherSelector"
import { Voucher, VoucherType } from "../../models/voucher"
import { Button } from "@heroui/react"
import { Ticket } from "lucide-react"
export const OrderDetail = ({
    order,
    recipes = [],
    isCustomer
}: {
    order: Order
    recipes?: RecipeViewRes[]
    isCustomer?: boolean
}) => {
    const defaultKeys = useMemo(() => {
        if (order.status === OrderStatus.Preparing) {
            return order.items.map((item) => item.id)
        }
        return []
    }, [order])

    // Voucher state 
    const [isVoucherModalOpen, setIsVoucherModalOpen] = useState(false)
    const [appliedShippingVoucher, setAppliedShippingVoucher] = useState<Voucher | null>(null)
    const [appliedDiscountVoucher, setAppliedDiscountVoucher] = useState<Voucher | null>(null)

    // Calculate Discounts
    const calculateDiscount = () => {
        let discount = 0
        let shippingDiscount = 0 // Giả sử có phí ship hoặc giảm thẳng vào tổng

        const orderTotal = order.totalAmount || 0

        if (appliedDiscountVoucher) {
            if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_FIXED_AMOUNT) {
                discount += Number(appliedDiscountVoucher.discount_value || 0)
            } else if (appliedDiscountVoucher.type === VoucherType.DISCOUNT_PERCENTAGE) {
                let calculated = orderTotal * (Number(appliedDiscountVoucher.discount_value || 0) / 100)
                if (appliedDiscountVoucher.max_discount_value) {
                    calculated = Math.min(calculated, Number(appliedDiscountVoucher.max_discount_value))
                }
                discount += calculated
            }
        }

        if (appliedShippingVoucher) {
            if (appliedShippingVoucher.type === VoucherType.SHIPPING_FIXED_AMOUNT) {
                shippingDiscount += Number(appliedShippingVoucher.discount_value || 0)
            } else if (appliedShippingVoucher.type === VoucherType.SHIPPING_PERCENTAGE) {
                // Tạm thời tính % trên tổng tiền (vì chưa có phí ship riêng)
                let calculated = orderTotal * (Number(appliedShippingVoucher.discount_value || 0) / 100)
                if (appliedShippingVoucher.max_discount_value) {
                    calculated = Math.min(calculated, Number(appliedShippingVoucher.max_discount_value))
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
            {/* 1. Thông tin khách hàng & Thanh toán - Bố cục ngang thoáng đạt */}
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

            {/* 2. Danh sách sản phẩm dạng bảng (Table-like Grid) */}
            <div className="space-y-4">
                {/* Header của bảng (Giữ nguyên) */}
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
                    {order.items.map((item) => (
                        <div
                            key={item.id}
                            className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                        >
                            <OrderItemRow item={item} />
                        </div>
                    ))}
                </div>

                {isCustomer ? (
                    <div className="print-hidden flex flex-col">
                        {order.items.map((item) => (
                            <div
                                key={item.id}
                                className="px-2 py-4 border-b border-default-100 last:border-none transition-none"
                            >
                                <OrderItemRow item={item} />
                            </div>
                        ))}
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
                        {order.items.map((item) => (
                            <AccordionItem
                                key={item.id}
                                aria-label={item.productName}
                                title={<OrderItemRow item={item} />}
                            >
                                <RecipeIngredientList
                                    variantId={item.variantId}
                                    recipes={recipes}
                                />
                            </AccordionItem>
                        ))}
                    </Accordion>
                )}
            </div>

            {/* 3. Phần tổng kết (Summary) - Căn lề phải cực đẹp */}
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

                    {/* Voucher Section */}
                    {isCustomer && order.status === OrderStatus.PaymentPending && (
                        <div className="pt-2">
                            <Button
                                className="w-full bg-primary/10 text-primary border border-primary/20 font-bold"
                                onPress={() => setIsVoucherModalOpen(true)}
                                startContent={<Ticket size={18} />}
                            >
                                {appliedShippingVoucher || appliedDiscountVoucher
                                    ? 'Đã chọn tối đa voucher'
                                    : 'Chọn hoặc nhập mã Khuyến mãi'}
                            </Button>

                            {/* Discount lines */}
                            {(appliedShippingVoucher || appliedDiscountVoucher) && (
                                <div className="mt-3 space-y-2">
                                    {appliedDiscountVoucher && productDiscount > 0 && (
                                        <div className="flex justify-between text-xs font-medium text-success">
                                            <span>Giảm giá ({appliedDiscountVoucher.code})</span>
                                            <span>-{formatCurrencyWithSymbol(productDiscount)}</span>
                                        </div>
                                    )}
                                    {appliedShippingVoucher && shippingDiscount > 0 && (
                                        <div className="flex justify-between text-xs font-medium text-success">
                                            <span>Hỗ trợ phí ship ({appliedShippingVoucher.code})</span>
                                            <span>-{formatCurrencyWithSymbol(shippingDiscount)}</span>
                                        </div>
                                    )}
                                    {appliedDiscountVoucher?.type === VoucherType.GIFT && (
                                        <div className="flex justify-between items-center text-xs font-medium text-success p-2 bg-success/10 rounded-lg">
                                            <span>Quà tặng ({appliedDiscountVoucher.code})</span>
                                            <span className="px-2 py-0.5 bg-success text-white rounded-full text-[10px]">Đã áp dụng quà tặng</span>
                                        </div>
                                    )}
                                </div>
                            )}
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
                                    <span className="text-xs text-default-400 line-through font-normal">{formatCurrencyWithSymbol(order.totalAmount)}</span>
                                )}
                                {formatCurrencyWithSymbol(finalAmount)}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Voucher Selector Modal */}
            <VoucherSelector
                order={order}
                isOpen={isVoucherModalOpen}
                onOpenChange={setIsVoucherModalOpen}
                appliedShippingVoucher={appliedShippingVoucher}
                appliedDiscountVoucher={appliedDiscountVoucher}
                onApplyVouchers={(shipping, discount) => {
                    setAppliedShippingVoucher(shipping)
                    setAppliedDiscountVoucher(discount)
                }}
            />
        </div>
    )
}
