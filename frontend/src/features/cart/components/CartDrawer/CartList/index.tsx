import { AlertTriangle } from "lucide-react"
import { motion, AnimatePresence } from "framer-motion"
import { CheckboxStyled } from "@/components"
import { GetCartResponse } from "@/features/cart/models/schema/response"
import { CartItem } from "@/features/cart/components"

// 1. Định nghĩa rành mạch các Props mà component này cần nhận vào
interface CartListProps {
    items: GetCartResponse
    selectedKeys: Set<string>
    optimisticQuantities: Record<string, number>
    shortageMap: Record<string, number>

    // Các hàm Callbacks đẩy ngược sự kiện lên thằng cha (CartDrawer)
    onSelectItem: (sku: string) => void
    onUpdateQuantity: (sku: string, quantity: number) => void
    onRemoveItem: (sku: string) => void
}

export function CartList({
    items,
    selectedKeys,
    optimisticQuantities,
    shortageMap,
    onSelectItem,
    onUpdateQuantity,
    onRemoveItem
}: CartListProps) {
    return (
        <div className="space-y-3">
            {items.map((item) => {
                // Kiểm tra xem item này có đang bị dính lỗi thiếu kho không
                const maxAvailable = shortageMap[item.sku]
                const isShortage = maxAvailable !== undefined

                return (
                    <div key={item.sku} className="flex flex-col gap-2 mb-0">
                        {/* Box chứa UI món hàng */}
                        <div
                            className={`flex items-center gap-2 p-2 rounded-2xl transition-all border-2 ${
                                isShortage ? "bg-danger-50 border-danger-200" : "border-transparent"
                            }`}
                            onClick={() => onSelectItem(item.sku)}
                        >
                            <div onClick={(e) => e.stopPropagation()}>
                                <CheckboxStyled
                                    isSelected={selectedKeys.has(item.sku)}
                                    onValueChange={() => onSelectItem(item.sku)}
                                />
                            </div>

                            <div className="flex-1">
                                <CartItem
                                    item={{
                                        ...item,
                                        // Ưu tiên hiển thị số lượng ảo đang gõ, nếu không có thì lấy số gốc của API
                                        quantity: optimisticQuantities[item.sku] ?? item.quantity
                                    }}
                                    onUpdateQuantity={(_, qty) => onUpdateQuantity(item.sku, qty)}
                                    onRemove={() => onRemoveItem(item.sku)}
                                />
                            </div>
                        </div>

                        {/* Animation Cảnh báo hết hàng */}
                        <AnimatePresence>
                            {isShortage && (
                                <motion.div
                                    initial={{ height: 0, opacity: 0, y: -10 }}
                                    animate={{ height: "auto", opacity: 1, y: 0 }}
                                    exit={{ height: 0, opacity: 0, y: -10 }}
                                    transition={{ duration: 0.2, ease: "easeOut" }}
                                    className="overflow-hidden"
                                >
                                    <div className="flex items-center gap-2 px-10 pt-1 pb-2 text-[11px] font-bold text-danger">
                                        <AlertTriangle size={14} className="shrink-0" />
                                        <span className="leading-tight">
                                            Kho chỉ còn {maxAvailable} món.
                                        </span>
                                        <button
                                            className="ml-auto underline text-primary active:scale-95 transition-transform"
                                            onClick={(e) => {
                                                e.stopPropagation()
                                                onUpdateQuantity(item.sku, maxAvailable)
                                            }}
                                        >
                                            {maxAvailable <= 0
                                                ? "Xoá khỏi giỏ hàng"
                                                : "Cập nhật ngay"}
                                        </button>
                                    </div>
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </div>
                )
            })}
        </div>
    )
}
