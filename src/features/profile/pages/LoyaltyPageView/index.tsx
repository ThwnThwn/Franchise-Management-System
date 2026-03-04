"use client"

import { useGetMembershipTiers, useGetMyLoyalty, useGetMyPointLogs, useGetMe } from "@/features/profile/hooks/useProfile"
import { motion } from "framer-motion"
import { Coffee, ChevronLeft, CalendarClock, History, Award, CheckCircle2 } from "lucide-react"
import Link from "next/link"
import React, { useState } from "react"
// import { format } from "date-fns"
// import { vi } from "date-fns/locale"

export default function LoyaltyPageView() {
    const { data: user } = useGetMe()
    const { data: loyalty } = useGetMyLoyalty(user?.email)
    const { data: tiers } = useGetMembershipTiers()

    // Default fetch page 1 size 20
    const { data: pointLogRes } = useGetMyPointLogs({ pageIndex: 1, pageSize: 20 })
    const pointLogs = pointLogRes?.items || []

    const [activeTab, setActiveTab] = useState<"benefits" | "history">("benefits")

    // lấy png
    const getTierImage = (tierName: string) => {
        const lower = tierName.toLowerCase()
        if (lower.includes("bạc") || lower.includes("silver")) return "/images/tiers/silver.png"
        if (lower.includes("vàng") || lower.includes("gold")) return "/images/tiers/gold.png"
        if (lower.includes("kim cương") || lower.includes("platinum") || lower.includes("diamond")) return "/images/tiers/platinum.png"
        return "/images/tiers/bronze.png"
    }

    return (
        <div className="min-h-screen bg-gray-50/50 pb-20">
            {/* Header / Hero */}
            <div className="bg-gradient-to-br from-amber-700 to-amber-900 pt-8 pb-32 px-4 md:px-8 relative overflow-hidden">
                <div className="absolute top-0 right-0 w-96 h-96 bg-white/10 rounded-full blur-3xl transform translate-x-1/2 -translate-y-1/2"></div>

                <div className="max-w-4xl mx-auto relative z-10">
                    <Link href="/profile" className="inline-flex items-center gap-2 text-amber-100 mb-6 hover:text-white transition-colors">
                        <ChevronLeft size={20} /> Quay lại Hồ sơ
                    </Link>

                    <h1 className="text-3xl font-bold text-white mb-2">Thẻ Thành Viên</h1>
                    <p className="text-amber-100/80 mb-8">Tích điểm đổi quà & tận hưởng đặc quyền cực chất</p>

                    {/* Member Card */}
                    <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-3xl p-6 md:p-8 shadow-2xl flex flex-col md:flex-row items-center gap-8 relative overflow-hidden">
                        <div className="absolute inset-0 bg-gradient-to-r from-amber-500/10 to-transparent"></div>

                        <div className="relative z-10 w-32 h-32 md:w-40 md:h-40 shrink-0">
                            {loyalty?.currentTier?.name ? (
                                <img
                                    src={getTierImage(loyalty.currentTier.name)}
                                    className="w-full h-full object-contain drop-shadow-2xl"
                                    alt="Tier"
                                />
                            ) : (
                                <div className="w-full h-full bg-white/20 rounded-full flex items-center justify-center">
                                    <Coffee size={48} className="text-amber-100" />
                                </div>
                            )}
                        </div>

                        <div className="relative z-10 flex-1 text-center md:text-left">
                            <h2 className="text-sm font-semibold text-amber-200 tracking-wider uppercase mb-1">
                                {loyalty?.currentTier?.name || "Khách Hàng Thân Thiết"}
                            </h2>
                            <p className="text-2xl md:text-3xl font-bold text-white mb-6 tracking-tight">
                                {user?.firstName} {user?.lastName}
                            </p>

                            <div className="grid grid-cols-2 gap-4 text-left">
                                <div className="bg-black/20 rounded-2xl p-4 border border-white/10">
                                    <p className="text-amber-100/70 text-xs mb-1">ĐIỂM HIỆN CÓ</p>
                                    <p className="text-2xl font-bold text-white">{loyalty?.availablePoints || 0}</p>
                                </div>
                                <div className="bg-black/20 rounded-2xl p-4 border border-white/10">
                                    <p className="text-amber-100/70 text-xs mb-1">TỔNG CHI TIÊU</p>
                                    <p className="text-2xl font-bold text-white">{Number(loyalty?.totalSpent || 0).toLocaleString()}đ</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content Tabs */}
            <div className="max-w-4xl mx-auto px-4 md:px-8 -mt-20 relative z-20">
                <div className="bg-white rounded-3xl shadow-xl overflow-hidden min-h-[500px]">
                    <div className="flex border-b border-gray-100">
                        <button
                            className={`flex-1 py-5 text-center font-semibold transition-colors flex items-center justify-center gap-2 ${activeTab === 'benefits' ? 'text-amber-700 bg-amber-50/50' : 'text-gray-500 hover:bg-gray-50'}`}
                            onClick={() => setActiveTab('benefits')}
                        >
                            <Award size={18} /> Đặc Quyền Hạng Thẻ
                        </button>
                        <button
                            className={`flex-1 py-5 text-center font-semibold transition-colors flex items-center justify-center gap-2 ${activeTab === 'history' ? 'text-amber-700 bg-amber-50/50' : 'text-gray-500 hover:bg-gray-50'}`}
                            onClick={() => setActiveTab('history')}
                        >
                            <History size={18} /> Lịch Sử Tích & Mua
                        </button>
                    </div>

                    <div className="p-6 md:p-8">
                        {activeTab === 'benefits' && (
                            <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="space-y-8">
                                <div className="bg-blue-50 text-blue-800 p-4 rounded-2xl flex gap-3 text-sm">
                                    <CheckCircle2 className="shrink-0 mt-0.5 text-blue-600" size={18} />
                                    <p>Tích luỹ điểm linh hoạt: Tỉ lệ tích điểm phụ thuộc vào hạng thành viên. Điểm có thể dùng để thanh toán hóa đơn.</p>
                                </div>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    {(tiers || []).map((tier: any) => {
                                        const isCurrent = loyalty?.currentTier?.id === tier.id;
                                        return (
                                            <div key={tier.id} className={`p-6 rounded-3xl border-2 transition-all ${isCurrent ? 'border-amber-400 bg-amber-50/30' : 'border-gray-100 hover:border-amber-200'}`}>
                                                <div className="flex items-center gap-4 mb-4">
                                                    <img src={getTierImage(tier.name)} alt={tier.name} className="w-14 h-14 object-contain drop-shadow-md" />
                                                    <div>
                                                        <h3 className="font-bold text-gray-900 text-lg flex items-center gap-2">
                                                            {tier.name}
                                                            {isCurrent && <span className="text-[10px] bg-amber-500 text-white px-2 py-0.5 rounded-full uppercase tracking-wider">Hiện tại</span>}
                                                        </h3>
                                                        <p className="text-sm text-gray-500">Mức chi: {Number(tier.minSpendRequired || 0).toLocaleString()}đ</p>
                                                    </div>
                                                </div>

                                                <div className="space-y-3 mt-6">
                                                    <div className="flex justify-between items-center bg-gray-50 p-3 rounded-xl">
                                                        <span className="text-sm text-gray-600">Giảm giá hóa đơn</span>
                                                        <span className="font-bold text-gray-900">{tier.discountRate}%</span>
                                                    </div>
                                                    <div className="flex justify-between items-center bg-gray-50 p-3 rounded-xl">
                                                        <span className="text-sm text-gray-600">Tích điểm</span>
                                                        <span className="font-bold text-gray-900">{Number(tier.amountPerPoint || 0).toLocaleString()}đ = 1đ</span>
                                                    </div>
                                                    <div className="flex justify-between items-center bg-gray-50 p-3 rounded-xl">
                                                        <span className="text-sm text-gray-600">Giá trị điểm</span>
                                                        <span className="font-bold text-gray-900">1đ = {Number(tier.valuePerPoint || 0).toLocaleString()}đ</span>
                                                    </div>
                                                </div>
                                            </div>
                                        )
                                    })}
                                </div>
                            </motion.div>
                        )}

                        {activeTab === 'history' && (
                            <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}>
                                <div className="space-y-4">
                                    {pointLogs.length > 0 ? (
                                        pointLogs.map((log: any) => {
                                            const isAdd = log.actionType?.toLowerCase() !== 'redeem' && log.actionType?.toLowerCase() !== 'deduct' && Number(log.amount) > 0;
                                            return (
                                                <div key={log.id} className="flex items-center justify-between p-4 bg-white border border-gray-100 hover:border-amber-100 hover:shadow-md transition-all rounded-2xl">
                                                    <div className="flex items-center gap-4">
                                                        <div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 ${isAdd ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'}`}>
                                                            {isAdd ? <History size={20} /> : <Coffee size={20} />}
                                                        </div>
                                                        <div>
                                                            <p className="font-semibold text-gray-900 mb-1">{log.description || (isAdd ? "Tích điểm mua hàng" : "Dùng điểm thanh toán")}</p>
                                                            {/* <div className="flex items-center gap-2 text-xs text-gray-500">
                                                                <CalendarClock size={12} />
                                                                {format(new Date(log.createdAt), "dd/MM/yyyy HH:mm", { locale: vi })}
                                                            </div> */}
                                                        </div>
                                                    </div>
                                                    <div className="text-right">
                                                        <p className={`font-bold text-lg ${isAdd ? 'text-green-600' : 'text-red-500'}`}>
                                                            {isAdd ? '+' : '-'}{Math.abs(Number(log.amount))}
                                                        </p>
                                                    </div>
                                                </div>
                                            )
                                        })
                                    ) : (
                                        <div className="text-center py-16">
                                            <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-4">
                                                <History className="text-gray-300" size={32} />
                                            </div>
                                            <p className="text-gray-500 font-medium">Chưa có lịch sử giao dịch điểm</p>
                                        </div>
                                    )}
                                </div>
                            </motion.div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    )
}
