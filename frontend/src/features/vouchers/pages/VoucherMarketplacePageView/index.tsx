"use client"

import { RoleSlug } from "@/constants/enums"
import CampaignSection from "@/features/campaigns/components/CampaignSection"
import { useGetMe, useGetMyLoyalty } from "@/features/profile/hooks/useProfile"
import { VoucherCard, VoucherViewModal } from "@/features/vouchers/components"
import { useGetAllVouchers, useRedeemVoucher } from "@/features/vouchers/hooks/useVoucher"
import { Voucher } from "@/features/vouchers/models/response"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { formatCurrency, normalizeCurrency } from "@/utils/helpers/currency"
import { Button } from "@heroui/react"
import dayjs from "dayjs"
import { useState } from "react"

export default function VoucherMarketplacePageView({
    initialVouchers
}: {
    initialVouchers: Voucher[]
}) {
    const now = dayjs()
    const isLogined = useTokenStore((s) => !!s.accessToken)
    const { onOpen: onLoginOpen } = useLoginDiscloresureSingleton()

    const { data: user } = useGetMe()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const { data: loyalty } = useGetMyLoyalty(user?.email)

    const { data: vouchersData } = useGetAllVouchers(initialVouchers)
    const vouchers = (vouchersData || []).filter((v) => v.isActive && now.isBefore(v.endDate))

    // An toàn chuyển đổi điểm
    const myPoints = normalizeCurrency(loyalty?.availablePoints || 0)

    const [viewingVoucher, setViewingVoucher] = useState<Voucher | undefined>(undefined)

    // Gọi Hook kèm hàm đóng Modal khi đổi thành công
    const { redeemWithConfirm, isPending: isRedeeming } = useRedeemVoucher({
        onSuccess: () => setViewingVoucher(undefined)
    })

    const handleRedeem = (voucher: Voucher) => {
        if (!isLogined) {
            onLoginOpen()
            return
        }

        redeemWithConfirm(voucher)
    }

    return (
        <div className="space-y-6 min-h-[80vh] w-7xl max-w-7xl mx-auto px-4 md:px-8">
            <CampaignSection />
            <div className="mb-8">
                <h1 className="text-3xl font-black text-[#4b3621]">Chợ Voucher Khách Hàng</h1>
                <p className="text-default-500">
                    Dùng điểm tích lũy để đổi lấy những phần quà hấp dẫn.
                </p>
            </div>

            {/* Thanh hiển thị điểm hiện tại */}
            <div className="bg-[#4b3621] text-white p-4 rounded-2xl flex items-center justify-between shadow-lg">
                <div className="flex flex-col">
                    <span className="text-xs uppercase tracking-widest font-bold text-[#f3e5ab]">
                        Điểm của bạn
                    </span>
                    <span className="text-3xl font-black">{formatCurrency(myPoints)}</span>
                </div>
            </div>

            {/* Grid Chợ Voucher (1 cột mobile, 2 cột Desktop) */}
            {vouchers.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-20 bg-default-50 rounded-2xl border-2 border-dashed border-default-200">
                    <p className="text-default-500 font-bold">
                        Hiện chưa có voucher nào trên quầy.
                    </p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {vouchers.map((voucher: Voucher) => (
                        <VoucherCard
                            key={voucher.id}
                            isCustomer={isCustomer}
                            voucher={voucher}
                            myPoints={myPoints}
                            isRedeeming={isRedeeming}
                            onView={() => setViewingVoucher(voucher)}
                            onRedeem={() => handleRedeem(voucher)}
                        />
                    ))}
                </div>
            )}

            {/* Modal Chi tiết Voucher - Re-use lại VoucherViewModal nhưng thay nút Edit thành Đổi */}
            <VoucherViewModal
                isOpen={!!viewingVoucher}
                onOpenChange={(isOpen) => !isOpen && setViewingVoucher(undefined)}
                voucher={viewingVoucher}
                customAction={
                    <Button
                        color="warning"
                        isDisabled={myPoints < normalizeCurrency(viewingVoucher?.pointCost || 0)}
                        onPress={() => handleRedeem(viewingVoucher!)}
                    >
                        Đổi {formatCurrency(viewingVoucher?.pointCost)} Điểm
                    </Button>
                }
            />
        </div>
    )
}
