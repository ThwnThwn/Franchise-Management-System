"use client"

import { useState, useMemo } from "react"
import {
    useGetAllVouchers,
    useGetMyVouchers,
    useRedeemVoucher
} from "@/features/vouchers/hooks/useVoucher"
import { useGetMe, useGetMyLoyalty } from "@/features/profile/hooks/useProfile"
import { useGetAllProducts } from "@/features/products/hooks/useProduct"
import { ChevronLeft, Ticket, Gift, Percent, Star, AlertCircle, PackageCheck } from "lucide-react"
import Link from "next/link"
import { motion } from "framer-motion"
import { Voucher } from "@/features/vouchers/models/response"
import { VoucherType, VoucherItemType } from "@/features/vouchers/constants"
import { Button, Chip, addToast } from "@heroui/react"
import { useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"

const VoucherCard = ({
    voucher,
    isOwned,
    userPoints,
    onRedeem,
    isRedeeming,
    variantMap = {}
}: {
    voucher: Voucher
    isOwned: boolean
    userPoints: number
    onRedeem?: (id: string, pointCost: number) => void
    isRedeeming?: boolean
    variantMap?: Record<string, string>
}) => {
    // Render Voucher Type Label
    const getVoucherInfo = (type: VoucherType) => {
        switch (type) {
            case VoucherType.SHIPPING_FIXED_AMOUNT:
                return {
                    icon: <Ticket size={24} />,
                    color: "bg-blue-100 text-blue-600",
                    label: "Freeship (Tiền)"
                }
            case VoucherType.SHIPPING_PERCENTAGE:
                return {
                    icon: <Percent size={24} />,
                    color: "bg-blue-100 text-blue-600",
                    label: "Freeship (%)"
                }
            case VoucherType.DISCOUNT_FIXED_AMOUNT:
                return {
                    icon: <Ticket size={24} />,
                    color: "bg-green-100 text-green-600",
                    label: "Giảm giá"
                }
            case VoucherType.DISCOUNT_PERCENTAGE:
                return {
                    icon: <Percent size={24} />,
                    color: "bg-green-100 text-green-600",
                    label: "Giảm giá (%)"
                }
            case VoucherType.GIFT:
                return {
                    icon: <Gift size={24} />,
                    color: "bg-purple-100 text-purple-600",
                    label: "Quà tặng"
                }
            default:
                return {
                    icon: <Ticket size={24} />,
                    color: "bg-gray-100 text-gray-600",
                    label: "Voucher"
                }
        }
    }

    const info = getVoucherInfo(voucher.type)

    const giftItems =
        voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_GIFT) ?? []
    const requiredItems =
        voucher.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_REQUIRED) ?? []

    const renderDiscount = () => {
        if (voucher.type === VoucherType.GIFT) {
            if (giftItems.length > 0) {
                return (
                    "Tặng: " +
                    giftItems
                        .map((i) => {
                            const name = variantMap[i.sku] ?? i.sku
                            return `${name} ×${i.quantity}`
                        })
                        .join(", ")
                )
            }
            return "Tặng quà đính kèm"
        }
        const suffix =
            voucher.type === VoucherType.SHIPPING_PERCENTAGE ||
            voucher.type === VoucherType.DISCOUNT_PERCENTAGE
                ? "%"
                : "đ"
        return `Giảm ${Number(voucher.discountValue).toLocaleString()}${suffix}`
    }

    const pointCost = Number(voucher.pointCost || 0)
    const canRedeem = userPoints >= pointCost
    const isOutOfStock =
        voucher.currentQuantity !== undefined ? voucher.currentQuantity <= 0 : false

    return (
        <div className="bg-white rounded-2xl border flex flex-row overflow-hidden hover:shadow-lg transition-all border-gray-100">
            {/* Left side pattern & icon */}
            <div
                className={`w-28 ${info.color} flex flex-col items-center justify-center border-r border-dashed border-gray-300 relative`}
            >
                <div className="absolute -top-3 -right-3 w-6 h-6 bg-white rounded-full"></div>
                <div className="absolute -bottom-3 -right-3 w-6 h-6 bg-white rounded-full"></div>
                {info.icon}
                <span className="text-xs font-semibold mt-2 px-2 text-center">{info.label}</span>
            </div>

            {/* Content */}
            <div className="flex-1 p-5 flex flex-col justify-between">
                <div>
                    <div className="flex justify-between items-start mb-2">
                        <h3 className="font-bold text-gray-900 text-lg">{voucher.code}</h3>
                        {!isOwned && pointCost > 0 && (
                            <Chip
                                color="warning"
                                variant="flat"
                                size="sm"
                                startContent={<Star size={14} className="ml-1" />}
                            >
                                {pointCost} Điểm
                            </Chip>
                        )}
                        {!isOwned && pointCost === 0 && (
                            <Chip color="success" variant="flat" size="sm">
                                Miễn phí
                            </Chip>
                        )}
                    </div>

                    <p className="font-semibold text-primary mb-1">{renderDiscount()}</p>

                    {voucher.minOrderValue && (
                        <p className="text-xs text-gray-500">
                            Đơn tối thiểu: {Number(voucher.minOrderValue).toLocaleString()}đ
                        </p>
                    )}
                    {voucher.maxDiscountValue && (
                        <p className="text-xs text-gray-500">
                            Giảm tối đa: {Number(voucher.maxDiscountValue).toLocaleString()}đ
                        </p>
                    )}
                    {requiredItems.length > 0 && (
                        <div className="mt-1 flex flex-wrap gap-1">
                            {requiredItems.map((item, idx) => (
                                <Chip
                                    key={idx}
                                    size="sm"
                                    variant="flat"
                                    color="warning"
                                    startContent={<PackageCheck size={12} />}
                                >
                                    Cần: {variantMap[item.sku] ?? item.sku} ×{item.quantity}
                                </Chip>
                            ))}
                        </div>
                    )}
                </div>

                {/* Footer Action */}
                <div className="mt-4 flex items-center justify-between">
                    <div className="text-xs text-gray-400">
                        {voucher.endDate
                            ? `HSD: ${new Date(voucher.endDate).toLocaleDateString()}`
                            : "Không giới hạn HSD"}
                    </div>

                    {!isOwned && (
                        <Button
                            color={canRedeem && !isOutOfStock ? "primary" : "default"}
                            variant={canRedeem && !isOutOfStock ? "solid" : "flat"}
                            isDisabled={!canRedeem || isOutOfStock}
                            isLoading={isRedeeming}
                            onPress={() => onRedeem?.(voucher.id, pointCost)}
                            size="sm"
                        >
                            {isOutOfStock ? "Hết lượt" : pointCost > 0 ? "Đổi mã" : "Nhận mã"}
                        </Button>
                    )}
                    {isOwned && (
                        <Button color="primary" variant="flat" size="sm" as={Link} href="/menu">
                            Dùng ngay
                        </Button>
                    )}
                </div>
            </div>
        </div>
    )
}

export default function MyVouchersView() {
    const { data: user } = useGetMe()
    const { data: loyalty } = useGetMyLoyalty(user?.email)

    // API calls for Vouchers
    const { data: allPublicVouchers = [], isLoading: isLoadingAll } = useGetAllVouchers()
    const {
        data: myVouchers = [],
        isLoading: isLoadingMy,
        refetch: refetchMyVouchers
    } = useGetMyVouchers()
    const queryClient = useQueryClient()
    const { mutate: redeemVoucher, isPending: isRedeeming } = useRedeemVoucher()

    // Fetch products to resolve variant names
    const { data: productsData } = useGetAllProducts({
        params: {},
        pagination: { page: 1, pageSize: 100 },
        enabled: true
    })

    const variantMap = useMemo<Record<string, string>>(() => {
        const map: Record<string, string> = {}
        productsData?.items?.forEach((p) => {
            p.variants.forEach((v) => {
                map[v.sku] = `${p.name}${v.attributes ? ` (${v.attributes})` : ""}`
            })
        })
        return map
    }, [productsData])

    const [activeTab, setActiveTab] = useState<"my_vouchers" | "exchange">("my_vouchers")
    const [redeemingId, setRedeemingId] = useState<string | null>(null)

    const userPoints = Number(loyalty?.availablePoints || 0)

    // Filter active vouchers that are available for exchange
    // Hides vouchers the user already has (if the backend doesn't filter them out)
    const availableToExchange = allPublicVouchers.filter((v) => {
        const usageCount = myVouchers.filter((mv) => mv.id === v.id).length
        const maxUsage = v.maxUsagePerUser ?? 1
        return v.isActive && usageCount < maxUsage
    })

    const handleRedeem = (voucherId: string, pointCost: number) => {
        setRedeemingId(voucherId)

        const onSuccess = () => {
            refetchMyVouchers()
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.LOYALTY_ACCOUNT] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.POINT_LOGS] })
            addToast({
                title: "Thành công",
                description: "Voucher đã được thêm vào kho của bạn",
                color: "success"
            })
            setRedeemingId(null)
        }

        const onError = () => {
            setRedeemingId(null)
            addToast({
                title: "Thất bại",
                description: "Không thể nhận voucher. Vui lòng kiểm tra lại điểm hoặc số lượng.",
                color: "danger"
            })
        }
        redeemVoucher(voucherId, { onSuccess, onError })
    }

    return (
        <div className="min-h-screen bg-gray-50/50 pb-20">
            {/* Header */}
            <div className="bg-linear-to-r from-amber-600 to-primary pb-24 pt-8 px-4 md:px-8 relative">
                <div className="max-w-4xl mx-auto relative z-10">
                    <Link
                        href="/profile"
                        className="inline-flex items-center gap-2 text-white/80 mb-6 hover:text-white transition-colors"
                    >
                        <ChevronLeft size={20} /> Quay lại Hồ sơ
                    </Link>

                    <div className="flex justify-between items-center">
                        <div>
                            <h1 className="text-3xl font-bold text-white mb-2">Kho Voucher</h1>
                            <p className="text-white/80">
                                Nhận thêm ưu đãi tiết kiệm cho mọi đơn hàng
                            </p>
                        </div>
                        <div className="bg-white/20 backdrop-blur-md rounded-2xl p-4 text-center border border-white/30 hidden md:block">
                            <p className="text-white/80 text-xs mb-1 uppercase tracking-wider">
                                Điểm của bạn
                            </p>
                            <p className="text-3xl font-bold text-white flex items-center justify-center gap-1">
                                {userPoints}{" "}
                                <Star size={20} className="text-amber-300 fill-amber-300" />
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content Tabs */}
            <div className="max-w-4xl mx-auto px-4 md:px-8 -mt-12 relative z-20">
                <div className="bg-white rounded-3xl shadow-xl overflow-hidden min-h-125">
                    <div className="flex border-b border-gray-100">
                        <button
                            className={`flex-1 py-5 text-center font-semibold transition-colors flex items-center justify-center gap-2 ${activeTab === "my_vouchers" ? "text-primary bg-primary-50/50 border-b-2 border-primary" : "text-gray-500 hover:bg-gray-50"}`}
                            onClick={() => setActiveTab("my_vouchers")}
                        >
                            <Ticket size={20} /> Voucher Của Tôi ({myVouchers.length})
                        </button>
                        <button
                            className={`flex-1 py-5 text-center font-semibold transition-colors flex items-center justify-center gap-2 ${activeTab === "exchange" ? "text-primary bg-primary-50/50 border-b-2 border-primary" : "text-gray-500 hover:bg-gray-50"}`}
                            onClick={() => setActiveTab("exchange")}
                        >
                            <Gift size={20} /> Đổi Voucher
                        </button>
                    </div>

                    <div className="p-6 md:p-8 bg-gray-50/30">
                        {activeTab === "my_vouchers" && (
                            <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                                {isLoadingMy ? (
                                    <div className="text-center py-20 text-gray-500">
                                        Đang tải Voucher của bạn...
                                    </div>
                                ) : myVouchers.length > 0 ? (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        {myVouchers.map((v) => (
                                            <VoucherCard
                                                key={v.id}
                                                voucher={v}
                                                isOwned={true}
                                                userPoints={userPoints}
                                                variantMap={variantMap}
                                            />
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center py-20">
                                        <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                            <Ticket className="text-gray-300" size={32} />
                                        </div>
                                        <p className="text-gray-500 font-medium mb-4">
                                            Bạn chưa có voucher nào
                                        </p>
                                        <Button
                                            color="primary"
                                            onPress={() => setActiveTab("exchange")}
                                        >
                                            Khám phá Voucher
                                        </Button>
                                    </div>
                                )}
                            </motion.div>
                        )}

                        {activeTab === "exchange" && (
                            <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                                {/* Mobile Point Display */}
                                <div className="md:hidden flex justify-between items-center bg-amber-50 rounded-xl p-4 mb-6 border border-amber-100">
                                    <span className="font-semibold text-amber-900">
                                        Điểm hiện có của bạn:
                                    </span>
                                    <span className="font-bold text-lg text-amber-600 flex items-center gap-1">
                                        {userPoints} <Star size={18} className="fill-amber-600" />
                                    </span>
                                </div>

                                {isLoadingAll ? (
                                    <div className="text-center py-20 text-gray-500">
                                        Đang tải danh sách ưu đãi...
                                    </div>
                                ) : availableToExchange.length > 0 ? (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        {availableToExchange.map((v) => (
                                            <VoucherCard
                                                key={v.id}
                                                voucher={v}
                                                isOwned={false}
                                                userPoints={userPoints}
                                                onRedeem={handleRedeem}
                                                isRedeeming={isRedeeming && redeemingId === v.id}
                                                variantMap={variantMap}
                                            />
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center py-20">
                                        <AlertCircle
                                            className="text-gray-300 mx-auto mb-4"
                                            size={48}
                                        />
                                        <p className="text-gray-500 font-medium">
                                            Hiện tại không có voucher nào mới để nhận/đổi.
                                        </p>
                                    </div>
                                )}
                            </motion.div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    )
}
