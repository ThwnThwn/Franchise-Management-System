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
import { useGetCart, useUpdateCartItem, useDeleteListCartItem, useClearCart } from "../hooks/useCartQuery"
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
    const isLogined = useTokenStore((s) => !!s.accessToken)
    const isCustomer = roleSlug === RoleSlug.Customer

    // API Hooks
    const { data: apiCart } = useGetCart()
    const updateCartItemMutation = useUpdateCartItem()
    const deleteListCartItemMutation = useDeleteListCartItem()

    // Local Store
    const localItems = useCartStore((s) => s.items)
    const localUpdateQuantity = useCartStore((state) => state.updateQuantity)
    const localRemoveItem = useCartStore((state) => state.removeItem)

    // Derived State
    const items = isLogined ? (apiCart ?? []) : localItems

    const handleUpdateQuantity = (sku: string, quantity: number) => {
        if (isLogined) {
            updateCartItemMutation.mutate({ sku, quantity })
        } else {
            localUpdateQuantity(sku, quantity)
        }
    }

    const handleRemoveItem = (sku: string) => {
        if (isLogined) {
            deleteListCartItemMutation.mutate({ skus: [sku] })
        } else {
            localRemoveItem(sku)
            // Update selected keys for local
            const nextSelectedKeys = new Set(selectedKeys)
            nextSelectedKeys.delete(sku)
            setSelectedKeys(nextSelectedKeys)
        }
    }

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

    const handleSuccess = useCallback(() => {
        const redirectPath = isCustomer ? "/orders" : "/"
        router.push(redirectPath)
        onClose()

        if (isLogined) {
            const skusToRemove = selectedItems.map(i => i.sku)
            deleteListCartItemMutation.mutate({ skus: skusToRemove })
        } else {
            // For guest, remove locally
            selectedItems.forEach((i) => localRemoveItem(i.sku))
        }

        setSelectedKeys(new Set())
    }, [isCustomer, onClose, router, isLogined, selectedItems, deleteListCartItemMutation, localRemoveItem])

    const createOrderMutation = useCreateOrder({ onSuccess: handleSuccess })

    const currentStoreId = useCartStore((s) => s.currentStoreId)

    const handleCheckout = async () => {
        if (selectedItems.length === 0) return

        await createOrderMutation.mutateAsync({
            items: selectedItems.map((item) => ({
                sku: item.sku,
                quantity: item.quantity
            })),
            tax,
            paymentMethod: PaymentMethod.Cash,
            notes: "",
            storeId: currentStoreId || (selectedItems.length > 0 ? selectedItems[0].storeId : undefined)
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
                className={`fixed top-0 right-0 h-full w-full max-w-md bg-background shadow-xl z-99999 transform transition-transform duration-300 ${isOpen ? "translate-x-0" : "translate-x-full"
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
                                    <div
                                        key={item.sku}
                                        className="w-full flex items-center justify-center gap-2 cursor-pointer"
                                        onClick={(e) => {
                                            // Prevent triggering select when clicking on interactive elements inside CartItem
                                            // This is a simple check, ideally CartItem should handle its own clicks or we separate concerns
                                            const target = e.target as HTMLElement
                                            if (target.tagName === "BUTTON" || target.closest("button") || target.tagName === "INPUT") {
                                                return
                                            }
                                            handleSelectItem(item.sku)
                                        }}
                                    >
                                        <CheckboxStyled
                                            isSelected={selectedKeys.has(item.sku)}
                                            onValueChange={() => handleSelectItem(item.sku)}
                                            // Stop propagation to prevent double toggle if div click also fires
                                            className="pointer-events-none sm:pointer-events-auto"
                                        />
                                        <div className="flex-1 pointer-events-none sm:pointer-events-auto">
                                            <CartItem
                                                key={index}
                                                item={item}
                                                onUpdateQuantity={handleUpdateQuantity}
                                                onRemove={() => handleRemoveItem(item.sku)}
                                            />
                                        </div>
                                    </div>
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
