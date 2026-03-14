import { Button, Chip, Divider } from "@heroui/react"
import { ReceiptText, Ticket, Coins, Tag } from "lucide-react"
import { Order, OrderSaleItem } from "@/features/orders/models/response"
import { Voucher } from "@/features/vouchers/models/response"

import {
    formatCurrency,
    formatCurrencyWithSymbol,
    normalizeCurrency
} from "@/utils/helpers/currency"
import { InputStyled } from "@/components"
import {
    OrderSaleItemType,
    OrderSaleItemTypeLabels,
    OrderSaleItemTypeColors,
    OrderSaleItemTypeIcons
} from "@/features/orders/constants"
import { User } from "@/models/user"

interface Props {
    order: Order
    discounts: {
        productDiscount: number
        shippingDiscount: number
        totalDiscount: number
        tierDiscount: number
    }
    tierName?: string
    discountRate?: number
    totalAfterTax: number
    calculatedTax: number
    pointValueDeduction: number
    isPending: boolean
    isCustomer?: boolean
    user?: User

    appliedPoint: number
    setAppliedPoint: (val: number) => void

    appliedShippingVoucher?: Voucher
    appliedDiscountVoucher?: Voucher
    onOpenVoucherSelector?: () => void
}

export const OrderPaymentSummary = ({
    order,
    discounts,
    calculatedTax,
    tierName,
    discountRate,
    totalAfterTax,
    isPending,
    isCustomer,
    user,
    appliedPoint,
    setAppliedPoint,
    pointValueDeduction,
    appliedShippingVoucher,
    appliedDiscountVoucher,
    onOpenVoucherSelector
}: Props) => {
    const { productDiscount, shippingDiscount, tierDiscount } = discounts
    const loyalty = user?.loyalty
    const actualPointBalance = loyalty?.availablePoints ?? 0

    // LOGIC LỌC "SẠCH" DATA TỪ BACKEND
    const orderSkus = order.items?.map((i) => i.sku.toUpperCase()) || []

    const preTaxSaleItems =
        order.saleItems?.filter((s) => {
            // 1. Loại bỏ Ví (Wallet) vì đã hiện ở Footer (sau thuế)
            if (s.type === OrderSaleItemType.WALLET) return false

            // 2. Loại bỏ Voucher tặng món (đã hiện ở danh sách sản phẩm)
            if (s.type === OrderSaleItemType.VOUCHER) {
                const isProductGift = orderSkus.some((sku) => s.name.toUpperCase().includes(sku))
                if (isProductGift) return false
            }

            // 3. Giữ lại PROMOTION và các loại giảm giá khác
            return true
        }) || []

    const backEndWalletDeductions =
        order.saleItems?.filter((s) => s.type === OrderSaleItemType.WALLET) || []

    const handlePointChange = (val: string) =>
        setAppliedPoint(Math.max(0, Math.floor(normalizeCurrency(val) || 0)))

    return (
        <div className="bg-white rounded-3xl border border-default-200 shadow-md p-6">
            <h3 className="font-black flex items-center gap-2 border-b border-default-100 pb-3 mb-4 text-default-800 uppercase text-xs tracking-widest">
                <ReceiptText size={18} className="text-primary" /> Chi tiết hóa đơn
            </h3>

            <div className="space-y-6">
                {/* --- PHẦN 1: TƯƠNG TÁC (CHỈ HIỆN KHI PENDING) --- */}
                {isCustomer && isPending && (
                    <div className="space-y-4">
                        {onOpenVoucherSelector && (
                            <Button
                                fullWidth
                                variant="flat"
                                color="warning"
                                className="font-bold border border-warning-200 h-12 shadow-sm"
                                onPress={onOpenVoucherSelector}
                                startContent={<Ticket size={18} />}
                            >
                                {appliedShippingVoucher || appliedDiscountVoucher
                                    ? "Đổi mã Voucher"
                                    : "Chọn Voucher / Quà tặng"}
                            </Button>
                        )}
                        {user && (
                            <div className="p-4 bg-default-50 rounded-2xl border border-default-100 shadow-inner">
                                <InputStyled
                                    type="number"
                                    label="Đổi điểm tích lũy"
                                    placeholder="0"
                                    value={appliedPoint > 0 ? appliedPoint.toString() : ""}
                                    onValueChange={handlePointChange}
                                    startContent={<Coins size={16} className="text-warning-500" />}
                                    description={`Bạn có: ${formatCurrency(actualPointBalance)} pts`}
                                />
                            </div>
                        )}
                    </div>
                )}

                {/* --- PHẦN 2: BẢNG TÍNH TIỀN --- */}
                <div className="pt-2 space-y-3 mb-0">
                    {/* Tạm tính gốc */}
                    <div className="flex justify-between text-sm font-medium">
                        <span className="text-default-500">Tạm tính (Chưa thuế)</span>
                        <span className="text-default-800 font-bold">
                            {formatCurrencyWithSymbol(order.subtotal)}
                        </span>
                    </div>

                    {/* --- CÁC KHOẢN GIẢM TRỪ TRƯỚC THUẾ --- */}
                    <div className="space-y-2">
                        {/* 1. Ưu đãi Tier (Real-time) */}
                        {isPending && (discountRate ?? 0) > 0 && (
                            <div className="flex justify-between items-center text-sm font-medium text-primary-600 bg-primary-50/50 px-3 py-2 rounded-xl border border-primary-100">
                                <div className="flex items-center gap-2">
                                    <Tag size={14} />
                                    <span>
                                        Ưu đãi {tierName} ({Math.round(discountRate! * 100)}%)
                                    </span>
                                </div>
                                <span className="font-bold">
                                    -{formatCurrencyWithSymbol(tierDiscount)}
                                </span>
                            </div>
                        )}

                        {/* 2. Voucher & Points (Real-time) */}
                        {isPending && productDiscount > 0 && (
                            <Row
                                label={`Giảm giá SP (${appliedDiscountVoucher?.code})`}
                                value={productDiscount}
                                color="text-success-600"
                            />
                        )}
                        {isPending && shippingDiscount > 0 && (
                            <Row
                                label={`Hỗ trợ ship (${appliedShippingVoucher?.code})`}
                                value={shippingDiscount}
                                color="text-success-600"
                            />
                        )}
                        {isPending && appliedPoint > 0 && (
                            <Row
                                label={`Quy đổi ${appliedPoint} điểm`}
                                value={pointValueDeduction}
                                color="text-warning-600"
                            />
                        )}

                        {/* 3. Khuyến mãi & Voucher từ Backend (Đơn đã chốt) */}
                        {!isPending &&
                            preTaxSaleItems.map((sale, idx) => {
                                const Icon = OrderSaleItemTypeIcons[sale.type]
                                return (
                                    <div
                                        key={idx}
                                        className="flex justify-between items-center px-1"
                                    >
                                        <div className="flex items-center gap-2">
                                            <Chip
                                                size="sm"
                                                variant="flat"
                                                color={OrderSaleItemTypeColors[sale.type]}
                                                startContent={<Icon size={12} />}
                                            >
                                                {OrderSaleItemTypeLabels[sale.type]}
                                            </Chip>
                                            <span className="text-[11px] font-bold text-default-600">
                                                {sale.name}
                                            </span>
                                        </div>
                                        <span className="text-sm font-bold text-success-600">
                                            -{formatCurrencyWithSymbol(sale.amount)}
                                        </span>
                                    </div>
                                )
                            })}
                    </div>

                    <Divider className="my-2 opacity-50" />

                    {/* Thuế VAT */}
                    <div className="flex justify-between text-sm font-medium">
                        <span className="text-default-500 italic font-normal">Thuế VAT (10%)</span>
                        <span className="text-default-800">
                            {formatCurrencyWithSymbol(isPending ? calculatedTax : order.tax)}
                        </span>
                    </div>
                    {backEndWalletDeductions.length > 0 && (
                        <div className="space-y-2 border-t border-dashed border-default-100">
                            {backEndWalletDeductions.map((sale, idx) => (
                                <SaleRow key={`wallet-${idx}`} sale={sale} />
                            ))}
                        </div>
                    )}
                </div>

                <Divider className="my-4 border-default-300" />

                {/* --- PHẦN 3: TỔNG HÓA ĐƠN --- */}

                <div className="flex justify-between items-center">
                    <div>
                        <span className="font-black uppercase tracking-tighter text-default-400 text-xs">
                            Tổng hóa đơn
                        </span>
                        <p className="text-[10px] text-default-400 italic">
                            Đã bao gồm thuế & giảm giá
                        </p>
                    </div>
                    <div className="flex flex-col items-end">
                        {(discounts.totalDiscount > 0 || appliedPoint > 0) && (
                            <span className="text-xs text-default-400 line-through font-medium">
                                {formatCurrencyWithSymbol(order.subtotal + order.subtotal * 0.1)}
                            </span>
                        )}
                        <span className="text-3xl font-black text-primary drop-shadow-sm tracking-tight">
                            {formatCurrencyWithSymbol(isPending ? totalAfterTax : order.paidAmount)}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    )
}

const SaleRow = ({ sale }: { sale: OrderSaleItem }) => {
    const Icon = OrderSaleItemTypeIcons[sale.type]
    return (
        <div className="flex justify-between items-center px-1">
            <div className="flex items-center gap-2">
                <Chip
                    size="sm"
                    variant="flat"
                    color={OrderSaleItemTypeColors[sale.type]}
                    startContent={<Icon size={12} />}
                >
                    {OrderSaleItemTypeLabels[sale.type]}
                </Chip>
                <span className="text-[11px] font-bold text-default-600">{sale.name}</span>
            </div>
            <span className="text-sm font-bold text-success-600">
                -{formatCurrencyWithSymbol(sale.amount)}
            </span>
        </div>
    )
}

// Component phụ cho các dòng trừ tiền đơn giản
const Row = ({ label, value, color }: { label: string; value: number; color: string }) => (
    <div className={`flex justify-between text-[11px] font-bold ${color} px-1`}>
        <span>{label}</span>
        <span>-{formatCurrencyWithSymbol(value)}</span>
    </div>
)
