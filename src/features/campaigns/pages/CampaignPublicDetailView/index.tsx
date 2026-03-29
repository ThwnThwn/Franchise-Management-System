"use client"

import { Button, Card, CardBody, Chip, Progress } from "@heroui/react"
import { ArrowLeft, CalendarClock, Sparkles, TicketPercent } from "lucide-react"
import { useRouter } from "next/navigation"
import { useState } from "react"

import { RoleSlug } from "@/constants/enums"
import { useRedeemCampaignVoucher } from "@/features/campaigns/hooks/useCampaigns"
import { CampaignResponse } from "@/features/campaigns/models/response"
import { useGetMe, useGetMyLoyalty } from "@/features/profile/hooks/useProfile"
import { VoucherCard, VoucherViewModal } from "@/features/vouchers/components"
import { Voucher } from "@/features/vouchers/models/response"
import { useCountdown, useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { formatCurrency, normalizeCurrency } from "@/utils/helpers/currency"
import { CountdownUnit } from "@/features/campaigns/components"
import { useTimeProgress } from "@/features/campaigns/hooks"

interface Props {
    campaign: CampaignResponse
}

export const CampaignPublicDetailView = ({ campaign }: Props) => {
    const router = useRouter()
    const { days, hours, minutes, seconds, isExpired } = useCountdown(campaign.endDate)

    // --- Hooks xác thực & Điểm ---
    const isLogined = useTokenStore((s) => !!s.accessToken)
    const { onOpen: onLoginOpen } = useLoginDiscloresureSingleton()
    const { data: user } = useGetMe()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const { data: loyalty } = useGetMyLoyalty(user?.email)

    // Ép kiểu an toàn cho số điểm
    const myPoints = normalizeCurrency(loyalty?.availablePoints || 0)

    // --- State & Mutations ---
    const [viewingVoucher, setViewingVoucher] = useState<Voucher | undefined>(undefined)
    const [redeemingId, setRedeemingId] = useState<string | null>(null) // Để biết đang quay loading ở nút nào
    const progressValue = useTimeProgress(campaign.startDate, campaign.endDate)

    // Gọi hook của campaign
    const { mutate: redeem, isPending: isRedeeming } = useRedeemCampaignVoucher()

    // Derived Data: Trạng thái sự kiện
    const isStarted = new Date(campaign.startDate ?? "") <= new Date()
    const isActive = isStarted && !isExpired

    // Hàm xử lý khi bấm nút "Đổi mã"
    const handleRedeem = (voucher: Voucher) => {
        if (!isLogined) {
            onLoginOpen()
            return
        }

        setRedeemingId(voucher.id)
        redeem(
            { campaignId: campaign.id, voucherId: voucher.id },
            {
                onSuccess: () => setViewingVoucher(undefined),
                onSettled: () => setRedeemingId(null)
            }
        )
    }

    return (
        <div className="min-h-screen bg-default-50/50 pb-20 rounded-2xl overflow-hidden">
            {/* --- HEADER --- */}
            <div className="bg-white border-b border-default-100 shadow-sm sticky top-0 z-50">
                <div className="container mx-auto px-6 h-20 flex items-center justify-between">
                    <div className="flex items-center gap-4">
                        <Button
                            isIconOnly
                            variant="light"
                            radius="full"
                            onPress={() => router.back()}
                        >
                            <ArrowLeft size={20} className="text-default-500" />
                        </Button>
                        <h1 className="text-xl md:text-2xl font-black text-default-900 tracking-tighter">
                            Chi tiết Sự kiện
                        </h1>
                    </div>
                    {!isActive && !isExpired && (
                        <Chip
                            color="warning"
                            variant="flat"
                            startContent={<CalendarClock size={14} />}
                        >
                            Sắp diễn ra
                        </Chip>
                    )}
                </div>
            </div>

            <div className="container mx-auto px-6 mt-10 grid grid-cols-1 lg:grid-cols-12 gap-10">
                {/* --- CỘT TRÁI (Thông tin chính) --- */}
                <div className="lg:col-span-8 space-y-10">
                    {/* Campaign Hero Card */}
                    <Card
                        className="rounded-3xl shadow-2xl border border-warning-200"
                        isPressable={false}
                    >
                        <CardBody className="p-0 grid grid-cols-1 md:grid-cols-12 overflow-hidden bg-linear-to-br from-amber-400 via-warning-500 to-amber-600">
                            <div className="md:col-span-4 p-8 flex items-center justify-center bg-black/5">
                                <Sparkles size={120} className="text-white opacity-20 rotate-12" />
                            </div>
                            <div className="md:col-span-8 p-10 flex flex-col justify-center text-white space-y-3">
                                <h2 className="text-4xl md:text-5xl font-black uppercase tracking-tighter leading-none">
                                    {campaign.name}
                                </h2>
                                <p className="text-base font-medium text-amber-100 max-w-[90%] line-clamp-2">
                                    {campaign.description ||
                                        "Quét deal hời, húp trọn voucher ngay!"}
                                </p>
                            </div>
                        </CardBody>
                    </Card>
                </div>

                {/* --- CỘT PHẢI (Countdown Stick) --- */}
                <div className="lg:col-span-4 sticky top-28 h-fit">
                    <Card className="rounded-3xl shadow-xl border border-default-100 p-8 bg-white">
                        <CardBody className="p-0 space-y-6">
                            <h4 className="text-sm font-black uppercase text-default-500 tracking-wider">
                                Kết thúc sau:
                            </h4>

                            <div className="flex justify-between items-baseline text-default-900">
                                <CountdownUnit
                                    value={days}
                                    label="Ngày"
                                    valueClassName="text-3xl"
                                    labelClassName="text-default-400"
                                />
                                <span className="text-xl font-black mb-4 opacity-50">:</span>
                                <CountdownUnit
                                    value={hours}
                                    label="Giờ"
                                    valueClassName="text-3xl"
                                    labelClassName="text-default-400"
                                />
                                <span className="text-xl font-black mb-4 opacity-50">:</span>
                                <CountdownUnit
                                    value={minutes}
                                    label="Phút"
                                    valueClassName="text-3xl"
                                    labelClassName="text-default-400"
                                />
                                <span className="text-xl font-black mb-4 opacity-50 animate-pulse">
                                    :
                                </span>
                                <CountdownUnit
                                    value={seconds}
                                    label="Giây"
                                    isSeconds
                                    valueClassName="text-3xl text-danger-500"
                                    labelClassName="text-default-400"
                                />
                            </div>

                            <Progress
                                size="sm"
                                radius="full"
                                value={progressValue}
                                color="warning"
                            />
                        </CardBody>
                    </Card>
                </div>
            </div>

            {/* Vouchers section */}
            <div className="mx-auto px-6 mt-10 space-y-6">
                <div className="flex items-center justify-between">
                    <div className="flex items-baseline gap-3">
                        <TicketPercent size={28} className="text-primary-500" />
                        <h3 className="text-2xl font-black uppercase tracking-tight text-default-900">
                            Số lượng voucher: ({campaign.vouchers?.length || 0})
                        </h3>
                    </div>
                    {isCustomer && (
                        <div className="text-sm font-bold bg-warning-100 text-warning-700 px-4 py-2 rounded-full border border-warning-200">
                            Điểm của bạn: {formatCurrency(myPoints)}
                        </div>
                    )}
                </div>

                {/* Danh sách Voucher */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {(campaign.vouchers || []).length === 0 ? (
                        <div className="col-span-2 text-center py-16 bg-white rounded-3xl border border-default-100 text-default-500 font-bold">
                            Sự kiện này hiện chưa có hoặc đã hết voucher!
                        </div>
                    ) : (
                        campaign.vouchers?.map((v) => (
                            <VoucherCard
                                key={v.id}
                                isCustomer={isCustomer}
                                voucher={v}
                                myPoints={myPoints}
                                isRedeeming={isRedeeming && redeemingId === v.id}
                                onView={() => setViewingVoucher(v)}
                                onRedeem={() => handleRedeem(v)}
                            />
                        ))
                    )}
                </div>
            </div>

            <VoucherViewModal
                isOpen={!!viewingVoucher}
                onOpenChange={(isOpen) => !isOpen && setViewingVoucher(undefined)}
                voucher={viewingVoucher}
                customAction={
                    <Button
                        color="warning"
                        isDisabled={
                            myPoints < normalizeCurrency(viewingVoucher?.pointCost || 0) ||
                            !isActive ||
                            (viewingVoucher?.currentQuantity || 0) <= 0
                        }
                        isLoading={isRedeeming && redeemingId === viewingVoucher?.id}
                        onPress={() => handleRedeem(viewingVoucher!)}
                    >
                        {normalizeCurrency(viewingVoucher?.pointCost || 0) > 0
                            ? `Đổi ${formatCurrency(viewingVoucher?.pointCost || 0)} Điểm`
                            : "Nhận Miễn Phí"}
                    </Button>
                }
            />
        </div>
    )
}
