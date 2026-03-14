"use client"

import React from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Chip,
    Card,
    CardBody
} from "@heroui/react"
import {
    Tag,
    Calendar,
    Package,
    Clock,
    ShieldCheck,
    ShieldAlert,
    Gift,
    PackageCheck,
    Coins,
    Users
} from "lucide-react"

import { Voucher } from "@/features/vouchers/models/response"
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
import { ModalStyled } from "@/components"
import { useDay } from "@/hooks"
import { DATE_TIME_VIEW_FORMAT } from "@/constants/constants"
import dayjs from "dayjs"

interface Props {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    voucher: Voucher | undefined
    customAction?: React.ReactNode
}

export const VoucherViewModal = ({ isOpen, onOpenChange, voucher, customAction }: Props) => {
    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })

    if (!voucher) return null

    const renderDiscountValue = () => {
        if (voucher.type === VoucherType.GIFT) return "Theo Quà tặng"
        const isPercent = [
            VoucherType.SHIPPING_PERCENTAGE,
            VoucherType.DISCOUNT_PERCENTAGE
        ].includes(voucher.type)
        return `${formatCurrency(voucher.discountValue)} ${isPercent ? "%" : "đ"}`
    }

    const gifts = voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_GIFT) || []
    const requires =
        voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_REQUIRED) || []

    // --- Sub-component cho các dòng Data ---
    const DataRow = ({
        label,
        value,
        icon: Icon
    }: {
        label: string
        value: React.ReactNode
        icon?: any
    }) => (
        <div className="flex flex-col gap-1">
            <span className="text-[10px] font-bold uppercase tracking-widest text-default-400 flex items-center gap-1.5">
                {Icon && <Icon size={12} />} {label}
            </span>
            <span className="text-sm font-semibold text-default-900">{value || "—"}</span>
        </div>
    )

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="3xl"
            isDismissable={true}
            isKeyboardDismissDisabled={false}
        >
            <ModalContent>
                {(onClose) => (
                    <>
                        {/* --- HEADER: TRẠNG THÁI & ĐỊNH DANH --- */}
                        <ModalHeader className="flex items-start gap-4 border-b border-default-100 py-5 px-8 bg-white">
                            <div
                                className={`p-3 rounded-2xl shadow-sm ${voucher.isActive ? "bg-success-100 text-success-600" : "bg-default-100 text-default-500"}`}
                            >
                                {voucher.isActive ? (
                                    <ShieldCheck size={28} />
                                ) : (
                                    <ShieldAlert size={28} />
                                )}
                            </div>
                            <div className="flex flex-col gap-1.5 pt-0.5">
                                <div className="flex items-center gap-3">
                                    <h2 className="text-2xl font-black text-default-900 leading-none uppercase tracking-tight">
                                        {voucher.code}
                                    </h2>
                                    <Chip
                                        size="sm"
                                        variant="flat"
                                        color={voucher.isActive ? "success" : "default"}
                                        className="font-bold"
                                    >
                                        {voucher.isActive ? "Đang phát hành" : "Tạm ngưng"}
                                    </Chip>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Chip
                                        size="sm"
                                        variant="flat"
                                        className={`${VoucherTypeStyles[voucher.type]} border border-default-200`}
                                    >
                                        {VoucherTypeLabels[voucher.type]}
                                    </Chip>
                                    <span className="text-xs font-medium text-default-400 flex items-center gap-1">
                                        <Clock size={12} /> Tạo lúc:{" "}
                                        {formatDateTime({ date: voucher.createdAt })}
                                    </span>
                                </div>
                            </div>
                        </ModalHeader>

                        {/* --- BODY: CHI TIẾT DỮ LIỆU --- */}
                        <ModalBody className="py-6 px-8 bg-[#FCFAFA]">
                            <div className="grid grid-cols-12 gap-6">
                                {/* CARD 1: GIÁ TRỊ & ĐIỀU KIỆN (Cột trái 7) */}
                                <Card
                                    shadow="sm"
                                    className="col-span-12 lg:col-span-7 border border-default-200 bg-white"
                                >
                                    <CardBody className="p-5">
                                        <h3 className="text-xs font-black uppercase text-primary-700 tracking-wider mb-4 border-b border-default-100 pb-2">
                                            Thông số Khuyến mãi
                                        </h3>
                                        <div className="grid grid-cols-2 gap-y-6 gap-x-4">
                                            <DataRow
                                                icon={Tag}
                                                label="Mức giảm"
                                                value={
                                                    <span className="text-lg font-black text-warning-600">
                                                        {renderDiscountValue()}
                                                    </span>
                                                }
                                            />
                                            <DataRow
                                                icon={Package}
                                                label="Đơn tối thiểu"
                                                value={
                                                    voucher.minOrderValue
                                                        ? formatCurrencyWithSymbol(
                                                              voucher.minOrderValue
                                                          )
                                                        : "Mọi đơn hàng"
                                                }
                                            />
                                            <DataRow
                                                icon={Tag}
                                                label="Giảm tối đa"
                                                value={
                                                    voucher.maxDiscountValue
                                                        ? formatCurrencyWithSymbol(
                                                              voucher.maxDiscountValue
                                                          )
                                                        : "Không giới hạn"
                                                }
                                            />
                                            <DataRow
                                                icon={Coins}
                                                label="Điểm cần đổi"
                                                value={
                                                    voucher.pointCost
                                                        ? `${normalizeCurrency(voucher.pointCost).toLocaleString()} điểm`
                                                        : "Miễn phí"
                                                }
                                            />
                                        </div>
                                    </CardBody>
                                </Card>

                                {/* CARD 2: KHO & THỜI GIAN (Cột phải 5) */}
                                <Card
                                    shadow="sm"
                                    className="col-span-12 lg:col-span-5 border border-default-200 bg-white"
                                >
                                    <CardBody className="p-5">
                                        <h3 className="text-xs font-black uppercase text-primary-700 tracking-wider mb-4 border-b border-default-100 pb-2">
                                            Giới hạn sử dụng
                                        </h3>
                                        <div className="flex flex-col gap-5">
                                            <div>
                                                <span className="text-[10px] font-bold uppercase tracking-widest text-default-400 flex items-center gap-1.5 mb-1">
                                                    <Package size={12} /> Tồn kho / Tổng phát hành
                                                </span>
                                                <div className="flex items-baseline gap-1">
                                                    <span className="text-2xl font-black text-default-900 leading-none">
                                                        {voucher.currentQuantity ??
                                                            voucher.initQuantity}
                                                    </span>
                                                    <span className="text-sm font-bold text-default-400">
                                                        / {voucher.initQuantity || "∞"}
                                                    </span>
                                                </div>
                                            </div>
                                            <DataRow
                                                icon={Users}
                                                label="Lượt dùng / Khách"
                                                value={`${voucher.maxUsagePerUser || 1} lần`}
                                            />
                                            <DataRow
                                                icon={Calendar}
                                                label="Hạn sử dụng"
                                                value={
                                                    <span
                                                        className={
                                                            voucher.endDate &&
                                                            new Date(voucher.endDate) < new Date()
                                                                ? "text-danger font-bold"
                                                                : ""
                                                        }
                                                    >
                                                        {voucher.endDate
                                                            ? formatDateTime({
                                                                  date: dayjs(voucher.endDate).add(
                                                                      -1,
                                                                      "minute"
                                                                  )
                                                              })
                                                            : "Không thời hạn"}
                                                    </span>
                                                }
                                            />
                                        </div>
                                    </CardBody>
                                </Card>

                                {/* CARD 3: DANH SÁCH QUÀ TẶNG (Trải dài 12 cột) */}
                                {(gifts.length > 0 || requires.length > 0) && (
                                    <div className="col-span-12 mt-2">
                                        <h3 className="text-sm font-black uppercase tracking-wider mb-3">
                                            Sản phẩm đính kèm
                                        </h3>
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                            {/* Cột SP Yêu cầu */}
                                            <Card
                                                shadow="none"
                                                className="border-2 border-dashed border-default-200 bg-default-50"
                                            >
                                                <CardBody className="p-4">
                                                    <span className="text-xs font-bold text-default-500 uppercase mb-3 flex items-center gap-2">
                                                        <PackageCheck size={14} /> Điều kiện cần mua
                                                    </span>
                                                    <div className="flex flex-wrap gap-2">
                                                        {requires.length > 0 ? (
                                                            requires.map((r) => (
                                                                <Chip
                                                                    key={r.id}
                                                                    variant="flat"
                                                                    className={
                                                                        VoucherItemTypeStyles[
                                                                            r.type
                                                                        ]
                                                                    }
                                                                >
                                                                    {r.sku}{" "}
                                                                    <span className="opacity-60 ml-1">
                                                                        ×{r.quantity}
                                                                    </span>
                                                                </Chip>
                                                            ))
                                                        ) : (
                                                            <span className="text-xs italic text-default-400">
                                                                Không có
                                                            </span>
                                                        )}
                                                    </div>
                                                </CardBody>
                                            </Card>

                                            {/* Cột Quà tặng */}
                                            <Card
                                                shadow="none"
                                                className="border-2 border-dashed border-warning-200 bg-warning-50/30"
                                            >
                                                <CardBody className="p-4">
                                                    <span className="text-xs font-bold text-warning-600 uppercase mb-3 flex items-center gap-2">
                                                        <Gift size={14} /> Quà tặng kèm
                                                    </span>
                                                    <div className="flex flex-wrap gap-2">
                                                        {gifts.length > 0 ? (
                                                            gifts.map((g) => (
                                                                <Chip
                                                                    key={g.id}
                                                                    variant="flat"
                                                                    className={
                                                                        VoucherItemTypeStyles[
                                                                            g.type
                                                                        ]
                                                                    }
                                                                >
                                                                    {g.sku}{" "}
                                                                    <span className="opacity-60 ml-1">
                                                                        ×{g.quantity}
                                                                    </span>
                                                                </Chip>
                                                            ))
                                                        ) : (
                                                            <span className="text-xs italic text-warning-400">
                                                                Không có
                                                            </span>
                                                        )}
                                                    </div>
                                                </CardBody>
                                            </Card>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </ModalBody>

                        {/* --- FOOTER --- */}
                        <ModalFooter className="bg-white border-t border-default-100 py-4 px-8 flex justify-between">
                            <p className="text-xs text-default-400 italic">
                                Cập nhật lần cuối: {formatDateTime({ date: voucher.updatedAt })}
                            </p>
                            <div className="flex gap-2">
                                <Button variant="light" className="font-bold" onPress={onClose}>
                                    Đóng
                                </Button>
                                {customAction} {/* Đẩy nguyên cục Button Redeem vào đây */}
                            </div>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
