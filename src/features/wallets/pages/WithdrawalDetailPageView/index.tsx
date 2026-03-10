"use client"

import { Spinner } from "@heroui/react"
import { Receipt } from "lucide-react"

import { DashboardHeader } from "@/features/dashboard"
import { ButtonStyled, ImageUploaderModal } from "@/components"
import { useImageUploadModal } from "@/hooks"

import {
    useGetWithdrawalById,
    useApproveWithdrawal,
    useRejectWithdrawal,
    useCancelWithdrawal,
    useCompleteWithdrawal
} from "@/features/wallets/hooks"
import { WithdrawalStatus } from "@/features/wallets/constants"

import {
    OverviewCard,
    BankInfoCard,
    TimelineCard,
    AuditTrailCard,
    AlertCard,
    ReceiptCard
} from "@/features/wallets/components"
import { useState } from "react"

const UPLOAD_FORM_KEY = "image"

export default function WithdrawalDetailPageView({ id }: { id: string }) {
    const { imgSrc, setImgSrc, isOpen, onOpenChange, onClose } = useImageUploadModal()
    const [showRawAccount, setShowRawAccount] = useState(false)

    // Fetch data
    const { data: detail, isLoading } = useGetWithdrawalById(id)

    // Admin Hooks
    const { approveWithConfirm, isPending: isApproving } = useApproveWithdrawal()
    const { rejectWithConfirm, isPending: isRejecting } = useRejectWithdrawal()
    const { cancelWithConfirm, isPending: isCanceling } = useCancelWithdrawal()
    const { completeWithConfirm, isPending: isCompleting } = useCompleteWithdrawal({
        onSuccess: onClose
    })

    const handleUploadBill = async (formData: FormData) => {
        const file = formData.get(UPLOAD_FORM_KEY) as File

        await completeWithConfirm(id, file)
    }

    if (isLoading || !detail) {
        return (
            <div className="flex justify-center py-20 min-h-screen">
                <Spinner color="primary" size="lg" />
            </div>
        )
    }

    return (
        <div className="max-w-6xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-6">
            <div className="flex items-center gap-4 mb-4">
                <DashboardHeader
                    icon={<Receipt className="text-white" />}
                    title="Xử lý Lệnh Rút Tiền"
                    description={`Quản trị lệnh rút mã: ${detail.requestCode}`}
                />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
                {/* CỘT TRÁI (8/12) - Thông tin chính */}
                <div className="md:col-span-7 lg:col-span-8 space-y-6">
                    <OverviewCard detail={detail} />

                    <BankInfoCard
                        detail={detail}
                        showRaw={showRawAccount}
                        onToggleRaw={() => setShowRawAccount(!showRawAccount)}
                    />

                    <AlertCard detail={detail} />
                    <AuditTrailCard detail={detail} />
                </div>

                {/* CỘT PHẢI (4/12) - Timeline & Actions */}
                <div className="md:col-span-5 lg:col-span-4 space-y-6">
                    <TimelineCard detail={detail} />
                    <ReceiptCard detail={detail} />

                    {/* KHỐI ACTION BUTTONS CỦA ADMIN */}
                    <div className="flex flex-col gap-3 bg-white p-4 rounded-xl border border-default-200 shadow-sm sticky top-24">
                        <h4 className="font-bold text-sm mb-2 text-default-600 uppercase">
                            Thao tác nghiệp vụ
                        </h4>

                        {detail.status === WithdrawalStatus.Pending && (
                            <>
                                <ButtonStyled
                                    color="success"
                                    size="lg"
                                    onPress={() => approveWithConfirm(detail.id)}
                                    isLoading={isApproving}
                                    className="text-white font-bold w-full shadow-md"
                                >
                                    Duyệt lệnh
                                </ButtonStyled>
                                <ButtonStyled
                                    color="danger"
                                    variant="flat"
                                    onPress={() => rejectWithConfirm(detail.id)}
                                    isLoading={isRejecting}
                                    className="w-full font-bold"
                                >
                                    Từ chối
                                </ButtonStyled>
                            </>
                        )}

                        {(detail.status === WithdrawalStatus.Approved ||
                            detail.status === WithdrawalStatus.Processing) && (
                            <>
                                <ButtonStyled
                                    color="primary"
                                    size="lg"
                                    onPress={onOpenChange}
                                    className="font-bold w-full shadow-md"
                                >
                                    Hoàn tất (Up Bill)
                                </ButtonStyled>
                                <ButtonStyled
                                    color="danger"
                                    variant="flat"
                                    onPress={() => cancelWithConfirm(detail.id)}
                                    isLoading={isCanceling}
                                    className="w-full font-bold"
                                >
                                    Hủy lệnh
                                </ButtonStyled>
                            </>
                        )}

                        {/* Thông báo nếu đã đóng */}
                        {(detail.status === WithdrawalStatus.Completed ||
                            detail.status === WithdrawalStatus.Cancelled ||
                            detail.status === WithdrawalStatus.Rejected) && (
                            <p className="text-sm text-center italic text-default-400">
                                Lệnh này đã đóng, không thể thao tác thêm.
                            </p>
                        )}
                    </div>
                </div>
            </div>

            {/* MODAL UPLOAD ẢNH */}
            <ImageUploaderModal
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                onClose={onClose}
                imgSrc={imgSrc}
                setImgSrc={setImgSrc}
                uploadFn={handleUploadBill}
                isUploadPending={isCompleting}
                formDataKey={UPLOAD_FORM_KEY}
                requireCrop={false}
                label="Tải lên biên lai chuyển khoản"
            />
        </div>
    )
}
