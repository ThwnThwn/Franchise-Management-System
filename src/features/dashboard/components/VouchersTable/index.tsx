import { Table, TableHeader, TableColumn, TableBody, TableRow, TableCell, Chip, Switch, Tooltip, Button } from "@heroui/react"
import { Voucher, VoucherType, VoucherItemType } from "@/features/orders/models/voucher"
import { Edit, Trash2, Gift, PackageCheck } from "lucide-react"
import { VariantNameMap } from "../../utils/exportVouchers"

interface VouchersTableProps {
    vouchers: Voucher[]
    isLoading: boolean
    onEdit: (voucher: Voucher) => void
    onToggleStatus: (id: string, is_active: boolean) => void
    onDelete: (id: string) => void
    onAssign: (voucher: Voucher) => void
    variantMap?: VariantNameMap
}

export const VouchersTable = ({ vouchers, isLoading, onEdit, onToggleStatus, onDelete, onAssign, variantMap = {} }: VouchersTableProps) => {

    const renderType = (type: VoucherType) => {
        switch (type) {
            case VoucherType.SHIPPING_FIXED_AMOUNT: return <Chip color="primary" variant="flat" size="sm">Freeship (Tiền)</Chip>
            case VoucherType.SHIPPING_PERCENTAGE: return <Chip color="primary" variant="flat" size="sm">Freeship (%)</Chip>
            case VoucherType.DISCOUNT_FIXED_AMOUNT: return <Chip color="success" variant="flat" size="sm">Ưu đãi (Tiền)</Chip>
            case VoucherType.DISCOUNT_PERCENTAGE: return <Chip color="success" variant="flat" size="sm">Ưu đãi (%)</Chip>
            case VoucherType.GIFT: return <Chip color="secondary" variant="flat" size="sm">Quà tặng</Chip>
            default: return <Chip size="sm">{type}</Chip>
        }
    }

    const renderDiscountValue = (voucher: Voucher) => {
        if (voucher.type === VoucherType.GIFT) return "Theo Quà tặng"
        const suffix = (voucher.type === VoucherType.SHIPPING_PERCENTAGE || voucher.type === VoucherType.DISCOUNT_PERCENTAGE) ? "%" : "đ"
        return `${Number(voucher.discount_value).toLocaleString()}${suffix}`
    }

    const renderItems = (voucher: Voucher, type: VoucherItemType) => {
        const items = voucher.voucher_items?.filter((i) => i.type === type) ?? []
        if (items.length === 0) return <span className="text-default-300 text-xs">—</span>
        return (
            <div className="flex flex-col gap-1">
                {items.map((item, idx) => {
                    const name = variantMap[item.sku] ?? item.sku
                    return (
                        <Chip
                            key={idx}
                            size="sm"
                            variant="flat"
                            color={type === VoucherItemType.ITEM_GIFT ? "secondary" : "warning"}
                            startContent={type === VoucherItemType.ITEM_GIFT ? <Gift size={12} /> : <PackageCheck size={12} />}
                        >
                            {name} ×{item.quantity}
                        </Chip>
                    )
                })}
            </div>
        )
    }

    return (
        <Table aria-label="Danh sách Voucher" shadow="none" removeWrapper>
            <TableHeader>
                <TableColumn>MÃ CODE</TableColumn>
                <TableColumn>LOẠI</TableColumn>
                <TableColumn>MỨC GIẢM</TableColumn>
                <TableColumn>ĐIỀU KIỆN (MIN ORDER)</TableColumn>
                <TableColumn>SỐ LƯỢNG</TableColumn>
                <TableColumn>QUÀ TẶNG</TableColumn>
                <TableColumn>ITEM YÊU CẦU</TableColumn>
                <TableColumn>TRẠNG THÁI</TableColumn>
                <TableColumn align="center">HÀNH ĐỘNG</TableColumn>
            </TableHeader>
            <TableBody
                items={vouchers}
                isLoading={isLoading}
                emptyContent={isLoading ? "Đang tải..." : "Không tìm thấy voucher nào"}
            >
                {(voucher) => (
                    <TableRow key={voucher.id}>
                        <TableCell>
                            <span className="font-bold text-lg text-primary">{voucher.code}</span>
                        </TableCell>
                        <TableCell>{renderType(voucher.type)}</TableCell>
                        <TableCell>
                            <div>
                                <span className="font-medium text-slate-800">{renderDiscountValue(voucher)}</span>
                                {voucher.max_discount_value && (
                                    <div className="text-xs text-default-400">Tối đa {Number(voucher.max_discount_value).toLocaleString()}đ</div>
                                )}
                            </div>
                        </TableCell>
                        <TableCell>
                            {voucher.min_order_value ? `${Number(voucher.min_order_value).toLocaleString()}đ` : 'Mọi đơn'}
                        </TableCell>
                        <TableCell>
                            {voucher.current_quantity !== undefined ? voucher.current_quantity : voucher.init_quantity} / {voucher.init_quantity || '∞'}
                        </TableCell>
                        <TableCell>
                            {renderItems(voucher, VoucherItemType.ITEM_GIFT)}
                        </TableCell>
                        <TableCell>
                            {renderItems(voucher, VoucherItemType.ITEM_REQUIRED)}
                        </TableCell>
                        <TableCell>
                            <Switch
                                isSelected={voucher.is_active}
                                size="sm"
                                color="success"
                                onValueChange={(val) => onToggleStatus(voucher.id, !val)} // !val because param is current state
                            />
                        </TableCell>
                        <TableCell>
                            <div className="flex justify-center gap-2">
                                <Tooltip content="Gán Voucher" color="secondary">
                                    <Button isIconOnly size="sm" variant="light" color="secondary" onPress={() => onAssign(voucher)}>
                                        <Gift size={16} />
                                    </Button>
                                </Tooltip>
                                <Tooltip content="Chỉnh sửa">
                                    <Button isIconOnly size="sm" variant="light" onPress={() => onEdit(voucher)}>
                                        <Edit size={16} className="text-default-500" />
                                    </Button>
                                </Tooltip>
                                <Tooltip content="Xóa" color="danger">
                                    <Button isIconOnly size="sm" variant="light" color="danger" onPress={() => onDelete(voucher.id)}>
                                        <Trash2 size={16} />
                                    </Button>
                                </Tooltip>
                            </div>
                        </TableCell>
                    </TableRow>
                )}
            </TableBody>
        </Table>
    )
}

