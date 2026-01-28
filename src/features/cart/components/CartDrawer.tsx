"use client"

import React, { useCallback } from "react"
import { Divider } from "@heroui/react"
import { X, ShoppingBag } from "lucide-react"
import { CartItem } from "./CartItem"
import { ButtonIconStyled, ButtonStyled } from "@/components"
import { useCartStore } from "@/features/cart/store"
import { PaymentMethod, useCreateOrder } from "@/features/orders"
import { useRouter } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

interface CartDrawerProps {
    isOpen: boolean
    onClose: () => void
}

const TAX_RATE = 0.1

export const CartDrawer: React.FC<CartDrawerProps> = ({ isOpen, onClose }) => {
    const router = useRouter()
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isCustomer = roleSlug === RoleSlug.Customer

    const items = useCartStore((s) => s.items)
    const total = useCartStore((state) => state.total)
    const updateQuantity = useCartStore((state) => state.updateQuantity)
    const removeItem = useCartStore((state) => state.removeItem)

    const handleSuccess = useCallback(() => {
        const redirectPath = isCustomer ? "/orders" : "/"
        router.push(redirectPath)
        onClose()
    }, [isCustomer, onClose, router])

    const createOrderMutation = useCreateOrder({ onSuccess: handleSuccess })

    const tax = total * TAX_RATE
    const finalTotal = total + tax

    const handleCheckout = async () => {
        if (items.length === 0) return

        await createOrderMutation.mutateAsync({
            items: items.map((item) => ({
                sku: item.sku,
                quantity: item.quantity
            })),
            tax,
            paymentMethod: PaymentMethod.Cash,
            notes: ""
        })
    }

    return (
        <>
            {/* Overlay */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-40 transition-opacity"
                    onClick={onClose}
                />
            )}

            {/* Drawer */}
            <div
                className={`fixed top-0 right-0 h-full w-full max-w-md bg-background shadow-xl z-50 transform transition-transform duration-300 ${
                    isOpen ? "translate-x-0" : "translate-x-full"
                }`}
            >
                <div className="flex flex-col h-full">
                    {/* Header */}
                    <div className="flex items-center justify-between p-4 border-b">
                        <h2 className="text-xl font-bold flex items-center gap-2">
                            <ShoppingBag size={24} />
                            Giỏ hàng ({items.length})
                        </h2>
                        <ButtonIconStyled variant="light" onPress={onClose}>
                            <X size={24} />
                        </ButtonIconStyled>
                    </div>

                    {/* Cart Items */}
                    <div className="flex-1 overflow-y-auto p-4">
                        {items.length === 0 ? (
                            <div className="flex flex-col items-center justify-center h-full text-gray-400">
                                <ShoppingBag size={64} className="mb-4" />
                                <p className="text-lg">Giỏ hàng trống</p>
                                <p className="text-sm">Hãy thêm sản phẩm vào giỏ hàng</p>
                            </div>
                        ) : (
                            <div>
                                {items.map((item) => (
                                    <CartItem
                                        key={item.sku}
                                        item={item}
                                        onUpdateQuantity={updateQuantity}
                                        onRemove={removeItem}
                                    />
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    {items.length > 0 && (
                        <div className="border-t p-4 space-y-3">
                            <div className="space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span>Tạm tính:</span>
                                    <span>{total.toLocaleString("vi-VN")} đ</span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span>Thuế VAT (10%):</span>
                                    <span>{tax.toLocaleString("vi-VN")} đ</span>
                                </div>
                                <Divider />
                                <div className="flex justify-between items-center">
                                    <span className="text-lg font-semibold">Tổng cộng:</span>
                                    <span className="text-xl font-bold text-primary">
                                        {finalTotal.toLocaleString("vi-VN")} đ
                                    </span>
                                </div>
                            </div>
                            <ButtonStyled
                                color="primary"
                                size="lg"
                                className="w-full"
                                onPress={handleCheckout}
                                isLoading={createOrderMutation.isPending}
                                isDisabled={items.length === 0}
                            >
                                Đặt hàng
                            </ButtonStyled>
                        </div>
                    )}
                </div>
            </div>
        </>
    )
}
