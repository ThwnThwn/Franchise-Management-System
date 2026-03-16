import React from "react"
import { Divider } from "@heroui/react"
import { ButtonStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

// Định nghĩa Props rõ ràng
interface CartFooterProps {
    selectedCount: number
    selectedTotal: number
    tax: number
    finalTotal: number
    isLoading: boolean
    hasShortageError: boolean
    onSubmit: () => void
}

export function CartFooter({
    selectedCount,
    selectedTotal,
    tax,
    finalTotal,
    isLoading,
    hasShortageError,
    onSubmit
}: CartFooterProps) {
    // Nếu chưa chọn món nào thì không cần render Footer (Tùy logic UI của ní, thường thì ẩn luôn cho đẹp)
    if (selectedCount === 0) return null

    return (
        <div className="border-t p-4 space-y-4 bg-background shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.1)] z-10 relative">
            {/* Phần chi tiết hóa đơn */}
            <div className="space-y-2">
                <div className="flex justify-between text-sm text-default-500">
                    <span>Đã chọn ({selectedCount} món):</span>
                    <span>{formatCurrencyWithSymbol(selectedTotal)}</span>
                </div>

                <div className="flex justify-between text-sm text-default-500">
                    <span>Thuế VAT dự kiến (10%):</span>
                    <span>{formatCurrencyWithSymbol(tax)}</span>
                </div>

                <Divider className="my-2 opacity-50" />

                <div className="flex justify-between items-center font-bold">
                    <span className="text-md uppercase tracking-wider">
                        Tổng thanh toán dự kiến:
                    </span>
                    <span className="text-xl text-primary">
                        {formatCurrencyWithSymbol(finalTotal)}
                    </span>
                </div>
            </div>

            {/* Nút hành động chính (Call to Action) */}
            <ButtonStyled
                color={hasShortageError ? "default" : "primary"}
                className={`w-full font-bold h-14 shadow-lg text-md transition-transform ${
                    !hasShortageError && !isLoading ? "active:scale-[0.98]" : ""
                }`}
                onPress={onSubmit}
                isLoading={isLoading}
                isDisabled={hasShortageError}
            >
                {hasShortageError ? "Vui lòng điều chỉnh giỏ hàng" : `Mua hàng (${selectedCount})`}
            </ButtonStyled>
        </div>
    )
}
