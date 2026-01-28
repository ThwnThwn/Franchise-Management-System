"use client"

import React from "react"
import { DropdownTrigger, DropdownMenu, DropdownItem } from "@heroui/react"
import { DropdownStyled, ButtonIconStyled, ButtonStyled } from "@/components"
import { ShoppingCart, X } from "lucide-react"
import { CART_TEXT, useCartStore } from "@/features/cart"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { useCreateOrder } from "@/features"

const TAX_RATE = 0.1 // 10% VAT

export function CartPopover() {
    // const { items, total, itemCount, removeItem } = useCart()
    const items = useCartStore((state) => state.items)
    const total = useCartStore((state) => state.total)
    const itemCount = useCartStore((state) => state.count)
    const removeItem = useCartStore((state) => state.removeItem)

    const createOrderMutation = useCreateOrder()
    const isEmpty = items.length === 0

    const tax = total * TAX_RATE
    const finalTotal = total + tax

    const handleCheckout = async () => {
        if (items.length === 0) return

        try {
            const result = await createOrderMutation.mutateAsync({
                items: items.map((item) => ({
                    sku: item.sku,
                    quantity: item.quantity
                })),
                tax,
                paymentMethod: 0, // Default: Cash
                notes: ""
            })

            alert(`Đặt hàng thành công! Mã đơn hàng: ${result.orderCode}`)
        } catch (error) {
            console.error("Failed to create order:", error)
            alert("Đặt hàng thất bại. Vui lòng thử lại!")
        }
    }

    return (
        <DropdownStyled classNames={{ content: "min-w-56 max-w-80" }}>
            <DropdownTrigger>
                <ButtonIconStyled className="z-9999 relative" aria-label="Giỏ hàng">
                    <ShoppingCart size={24} />
                    {itemCount > 0 && (
                        <span className="absolute -top-1 -right-1 bg-danger text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                            {itemCount}
                        </span>
                    )}
                </ButtonIconStyled>
            </DropdownTrigger>
            <DropdownMenu aria-label="Giỏ hàng" disabledKeys={isEmpty ? ["empty"] : []}>
                {isEmpty ? (
                    <DropdownItem
                        key="empty"
                        isDisabled
                        className="cursor-default justify-center py-4"
                    >
                        {CART_TEXT.labels.CART_EMPTY}
                    </DropdownItem>
                ) : (
                    <>
                        {items.map((item, i) => (
                            <DropdownItem
                                key={`${item.sku}-${i}`}
                                className="cursor-default py-3 px-4"
                                textValue={item.productName}
                            >
                                <div className="flex items-start gap-2">
                                    <div className="flex flex-col gap-0.5 flex-1">
                                        <span className="font-medium">{item.productName}</span>
                                        <span className="text-small text-default-500">
                                            {item.sizeName && `Size ${item.sizeName}`} ×{" "}
                                            {item.quantity} —{" "}
                                            {formatCurrencyWithSymbol(
                                                item.basePrice * item.quantity
                                            )}
                                        </span>
                                    </div>
                                    <button
                                        type="button"
                                        className="text-default-400 hover:text-danger shrink-0"
                                        onClick={(e) => {
                                            e.preventDefault()
                                            e.stopPropagation()
                                            removeItem(item.sku)
                                        }}
                                        aria-label="Xóa sản phẩm khỏi giỏ"
                                    >
                                        <X size={16} />
                                    </button>
                                </div>
                            </DropdownItem>
                        ))}
                        <DropdownItem
                            key="cart-summary"
                            className="cursor-default pointer-events-none border-t border-default-200 mt-1 pt-3"
                        >
                            <div className="flex flex-col gap-2">
                                <div className="flex flex-col gap-1">
                                    <span className="text-small text-default-500">
                                        Tạm tính:{" "}
                                        <span className="font-semibold text-foreground">
                                            {formatCurrencyWithSymbol(total)}
                                        </span>
                                    </span>
                                    <span className="text-small text-default-500">
                                        Thuế VAT (10%):{" "}
                                        <span className="font-semibold text-foreground">
                                            {formatCurrencyWithSymbol(tax)}
                                        </span>
                                    </span>
                                    <span className="text-small text-default-500">
                                        Tổng cộng:{" "}
                                        <span className="font-semibold text-primary">
                                            {formatCurrencyWithSymbol(finalTotal)}
                                        </span>
                                    </span>
                                </div>
                                <ButtonStyled
                                    color="primary"
                                    className="w-full mt-1 pointer-events-auto"
                                    onPress={handleCheckout}
                                    isLoading={createOrderMutation.isPending}
                                    isDisabled={isEmpty}
                                >
                                    Đặt hàng
                                </ButtonStyled>
                            </div>
                        </DropdownItem>
                    </>
                )}
            </DropdownMenu>
        </DropdownStyled>
    )
}
