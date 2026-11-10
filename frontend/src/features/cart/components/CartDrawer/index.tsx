"use client"

import { useEffect, useRef, useState } from "react"
import { addToast } from "@heroui/react"
import { X, ShoppingBag } from "lucide-react"
import { ButtonIconStyled, CheckboxStyled } from "@/components"
import { useCartStore } from "@/features/cart/store"
import { OrderType, useCreateOrder } from "@/features/orders"
import { useRouter } from "next/navigation"
import { useCartDrawerDiscloresureSingleton, useTokenStore } from "@/hooks"
import {
    useGetCart,
    useUpdateCartItem,
    useDeleteListCartItem
} from "@/features/cart/hooks/useCartQuery"
import { RoleSlug } from "@/constants/enums"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { useGetMe } from "@/features/profile/hooks"
import { CheckoutFormValues, checkoutSchema } from "./schema"
import { useFormik } from "formik"
import { CartList } from "./CartList"
import { CheckoutForm } from "./CheckoutForm"
import { CartFooter } from "./CartFooter"

const TAX_RATE = 0.1

export default function CartDrawer() {
    const { data: user } = useGetMe()

    // Quản lý trạng thái đóng mở Drawer và điều hướng trang
    const { isOpen, onClose } = useCartDrawerDiscloresureSingleton()
    const router = useRouter()

    // Lấy thông tin vai trò người dùng để phân quyền gọi API
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isCustomer = roleSlug === RoleSlug.Customer

    // Các Hook xử lý dữ liệu giỏ hàng từ API
    const { data: apiCart } = useGetCart({ enabled: isCustomer && isOpen })
    const updateCartItemMutation = useUpdateCartItem()
    const deleteListCartItemMutation = useDeleteListCartItem()
    const currentStore = useCartStore((s) => s.currentStore)

    // --- CÁC TRẠNG THÁI QUẢN LÝ TẠI CHỖ ---

    // Lưu trữ danh sách các mặt hàng bị thiếu kho (được trả về từ backend)
    const [shortageMap, setShortageMap] = useState<Record<string, number>>({})

    // Trạng thái hiển thị số lượng ảo để người dùng thấy thay đổi ngay lập tức (Optimistic Update)
    const [optimisticQuantities, setOptimisticQuantities] = useState<Record<string, number>>({})

    // Lưu trữ các hàm trì hoãn (debounce) riêng biệt cho từng sản phẩm để tránh spam API
    const debounceMapRef = useRef<Record<string, ReturnType<typeof debouncedWrapper>>>({})

    // Quản lý danh sách các sản phẩm đang được tích chọn để thanh toán
    const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set())

    // Tạo chuỗi định danh dữ liệu để tối ưu hóa việc chạy useEffect
    const items = apiCart || []
    const itemsFingerprint = JSON.stringify(items.map((i) => ({ sku: i.sku, q: i.quantity })))

    // Đồng bộ dữ liệu từ server vào trạng thái hiển thị tại chỗ mỗi khi dữ liệu giỏ hàng thay đổi
    useEffect(() => {
        if (items.length > 0) {
            setSelectedKeys((prev) =>
                prev.size === 0 ? new Set(items.map((item) => item.sku)) : prev
            )
            const newQuantities = items.reduce(
                (acc, item) => ({ ...acc, [item.sku]: item.quantity }),
                {}
            )
            setOptimisticQuantities(newQuantities)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [itemsFingerprint])

    // Dọn dẹp các hàm debounce khi component bị hủy để tránh rò rỉ bộ nhớ
    useEffect(() => {
        const currentMap = debounceMapRef.current
        return () => {
            Object.values(currentMap).forEach((func) => func.cancel && func.cancel())
        }
    }, [])

    // --- CÁC HÀM XỬ LÝ SỰ KIỆN ---

    // Cập nhật số lượng sản phẩm: Hiển thị ngay lập tức và gọi API sau 1 giây nếu không có thay đổi thêm
    const handleUpdateQuantity = (sku: string, newQuantity: number) => {
        setOptimisticQuantities((prev) => {
            if (newQuantity <= 0) {
                const next = { ...prev }
                delete next[sku] // Xóa khỏi state ngay lập tức nếu số lượng bằng 0
                return next
            }
            return { ...prev, [sku]: newQuantity }
        })

        // Xóa trạng thái báo lỗi thiếu kho của sản phẩm này khi người dùng bắt đầu chỉnh sửa số lượng
        if (shortageMap[sku]) {
            const nextMap = { ...shortageMap }
            delete nextMap[sku]
            setShortageMap(nextMap)
        }

        // Tạo hàm debounce cho SKU này nếu chưa có
        if (!debounceMapRef.current[sku]) {
            debounceMapRef.current[sku] = debouncedWrapper(async (q: number) => {
                if (q <= 0) {
                    await deleteListCartItemMutation.mutateAsync({ skus: [sku] })
                } else {
                    await updateCartItemMutation.mutateAsync({ sku, quantity: q })
                }
            }, 1000)
        }
        debounceMapRef.current[sku](newQuantity)
    }

    // Xóa một sản phẩm khỏi giỏ hàng và cập nhật lại danh sách lựa chọn
    const handleRemoveItem = (sku: string) => {
        deleteListCartItemMutation.mutate({ skus: [sku] })
        if (selectedKeys.has(sku)) {
            const nextKeys = new Set(selectedKeys)
            nextKeys.delete(sku)
            setSelectedKeys(nextKeys)
        }
    }

    // Xử lý chọn hoặc bỏ chọn toàn bộ sản phẩm
    const handleSelectAll = (isSelected: boolean) => {
        setSelectedKeys(isSelected ? new Set(items.map((item) => item.sku)) : new Set())
    }

    // Xử lý chọn hoặc bỏ chọn từng sản phẩm đơn lẻ
    const handleSelectItem = (sku: string) => {
        const nextSelectedKeys = new Set(selectedKeys)
        if (nextSelectedKeys.has(sku)) nextSelectedKeys.delete(sku)
        else nextSelectedKeys.add(sku)
        setSelectedKeys(nextSelectedKeys)
    }

    // --- LOGIC TÍNH TOÁN HÓA ĐƠN ---

    // Danh sách các mặt hàng người dùng đã chọn để mua
    const selectedItems = items.filter((item) => selectedKeys.has(item.sku))

    // Tính tổng tiền hàng dựa trên số lượng đang hiển thị (số lượng ảo)
    const selectedTotal = selectedItems.reduce((acc, item) => {
        const currentQty = optimisticQuantities[item.sku] ?? item.quantity
        return acc + item.basePrice * currentQty
    }, 0)

    const tax = selectedTotal * TAX_RATE
    const finalTotal = selectedTotal + tax

    // Trạng thái hiển thị của checkbox tổng
    const isAllSelected = items.length > 0 && selectedKeys.size === items.length
    const isIndeterminate = selectedKeys.size > 0 && selectedKeys.size < items.length

    // --- XỬ LÝ THANH TOÁN ---
    const createOrderMutation = useCreateOrder({
        onSuccess: (res) => {
            // Kiểm tra phản hồi từ backend xem có bị thiếu hàng trong kho hay không
            if (res.checkStockResponse && !res.checkStockResponse.isEnough) {
                const newShortageMap: Record<string, number> = {}
                res.checkStockResponse.shortageItems.forEach((item) => {
                    newShortageMap[item.sku] = item.maxFeasibleQuantity
                })
                setShortageMap(newShortageMap)
                return // Dừng xử lý thành công nếu kho không đủ
            }

            // Xử lý khi đặt hàng thành công hoàn toàn
            router.push(isCustomer ? `/orders/${res.orderId}` : "/")
            onClose()

            // Reset các trạng thái local
            setSelectedKeys(new Set())
            setShortageMap({})
        }
    })

    // --- SETUP FORMIK ---
    const formik = useFormik<CheckoutFormValues>({
        initialValues: {
            type: OrderType.Delivery,
            address: "", // Sẽ được update khi user load xong
            notes: "",
            scheduledTime: ""
        },
        validationSchema: checkoutSchema,
        onSubmit: async (values) => {
            // Chặn nếu chưa chọn cửa hàng
            if (!currentStore) {
                addToast({ title: "Vui lòng chọn cửa hàng!", color: "warning" })
                return
            }

            if (selectedItems.length === 0) return
            setShortageMap({})

            // Gắn payload từ Formik vào Mutation
            await createOrderMutation.mutateAsync({
                items: selectedItems.map((item) => ({
                    sku: item.sku,
                    quantity: optimisticQuantities[item.sku] ?? item.quantity
                })),
                storeId: currentStore.id,
                type: values.type,
                address: values.type === OrderType.Delivery ? values.address : undefined,
                notes: values.notes || "",
                scheduledTime: values.scheduledTime || undefined
            })
        }
    })

    // Cập nhật địa chỉ mặc định khi API user trả về data
    useEffect(() => {
        if (user?.address && !formik.values.address) {
            formik.setFieldValue("address", user.address)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [user])

    // Kiểm tra xem có bất kỳ sản phẩm nào đang được chọn bị dính lỗi kho hay không
    const hasShortageError = selectedItems.some((item) => shortageMap[item.sku] !== undefined)

    return (
        <>
            {/* Lớp nền mờ phía sau Drawer */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-50 transition-opacity"
                    onClick={onClose}
                />
            )}

            {/* Cấu trúc chính của Drawer */}
            <div
                className={`fixed top-0 right-0 h-full w-full max-w-md bg-background shadow-xl z-99999 transform transition-transform duration-300 ${isOpen ? "translate-x-0" : "translate-x-full"}`}
            >
                <div className="flex flex-col h-full">
                    {/* Phần đầu: Tiêu đề và nút đóng */}
                    <div className="flex items-center justify-between px-4 py-1 border-b bg-default-50">
                        <div className="flex items-center gap-3">
                            <CheckboxStyled
                                isSelected={isAllSelected}
                                isIndeterminate={isIndeterminate}
                                onValueChange={handleSelectAll}
                            />
                            <h2 className="text-md font-bold flex items-center gap-2">
                                <ShoppingBag size={20} /> Giỏ hàng ({items.length})
                            </h2>
                        </div>
                        <ButtonIconStyled variant="light" onPress={onClose}>
                            <X size={20} />
                        </ButtonIconStyled>
                    </div>

                    {/* Danh sách các mặt hàng trong giỏ */}
                    <div className="flex-1 overflow-y-auto p-4 scrollbar-hide">
                        {items.length === 0 ? (
                            <div className="flex flex-col items-center justify-center h-full text-gray-400">
                                <ShoppingBag size={64} className="mb-4 opacity-50" />
                                <p className="text-lg font-medium">Giỏ hàng trống</p>
                                <p className="text-sm">Hãy thêm sản phẩm vào giỏ hàng</p>
                            </div>
                        ) : (
                            <>
                                <CartList
                                    items={items}
                                    selectedKeys={selectedKeys}
                                    optimisticQuantities={optimisticQuantities}
                                    shortageMap={shortageMap}
                                    onSelectItem={handleSelectItem}
                                    onUpdateQuantity={handleUpdateQuantity}
                                    onRemoveItem={handleRemoveItem}
                                />
                                <CheckoutForm formik={formik} />
                            </>
                        )}
                    </div>

                    {/* Phần chân trang: Hiển thị tổng tiền và nút mua hàng */}
                    <CartFooter
                        selectedCount={selectedItems.length}
                        selectedTotal={selectedTotal}
                        tax={tax}
                        finalTotal={finalTotal}
                        isLoading={createOrderMutation.isPending}
                        hasShortageError={hasShortageError}
                        onSubmit={() => formik.handleSubmit()}
                    />
                </div>
            </div>
        </>
    )
}
