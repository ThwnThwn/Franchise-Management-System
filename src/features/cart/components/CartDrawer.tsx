"use client"

import React, { useCallback, useMemo, useState } from "react"
import { Divider } from "@heroui/react"
import { X, ShoppingBag } from "lucide-react"
import { CartItem } from "./CartItem"
import { ButtonIconStyled, ButtonStyled, CheckboxStyled } from "@/components"
import { useCartStore } from "@/features/cart/store"
import { PaymentMethod, useCreateOrder } from "@/features/orders"
import { useRouter } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

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
    // Không dùng total từ store nữa vì nó là tổng của tất cả
    const updateQuantity = useCartStore((state) => state.updateQuantity)
    const removeItem = useCartStore((state) => state.removeItem)

    // 1. State quản lý các item được chọn (lưu SKU)
    const [selectedKeys, setSelectedKeys] = useState<Set<string>>(
        () => new Set(items.map((i) => i.sku))
    )

    // 2. Logic tính toán dựa trên item được chọn
    const selectedItems = useMemo(() => {
        return items.filter((item) => selectedKeys.has(item.sku))
    }, [items, selectedKeys])

    const selectedTotal = useMemo(() => {
        return selectedItems.reduce((acc, item) => acc + item.basePrice * item.quantity, 0)
    }, [selectedItems])

    const tax = selectedTotal * TAX_RATE
    const finalTotal = selectedTotal + tax

    // 3. Các hàm xử lý Checkbox
    const isAllSelected = items.length > 0 && selectedKeys.size === items.length
    const isIndeterminate = selectedKeys.size > 0 && selectedKeys.size < items.length

    const handleSelectAll = (isSelected: boolean) => {
        if (isSelected) {
            const allSkus = new Set(items.map((item) => item.sku))
            setSelectedKeys(allSkus)
        } else {
            setSelectedKeys(new Set())
        }
    }

    const handleSelectItem = (sku: string) => {
        const nextSelectedKeys = new Set(selectedKeys)
        if (nextSelectedKeys.has(sku)) {
            nextSelectedKeys.delete(sku)
        } else {
            nextSelectedKeys.add(sku)
        }
        setSelectedKeys(nextSelectedKeys)
    }

    // Xử lý khi xóa item thì phải bỏ chọn nó khỏi state luôn
    const handleRemoveItem = (sku: string) => {
        removeItem(sku)
        const nextSelectedKeys = new Set(selectedKeys)
        nextSelectedKeys.delete(sku)
        setSelectedKeys(nextSelectedKeys)
    }

    const handleSuccess = useCallback(() => {
        const redirectPath = isCustomer ? "/orders" : "/"
        router.push(redirectPath)
        onClose()
        // Sau khi đặt hàng thành công, cần xóa các item đã mua khỏi giỏ
        // Note: Cần thêm logic xóa item đã chọn khỏi store ở đây nếu API không tự sync
        selectedItems.forEach((i) => removeItem(i.sku))
        setSelectedKeys(new Set())
    }, [isCustomer, onClose, router, removeItem, selectedItems])

    const createOrderMutation = useCreateOrder({ onSuccess: handleSuccess })

    const handleCheckout = async () => {
        if (selectedItems.length === 0) return

        await createOrderMutation.mutateAsync({
            items: selectedItems.map((item) => ({
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
                    className="fixed inset-0 bg-black/50 z-9999 transition-opacity"
                    onClick={onClose}
                />
            )}

            {/* Drawer */}
            <div
                className={`fixed top-0 right-0 h-full w-full max-w-md bg-background shadow-xl z-99999 transform transition-transform duration-300 ${
                    isOpen ? "translate-x-0" : "translate-x-full"
                }`}
            >
                <div className="flex flex-col h-full">
                    {/* Header */}
                    <div className="flex items-center justify-between px-4 py-1 border-b bg-default-50">
                        <div className="flex items-center gap-3">
                            {/* Checkbox Select All */}
                            <CheckboxStyled
                                isSelected={isAllSelected}
                                isIndeterminate={isIndeterminate}
                                onValueChange={handleSelectAll}
                            />
                            <h2 className="text-md font-bold flex items-center gap-2">
                                <ShoppingBag size={20} />
                                Giỏ hàng ({items.length})
                            </h2>
                        </div>
                        <ButtonIconStyled variant="light" onPress={onClose}>
                            <X size={20} />
                        </ButtonIconStyled>
                    </div>

                    {/* Cart Items */}
                    <div className="flex-1 overflow-y-auto p-4 scrollbar-hide">
                        {items.length === 0 ? (
                            <div className="flex flex-col items-center justify-center h-full text-gray-400">
                                <ShoppingBag size={64} className="mb-4 opacity-50" />
                                <p className="text-lg font-medium">Giỏ hàng trống</p>
                                <p className="text-sm">Hãy thêm sản phẩm vào giỏ hàng</p>
                            </div>
                        ) : (
                            <div>
                                {items.map((item, index) => (
                                    <button
                                        key={item.sku}
                                        className="w-full flex items-center justify-center gap-2"
                                        onClick={() => handleSelectItem(item.sku)}
                                    >
                                        <CheckboxStyled
                                            isSelected={selectedKeys.has(item.sku)}
                                            // onValueChange={() => handleSelectItem(item.sku)}
                                        />
                                        <CartItem
                                            key={index}
                                            item={item}
                                            onUpdateQuantity={updateQuantity}
                                            onRemove={() => handleRemoveItem(item.sku)}
                                        />
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    {items.length > 0 && (
                        <div className="border-t p-4 space-y-3 bg-background shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.1)]">
                            <div className="space-y-2">
                                <div className="flex justify-between text-sm text-default-500">
                                    <span>Đã chọn:</span>
                                    <span>{selectedItems.length} sản phẩm</span>
                                </div>
                                <div className="flex justify-between">
                                    <span>Tạm tính:</span>
                                    <span>{formatCurrencyWithSymbol(selectedTotal)}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span>Thuế VAT (10%):</span>
                                    <span>{formatCurrencyWithSymbol(tax)}</span>
                                </div>
                                <Divider className="my-2" />
                                <div className="flex justify-between items-center">
                                    <span className="text-lg font-semibold">Tổng cộng:</span>
                                    <span className="text-xl font-bold text-primary">
                                        {formatCurrencyWithSymbol(finalTotal)}
                                    </span>
                                </div>
                            </div>
                            <ButtonStyled
                                color="primary"
                                className="w-full font-bold shadow-lg shadow-primary/40"
                                onPress={handleCheckout}
                                isLoading={createOrderMutation.isPending}
                                isDisabled={selectedItems.length === 0} // Disable nếu chưa chọn gì
                            >
                                Mua hàng ({selectedItems.length})
                            </ButtonStyled>
                        </div>
                    )}
                </div>
            </div>
        </>
    )
}
