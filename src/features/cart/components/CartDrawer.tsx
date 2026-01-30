"use client"

import React, { useEffect, useMemo, useRef, useState } from "react"
import { Divider } from "@heroui/react"
import { X, ShoppingBag } from "lucide-react"
import { CartItem } from "./CartItem"
import { ButtonIconStyled, ButtonStyled, CheckboxStyled } from "@/components"
import { useCartStore } from "@/features/cart/store"
import { PaymentMethod, useCreateOrder } from "@/features/orders"
import { useRouter } from "next/navigation"
import { useCartDrawerDiscloresureSingleton, useTokenStore } from "@/hooks"
import { useGetCart, useUpdateCartItem, useDeleteListCartItem } from "../hooks/useCartQuery"
import { RoleSlug } from "@/constants/enums"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"

const TAX_RATE = 0.1

export default function CartDrawer() {
    const { isOpen, onClose } = useCartDrawerDiscloresureSingleton()
    const router = useRouter()
    const roleSlug = useTokenStore((s) => s.roleSlug)
    // const isLogined = useTokenStore((s) => !!s.accessToken)
    const isCustomer = roleSlug === RoleSlug.Customer

    // --- API HOOKS ---
    const { data: apiCart } = useGetCart({ enabled: isCustomer })
    const updateCartItemMutation = useUpdateCartItem()
    const deleteListCartItemMutation = useDeleteListCartItem()
    const currentStoreId = useCartStore((s) => s.currentStoreId)

    // --- STATE & REF ---
    // 1. Items lấy từ API (mặc định rỗng)
    const items = useMemo(() => {
        return apiCart || []
    }, [apiCart])

    // 2. State Optimistic (Hiển thị số lượng ảo tức thì để UI mượt)
    const [optimisticQuantities, setOptimisticQuantities] = useState<Record<string, number>>({})

    // 3. Ref để quản lý các hàm debounce riêng biệt cho từng SKU
    const debounceMapRef = useRef<Record<string, ReturnType<typeof debouncedWrapper>>>({})

    // 4. State quản lý checkbox chọn sản phẩm
    const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set())

    // Tạo ra một chuỗi đại diện cho dữ liệu (String Fingerprint)
    // React sẽ so sánh chuỗi này. Nếu dữ liệu server ko đổi -> chuỗi ko đổi -> useEffect KHÔNG chạy -> Hết lỗi.
    const itemsFingerprint = JSON.stringify(items.map((i) => ({ sku: i.sku, q: i.quantity })))

    // useEffect(() => {
    //     if (items.length > 0) {
    //         // Nếu muốn mặc định chọn hết khi mới vào:
    //         setSelectedKeys((prev) => {
    //             // Chỉ set nếu chưa chọn gì (lần đầu load)
    //             // Hoặc nếu bạn muốn luôn chọn hết mỗi khi data thay đổi thì bỏ check prev.size
    //             if (prev.size === 0) {
    //                 return new Set(items.map((item) => item.sku))
    //             }
    //             return prev
    //         })
    //     }
    //     // eslint-disable-next-line react-hooks/exhaustive-deps
    // }, [itemsFingerprint])

    useEffect(() => {
        if (items.length > 0) {
            setSelectedKeys((prev) => {
                if (prev.size === 0) {
                    return new Set(items.map((item) => item.sku))
                }
                return prev
            })
            const newQuantities = items.reduce(
                (acc, item) => ({
                    ...acc,
                    [item.sku]: item.quantity
                }),
                {}
            )
            setOptimisticQuantities(newQuantities)
        } else {
            setOptimisticQuantities({})
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [itemsFingerprint])
    useEffect(() => {
        const currentMap = debounceMapRef.current // Copy ref ra biến local
        return () => {
            // Cleanup dùng biến local để đảm bảo an toàn
            Object.values(currentMap).forEach((func) => func.cancel && func.cancel())
        }
    }, [])

    // --- HANDLERS ---

    // Hàm update số lượng: Cập nhật UI ngay -> Gọi API sau 1s
    const handleUpdateQuantity = (sku: string, newQuantity: number) => {
        // A. Cập nhật giao diện ngay lập tức
        setOptimisticQuantities((prev) => ({ ...prev, [sku]: newQuantity }))

        // B. Gọi API (có Debounce)
        if (!debounceMapRef.current[sku]) {
            debounceMapRef.current[sku] = debouncedWrapper(async (q: number) => {
                await updateCartItemMutation.mutateAsync({ sku, quantity: q })
            }, 1000)
        }
        // Kích hoạt debounce
        debounceMapRef.current[sku](newQuantity)
    }

    const handleRemoveItem = (sku: string) => {
        deleteListCartItemMutation.mutate({ skus: [sku] })
        // Xóa khỏi danh sách đã chọn nếu đang chọn
        if (selectedKeys.has(sku)) {
            const nextKeys = new Set(selectedKeys)
            nextKeys.delete(sku)
            setSelectedKeys(nextKeys)
        }
    }

    // --- LOGIC TÍNH TOÁN & CHECKBOX ---
    const isAllSelected = items.length > 0 && selectedKeys.size === items.length
    const isIndeterminate = selectedKeys.size > 0 && selectedKeys.size < items.length

    const handleSelectAll = (isSelected: boolean) => {
        if (isSelected) {
            setSelectedKeys(new Set(items.map((item) => item.sku)))
        } else {
            setSelectedKeys(new Set())
        }
    }

    const handleSelectItem = (sku: string) => {
        const nextSelectedKeys = new Set(selectedKeys)
        if (nextSelectedKeys.has(sku)) nextSelectedKeys.delete(sku)
        else nextSelectedKeys.add(sku)
        setSelectedKeys(nextSelectedKeys)
    }

    // Lọc ra các item đang được chọn
    const selectedItems = items.filter((item) => selectedKeys.has(item.sku))

    // Tính tổng tiền dựa trên số lượng ẢO (optimistic) để giá nhảy ngay lập tức
    const selectedTotal = selectedItems.reduce((acc, item) => {
        const currentQty = optimisticQuantities[item.sku] ?? item.quantity
        return acc + item.basePrice * currentQty
    }, 0)

    const tax = selectedTotal * TAX_RATE
    const finalTotal = selectedTotal + tax

    // --- CHECKOUT ---
    const handleSuccess = () => {
        router.push(isCustomer ? "/orders" : "/")
        onClose()
        // Sau khi tạo đơn thành công, xóa các món đã chọn khỏi giỏ hàng
        const skusToRemove = selectedItems.map((i) => i.sku)
        if (skusToRemove.length > 0) {
            deleteListCartItemMutation.mutate({ skus: skusToRemove })
        }
        setSelectedKeys(new Set())
    }

    const createOrderMutation = useCreateOrder({ onSuccess: handleSuccess })
    const handleCheckout = async () => {
        if (selectedItems.length === 0) return
        await createOrderMutation.mutateAsync({
            items: selectedItems.map((item) => ({
                sku: item.sku,
                // Lấy số lượng mới nhất từ optimistic state
                quantity: optimisticQuantities[item.sku] ?? item.quantity
            })),
            tax,
            paymentMethod: PaymentMethod.Cash,
            notes: "",
            storeId: currentStoreId || selectedItems[0]?.storeId
        })
    }

    return (
        <>
            {/* Overlay */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-50 transition-opacity"
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
                    <div className="flex items-center justify-between px-4 py-1 border-b bg-default-50">
                        <div className="flex items-center gap-3">
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

                    {/* Cart Items List */}
                    <div className="flex-1 overflow-y-auto p-4 scrollbar-hide">
                        {items.length === 0 ? (
                            <div className="flex flex-col items-center justify-center h-full text-gray-400">
                                <ShoppingBag size={64} className="mb-4 opacity-50" />
                                <p className="text-lg font-medium">Giỏ hàng trống</p>
                                <p className="text-sm">Hãy thêm sản phẩm vào giỏ hàng</p>
                            </div>
                        ) : (
                            <div className="space-y-4">
                                {items.map((item) => (
                                    <div
                                        key={item.sku}
                                        className="w-full flex items-center justify-center gap-2 cursor-pointer"
                                        // Xử lý click vào vùng trống để chọn checkbox (UX)
                                        onClick={(e) => {
                                            const target = e.target as HTMLElement
                                            if (
                                                target.tagName === "BUTTON" ||
                                                target.closest("button") ||
                                                target.tagName === "INPUT"
                                            ) {
                                                return
                                            }
                                            handleSelectItem(item.sku)
                                        }}
                                    >
                                        <CheckboxStyled
                                            isSelected={selectedKeys.has(item.sku)}
                                            onValueChange={() => handleSelectItem(item.sku)}
                                            className="pointer-events-none sm:pointer-events-auto"
                                        />
                                        <div className="flex-1 pointer-events-none sm:pointer-events-auto">
                                            <CartItem
                                                item={{
                                                    ...item,
                                                    // Quan trọng: Ghi đè quantity bằng số lượng ảo để UI cập nhật ngay
                                                    quantity:
                                                        optimisticQuantities[item.sku] ??
                                                        item.quantity
                                                }}
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
                                isDisabled={selectedItems.length === 0}
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
