import React from "react"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherItemType, VoucherType } from "@/features/vouchers"
import { ScrollShadow, Divider, Chip } from "@heroui/react"
import { Calendar, Info, PackageCheck, Ticket, Gift } from "lucide-react"
import { formatCurrency, formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { useDay } from "@/hooks"
import { DATE_TIME_VIEW_FORMAT } from "@/constants/constants"
import dayjs from "dayjs"

interface Props {
    voucher?: Voucher
}

export function VoucherDetailPanel({ voucher }: Props) {
    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })

    if (!voucher) {
        return (
            <div className="h-full flex flex-col items-center justify-center text-default-300 bg-default-50/30 rounded-2xl border-2 border-dashed border-default-200 p-6">
                <Ticket size={48} className="mb-4 opacity-50" />
                <p className="text-center font-medium">
                    Chọn một voucher ở danh sách bên phải để xem chi tiết
                </p>
            </div>
        )
    }

    // Lọc riêng danh sách Quà Tặng và Yêu Cầu
    const gifts = voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_GIFT) || []
    const requiredItems =
        voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_REQUIRED) || []

    return (
        <div className="h-full flex flex-col bg-warning-50/30 rounded-2xl border border-warning-200 overflow-hidden">
            <div className="bg-warning-100 p-4 border-b border-warning-200">
                <h3 className="font-black text-xl text-warning-800 uppercase tracking-tight">
                    {voucher.code}
                </h3>
            </div>

            <ScrollShadow className="p-5 flex-1 space-y-5">
                {/* Thông tin giảm giá / Quà Tặng */}
                <div>
                    <span className="text-[10px] font-bold text-warning-600 uppercase tracking-widest flex items-center gap-1.5 mb-1.5">
                        <Info size={14} /> Mức Ưu Đãi
                    </span>

                    {voucher.type === VoucherType.GIFT ? (
                        gifts.length > 0 ? (
                            <div className="flex flex-col gap-2 mt-1">
                                <span className="font-bold text-sm text-default-700">
                                    Tặng kèm:
                                </span>
                                <div className="flex flex-wrap gap-2">
                                    {gifts.map((g) => (
                                        <Chip
                                            key={g.id}
                                            variant="flat"
                                            color="warning"
                                            startContent={<Gift size={14} className="ml-1" />}
                                        >
                                            <span className="font-bold">{g.sku}</span>
                                            <span className="opacity-70 ml-1">×{g.quantity}</span>
                                        </Chip>
                                    ))}
                                </div>
                            </div>
                        ) : (
                            <p className="text-lg font-black text-default-800">Quà Tặng Đính Kèm</p>
                        )
                    ) : (
                        <>
                            <p className="text-lg font-black text-default-800">
                                Giảm{" "}
                                {voucher.type === VoucherType.SHIPPING_PERCENTAGE ||
                                voucher.type === VoucherType.DISCOUNT_PERCENTAGE
                                    ? `${formatCurrency(voucher.discountValue)}%`
                                    : formatCurrency(voucher.discountValue)}
                            </p>
                            {voucher.maxDiscountValue && (
                                <p className="text-sm font-medium text-default-500 mt-1">
                                    Tối đa: {formatCurrency(voucher.maxDiscountValue)}
                                </p>
                            )}
                        </>
                    )}
                </div>

                <Divider className="bg-warning-200/50" />

                {/* Điều kiện */}
                <div>
                    <span className="text-[10px] font-bold text-warning-600 uppercase tracking-widest flex items-center gap-1.5 mb-2">
                        <PackageCheck size={14} /> Điều kiện áp dụng
                    </span>
                    <ul className="list-disc list-inside text-sm font-medium text-default-700 space-y-2">
                        <li>
                            Đơn tối thiểu:{" "}
                            {voucher.minOrderValue
                                ? formatCurrencyWithSymbol(voucher.minOrderValue)
                                : "0đ"}
                        </li>

                        {/* Render danh sách Sản phẩm Yêu cầu (Nếu có) */}
                        {requiredItems.length > 0 && (
                            <li className="flex flex-col gap-1.5 mt-1">
                                <span>Phải có trong giỏ hàng:</span>
                                <div className="flex flex-wrap gap-1.5 ml-4">
                                    {requiredItems.map((r) => (
                                        <Chip key={r.id} size="sm" variant="faded" color="default">
                                            <span className="font-bold">{r.sku}</span>
                                            <span className="opacity-70 ml-1">×{r.quantity}</span>
                                        </Chip>
                                    ))}
                                </div>
                            </li>
                        )}
                    </ul>
                </div>

                <Divider className="bg-warning-200/50" />

                {/* Hạn sử dụng */}
                <div>
                    <span className="text-[10px] font-bold text-warning-600 uppercase tracking-widest flex items-center gap-1.5 mb-1">
                        <Calendar size={14} /> Hạn sử dụng
                    </span>
                    <p className="text-sm font-medium text-default-700">
                        {voucher.endDate
                            ? formatDateTime({ date: dayjs(voucher.endDate).add(-1, "minute") })
                            : "Không giới hạn"}
                    </p>
                </div>
            </ScrollShadow>
        </div>
    )
}
