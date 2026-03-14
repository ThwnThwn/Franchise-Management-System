import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Switch
} from "@heroui/react"
import { Voucher } from "@/features/vouchers/models/response"
import { Gift, PackageCheck } from "lucide-react"
import {
    formatCurrency,
    formatCurrencyWithSymbol,
    normalizeCurrency
} from "@/utils/helpers/currency"
import {
    VoucherItemType,
    VoucherItemTypeStyles,
    VoucherType,
    VoucherTypeLabels,
    VoucherTypeStyles
} from "@/features/vouchers/constants"

interface VouchersTableProps {
    vouchers: Voucher[]
    isLoading: boolean
    onToggleStatus: (id: string, isActive: boolean) => void
    onRowAction: (voucher: Voucher) => void
}

export const VouchersTable = ({
    vouchers,
    isLoading,
    onToggleStatus,
    onRowAction
}: VouchersTableProps) => {
    const renderDiscountValue = (voucher: Voucher) => {
        if (voucher.type === VoucherType.GIFT)
            return <span className="text-default-400 italic text-sm">Theo Quà tặng</span>

        const isPercent = [
            VoucherType.SHIPPING_PERCENTAGE,
            VoucherType.DISCOUNT_PERCENTAGE
        ].includes(voucher.type)
        const displayValue = isPercent
            ? `${formatCurrency(voucher.discountValue)}%`
            : formatCurrencyWithSymbol(voucher.discountValue)

        return (
            <div className="flex flex-col">
                <span className="font-black text-slate-800 text-base">{displayValue}</span>
                {voucher.maxDiscountValue && (
                    <span className="text-[11px] text-default-500 font-medium">
                        Tối đa {formatCurrencyWithSymbol(voucher.maxDiscountValue)}
                    </span>
                )}
            </div>
        )
    }

    const renderItems = (voucher: Voucher, type: VoucherItemType) => {
        const items = voucher.voucherItems?.filter((i) => i.type === type) ?? []
        if (items.length === 0) return <span className="text-default-300 text-xs italic">—</span>

        return (
            <div className="flex flex-col gap-1.5">
                {items.map((item, idx) => (
                    <Chip
                        key={idx}
                        size="sm"
                        variant="flat"
                        className={VoucherItemTypeStyles[type]}
                        startContent={
                            type === VoucherItemType.ITEM_GIFT ? (
                                <Gift size={12} />
                            ) : (
                                <PackageCheck size={12} />
                            )
                        }
                    >
                        <span className="font-medium">{item.sku}</span>
                        <span className="opacity-70 ml-1">×{item.quantity}</span>
                    </Chip>
                ))}
            </div>
        )
    }

    return (
        <Table
            aria-label="Danh sách Voucher"
            shadow="none"
            classNames={{
                wrapper: "p-0 border-none",
                th: "bg-default-50 text-default-600 font-bold uppercase tracking-wider text-[10px]",
                td: "py-3 border-b border-default-100 last:border-none align-top"
            }}
        >
            <TableHeader>
                <TableColumn width="15%">VOUCHER</TableColumn>
                <TableColumn width="15%">MỨC GIẢM</TableColumn>
                <TableColumn width="15%">ĐIỀU KIỆN (MIN)</TableColumn>
                <TableColumn width="15%">SP YÊU CẦU</TableColumn>
                <TableColumn width="15%">QUÀ TẶNG KÈM</TableColumn>
                <TableColumn width="10%">KHO</TableColumn>
                <TableColumn width="15%" align="center">
                    TRẠNG THÁI
                </TableColumn>
            </TableHeader>

            <TableBody
                items={vouchers}
                isLoading={isLoading}
                emptyContent={
                    isLoading ? "Đang pha chế dữ liệu..." : "Chưa có voucher nào trên quầy."
                }
            >
                {(voucher) => (
                    <TableRow
                        key={voucher.id}
                        onClick={() => onRowAction(voucher)}
                        className="hover:bg-default-100 hover:cursor-pointer transition-colors"
                    >
                        {/* Cột 1: Chỉ còn Mã Code + Loại */}
                        <TableCell>
                            <div className="flex flex-col gap-1.5 items-start mt-1">
                                <span className="font-black text-base text-primary-700 tracking-tight uppercase leading-none">
                                    {voucher.code}
                                </span>
                                <Chip
                                    size="sm"
                                    variant="flat"
                                    className={`${VoucherTypeStyles[voucher.type]} border`}
                                >
                                    {VoucherTypeLabels[voucher.type]}
                                </Chip>
                            </div>
                        </TableCell>

                        <TableCell>{renderDiscountValue(voucher)}</TableCell>

                        <TableCell>
                            <span className="font-medium text-default-700 mt-1 block">
                                {normalizeCurrency(voucher.minOrderValue)
                                    ? formatCurrencyWithSymbol(voucher.minOrderValue)
                                    : "Mọi đơn hàng"}
                            </span>
                        </TableCell>

                        <TableCell>{renderItems(voucher, VoucherItemType.ITEM_REQUIRED)}</TableCell>
                        <TableCell>{renderItems(voucher, VoucherItemType.ITEM_GIFT)}</TableCell>

                        <TableCell>
                            <div className="flex flex-col mt-0.5">
                                <span className="font-black text-default-800 text-lg leading-none">
                                    {voucher.currentQuantity ?? voucher.initQuantity}
                                </span>
                                <span className="text-[10px] font-bold text-default-400 uppercase tracking-widest mt-1">
                                    / {voucher.initQuantity || "∞"} TỔNG
                                </span>
                            </div>
                        </TableCell>

                        {/* Cột Trạng thái */}
                        <TableCell>
                            <div className="flex justify-center mt-1">
                                <Switch
                                    isSelected={voucher.isActive}
                                    size="sm"
                                    color="warning"
                                    onValueChange={(val) => onToggleStatus(voucher.id, val)}
                                    classNames={{ wrapper: "shadow-sm" }}
                                />
                            </div>
                        </TableCell>
                    </TableRow>
                )}
            </TableBody>
        </Table>
    )
}
