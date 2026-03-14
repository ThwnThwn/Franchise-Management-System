import { Voucher } from "@/features/vouchers/models/response"
import { VoucherType } from "@/features/vouchers/constants"
import { Gift, Truck, Tag, Circle, CheckCircle2 } from "lucide-react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

interface Props {
    voucher: Voucher
    isSelected: boolean
    isEligible: boolean
    reason?: string
    onSelect: (v: Voucher) => void
}

export function VoucherSelectionItem({ voucher, isSelected, isEligible, reason, onSelect }: Props) {
    // Phân loại style theo type
    const isShipping =
        voucher.type === VoucherType.SHIPPING_FIXED_AMOUNT ||
        voucher.type === VoucherType.SHIPPING_PERCENTAGE
    const isGift = voucher.type === VoucherType.GIFT

    return (
        <div
            onClick={() => isEligible && onSelect(voucher)}
            className={`flex border-2 rounded-xl overflow-hidden transition-all ${
                isSelected
                    ? "border-warning bg-warning-50/50" // Màu Espresso/Warning khi được chọn
                    : isEligible
                      ? "border-default-200 hover:border-warning/50 cursor-pointer bg-white"
                      : "border-default-100 opacity-60 cursor-not-allowed bg-default-50" // Disabled state
            }`}
        >
            {/* CỘT TRÁI - ICON & LOẠI */}
            <div
                className={`w-28 flex flex-col items-center justify-center border-r-2 border-dashed ${
                    isSelected
                        ? "bg-warning-100/50 border-warning-200 text-warning-600"
                        : isEligible
                          ? "bg-default-100 border-default-200 text-default-500"
                          : "bg-default-50 border-default-100 text-default-400"
                }`}
            >
                {isGift ? <Gift size={32} /> : isShipping ? <Truck size={32} /> : <Tag size={32} />}
                <span className="text-[10px] font-bold uppercase mt-2 text-center px-2">
                    {isGift ? "Quà Tặng" : isShipping ? "Vận Chuyển" : "Giảm Giá"}
                </span>
            </div>

            {/* CỘT PHẢI - THÔNG TIN & TICK CHỌN */}
            <div className="flex-1 p-3 flex flex-row items-center relative">
                <div className="flex-1 pr-4">
                    <h4 className="font-bold text-md text-foreground mb-1 leading-tight line-clamp-1">
                        {voucher.code}
                    </h4>

                    {/* Hiển thị mức giảm */}
                    {voucher.discountValue && !isGift && (
                        <p className="text-sm font-black text-warning-600 mb-1">
                            Giảm{" "}
                            {voucher.type === VoucherType.SHIPPING_PERCENTAGE ||
                            voucher.type === VoucherType.DISCOUNT_PERCENTAGE
                                ? `${voucher.discountValue}%`
                                : formatCurrencyWithSymbol(voucher.discountValue)}
                        </p>
                    )}

                    <p className="text-[11px] text-default-500 line-clamp-1">
                        {voucher.minOrderValue
                            ? `Đơn tối thiểu ${formatCurrencyWithSymbol(voucher.minOrderValue)}`
                            : "Mọi đơn hàng"}
                        {voucher.maxDiscountValue &&
                            ` • Giảm tối đa ${formatCurrencyWithSymbol(voucher.maxDiscountValue)}`}
                    </p>

                    {/* Báo lỗi đỏ nếu không đủ điều kiện */}
                    {!isEligible && reason && (
                        <div className="mt-1 text-[11px] font-medium text-danger">{reason}</div>
                    )}
                </div>

                {/* Vùng check (Nút Radio/Tick) */}
                <div className="pr-2 pl-2">
                    {isSelected ? (
                        <CheckCircle2 className="text-warning-600" size={24} />
                    ) : (
                        <Circle
                            className={isEligible ? "text-default-300" : "text-default-200"}
                            size={24}
                        />
                    )}
                </div>
            </div>
        </div>
    )
}
