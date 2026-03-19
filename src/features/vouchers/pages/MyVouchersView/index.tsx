"use client"

import { useState } from "react"
import { useGetMyVouchers } from "@/features/vouchers/hooks/useVoucher"
import { ChevronLeft, Star, Ticket } from "lucide-react"
import Link from "next/link"
import { motion } from "framer-motion"
import { Voucher } from "@/features/vouchers/models/response"
import { Button, Spinner } from "@heroui/react"
import { VoucherCard, VoucherViewModal } from "@/features/vouchers/components"
import { useGetMe, useGetMyLoyalty } from "@/features/profile/hooks"
import { formatCurrency } from "@/utils/helpers/currency"

export default function MyVouchersView() {
    const { data: myVouchersData, isLoading } = useGetMyVouchers()
    const myVouchers = myVouchersData || []

    const { data: user } = useGetMe()
    const { data: loyalty } = useGetMyLoyalty(user?.email)

    const [viewingVoucher, setViewingVoucher] = useState<Voucher | undefined>(undefined)

    return (
        <div className="min-h-screen bg-gray-50 pb-20 rounded-2xl overflow-hidden">
            {/* Header */}
            <div className="bg-linear-to-r from-amber-600 to-primary pb-24 pt-8 px-4 md:px-8 relative shadow-sm">
                {/* MỞ RỘNG CONTAINER HEADER */}
                <div className="max-w-6xl mx-auto relative z-10">
                    <Link
                        href="/profile"
                        className="inline-flex items-center gap-2 text-white/80 mb-6 hover:text-white transition-colors"
                    >
                        <ChevronLeft size={20} /> Quay lại Hồ sơ
                    </Link>
                    <div className="flex justify-between items-center gap-6">
                        <div>
                            <h1 className="text-3xl font-black text-white mb-2 tracking-tight">
                                Túi Quà Của Tôi
                            </h1>
                            <p className="text-white/90 font-medium">
                                Bạn đang sở hữu {myVouchers.length} ưu đãi chờ được sử dụng.
                            </p>
                        </div>
                        <div className="bg-white/20 backdrop-blur-md rounded-2xl p-4 border border-white/30 flex items-center gap-4 min-w-50 shadow-inner">
                            <div className="p-3 bg-white/20 rounded-xl shadow-sm">
                                <Star className="text-amber-300 fill-amber-300" size={28} />
                            </div>
                            <div className="flex flex-col">
                                <span className="text-white/80 text-[10px] uppercase font-black tracking-widest">
                                    Điểm tích lũy
                                </span>
                                <span className="text-2xl font-black text-white leading-none mt-1">
                                    {formatCurrency(loyalty?.availablePoints)}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content Body */}
            <div className="max-w-6xl mx-auto px-4 md:px-8 -mt-12 relative z-20">
                <div className="bg-white rounded-3xl shadow-xl overflow-hidden min-h-100 border border-default-100">
                    <div className="px-6 py-5 border-b border-default-100 bg-default-50/50 flex items-center justify-between">
                        <h2 className="font-bold text-default-800 flex items-center gap-2">
                            <Ticket size={20} className="text-primary" /> Voucher hiện có
                        </h2>
                        <Button
                            as={Link}
                            href="/vouchers"
                            size="sm"
                            variant="flat"
                            color="warning"
                            className="font-bold"
                        >
                            Săn thêm quà
                        </Button>
                    </div>
                    <div className="p-6 md:p-8">
                        <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}>
                            {isLoading ? (
                                <div className="flex flex-col items-center justify-center py-20 gap-4">
                                    <Spinner color="warning" size="lg" />
                                    <p className="text-default-500 font-medium">
                                        Đang mở túi quà...
                                    </p>
                                </div>
                            ) : myVouchers.length > 0 ? (
                                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                                    {myVouchers.map((v: Voucher) => (
                                        <VoucherCard
                                            key={v.id}
                                            voucher={v}
                                            isCustomer={true}
                                            isOwned={true}
                                            myPoints={999999}
                                            isRedeeming={false}
                                            onView={() => setViewingVoucher(v)}
                                            onRedeem={() => {}}
                                        />
                                    ))}
                                </div>
                            ) : (
                                <div className="text-center py-20">
                                    <div className="w-24 h-24 bg-default-100 rounded-full flex items-center justify-center mx-auto mb-5">
                                        <Ticket className="text-default-400" size={40} />
                                    </div>
                                    <p className="text-default-600 font-bold text-lg mb-2">
                                        Túi quà đang trống
                                    </p>
                                    <p className="text-default-400 mb-6 text-sm max-w-sm mx-auto">
                                        Bạn chưa thu thập mã giảm giá nào. Hãy ghé thăm Chợ Voucher
                                        để dùng điểm đổi lấy những ưu đãi hấp dẫn nhé!
                                    </p>
                                    <Button
                                        as={Link}
                                        href="/vouchers"
                                        color="primary"
                                        className="font-bold shadow-md px-8"
                                    >
                                        Khám phá ngay
                                    </Button>
                                </div>
                            )}
                        </motion.div>
                    </div>
                </div>
            </div>

            {/* Modal Chi tiết Voucher */}
            <VoucherViewModal
                isOpen={!!viewingVoucher}
                onOpenChange={(isOpen) => !isOpen && setViewingVoucher(undefined)}
                voucher={viewingVoucher}
                customAction={
                    <Button
                        as={Link}
                        href="/menu"
                        color="primary"
                        className="font-bold shadow-md px-8"
                    >
                        Dùng ngay
                    </Button>
                }
            />
        </div>
    )
}
