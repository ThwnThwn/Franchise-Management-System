import { Card, CardBody, Button, Tooltip } from "@heroui/react"
import { Coins, Tag, Package, Gift } from "lucide-react"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherTypeLabels } from "@/features/vouchers/constants"
import {
    formatCurrency,
    formatCurrencyWithSymbol,
    normalizeCurrency
} from "@/utils/helpers/currency"
import Link from "next/link"

interface VoucherCardProps {
    isCustomer: boolean
    isOwned?: boolean
    voucher: Voucher
    myPoints: number
    onView: () => void
    onRedeem: () => void
    isRedeeming: boolean
}

export const VoucherCard = ({
    isCustomer,
    isOwned = false,
    voucher,
    myPoints,
    onView,
    onRedeem,
    isRedeeming
}: VoucherCardProps) => {
    // Ép kiểu an toàn
    const pointCost = normalizeCurrency(voucher.pointCost || 0)
    const isOutOfStock = (voucher.currentQuantity ?? 0) <= 0
    // Chỉ check điểm nếu đúng là Khách hàng (tránh báo lỗi không đủ điểm khi chưa đăng nhập)
    const isNotEnoughPoints = isCustomer && myPoints < pointCost

    const isButtonDisabled = !isCustomer || isOutOfStock || isNotEnoughPoints

    let tooltipContent = "Đổi ngay"
    if (!isCustomer) {
        tooltipContent = "Vui lòng đăng nhập tài khoản Khách hàng để đổi quà"
    } else if (isOutOfStock) {
        tooltipContent = "Rất tiếc, quà này đã hết lượt đổi"
    } else if (isNotEnoughPoints) {
        tooltipContent = "Bạn chưa tích đủ điểm"
    }

    // Render giá trị hiển thị to
    const displayValue =
        voucher.type === 4 // GIFT
            ? "Quà Tặng"
            : `${formatCurrency(voucher.discountValue)}${voucher.type % 2 === 0 ? "%" : "đ"}`

    return (
        <Card
            isPressable
            as="div"
            onPress={onView}
            className="border border-default-200 min-w-sm shadow-sm hover:border-warning-300 transition-all bg-white"
        >
            <CardBody className="p-0 flex flex-row items-stretch">
                {/* Khu vực Trái: Màu sắc và Giá trị */}
                <div className="w-1/3 bg-warning-50 border-r border-dashed border-warning-200 flex flex-col items-center justify-center p-4 text-center">
                    {voucher.type === 4 ? (
                        <Gift size={32} className="text-warning-600 mb-2" />
                    ) : (
                        <Tag size={32} className="text-warning-600 mb-2" />
                    )}
                    <span className="font-black text-xl text-warning-700 leading-tight">
                        {displayValue}
                    </span>
                    <span className="text-[10px] uppercase font-bold text-warning-500 mt-1 tracking-widest">
                        {VoucherTypeLabels[voucher.type]}
                    </span>
                </div>

                {/* Khu vực Phải: Thông tin & Nút bấm */}
                <div className="w-2/3 p-4 flex flex-col justify-between">
                    <div>
                        <h3 className="font-bold text-lg text-default-900 leading-none line-clamp-1">
                            {voucher.code}
                        </h3>
                        <p className="text-xs text-default-500 mt-2 flex items-center gap-1">
                            <Package size={12} /> Đơn tối thiểu:{" "}
                            <span className="font-semibold">
                                {formatCurrencyWithSymbol(voucher.minOrderValue)}
                            </span>
                        </p>
                        <p className="text-xs text-default-500 mt-0.5 flex items-center gap-1">
                            <Coins size={12} /> Số lượng còn:{" "}
                            <span className="font-bold text-default-700">
                                {voucher.currentQuantity}
                            </span>
                        </p>
                    </div>

                    <div className="mt-4 flex items-center justify-between">
                        {!isOwned && (
                            <span className="font-black text-lg text-primary-600 flex items-center gap-1.5">
                                {pointCost > 0 ? (
                                    <>
                                        {formatCurrency(pointCost)} <Coins size={16} />
                                    </>
                                ) : (
                                    "MIỄN PHÍ"
                                )}
                            </span>
                        )}
                        {isOwned ? (
                            <Button
                                as={Link}
                                href="/menu"
                                size="sm"
                                color="primary"
                                className="font-bold shadow-md ml-auto"
                            >
                                Dùng ngay
                            </Button>
                        ) : (
                            <Tooltip
                                content={tooltipContent}
                                color={isButtonDisabled ? "danger" : "default"}
                                isDisabled={!isButtonDisabled} // Trạng thái bình thường (đổi được) thì ẩn tooltip đi cho đỡ vướng
                                placement="top-end" // Đẩy tooltip lên trên góc để không che ngón tay mobile
                            >
                                <div className="inline-block">
                                    <Button
                                        size="sm"
                                        color="warning"
                                        className="font-bold shadow-md"
                                        isDisabled={isButtonDisabled}
                                        isLoading={isRedeeming}
                                        onPress={() => onRedeem()}
                                    >
                                        Đổi mã
                                    </Button>
                                </div>
                            </Tooltip>
                        )}
                    </div>
                </div>
            </CardBody>
        </Card>
    )
}
