"use client"

import { useMemo } from "react"
import { Spinner, Chip, User } from "@heroui/react"
import { ArrowRight, AlertCircle, CheckCircle2 } from "lucide-react"

import { useGetMySwaps, useCancelSwap } from "@/features/shifts/hooks"
import {
    ShiftSwapStatus,
    ShiftSwapStatusLabels,
    ShiftSwapStatusColors
} from "@/features/shifts/constants"
import { CreateSwapFormBody } from "@/features/shifts/components"

import { ButtonStyled } from "@/components"
import { useTokenStore } from "@/hooks"
import { ShiftResponse } from "@/features/shifts/models/response"

interface SwapShiftTabBodyProps {
    shift?: ShiftResponse
    assignmentId: string
}

export const SwapShiftTabBody = ({ shift, assignmentId }: SwapShiftTabBodyProps) => {
    const myUserId = useTokenStore((s) => s.userId)

    // 1. Fetch danh sách Swap của mình
    const { data: mySwaps, isLoading } = useGetMySwaps()
    const { cancelWithConfirm, isPending: isCanceling } = useCancelSwap()

    // 2. Tìm xem ca này đã có Đơn Đổi Ca nào do MÌNH TẠO chưa
    const existingSwap = useMemo(() => {
        if (!mySwaps || !shift) return null

        // Tìm đơn có shiftId trùng với ca hiện tại, và mình là người gửi (source)
        // LƯU Ý: Có thể lọc bỏ những đơn Đã Hủy hoặc Đã Từ Chối để user có thể tạo lại đơn mới
        return mySwaps.find(
            (s) =>
                s.shiftId === shift.id &&
                s.sourceUserId === myUserId &&
                s.status !== ShiftSwapStatus.Cancelled &&
                s.status !== ShiftSwapStatus.Rejected
        )
    }, [mySwaps, shift, myUserId])

    // --- RENDER 1: ĐANG TẢI ---
    if (isLoading || !shift) {
        return (
            <div className="flex flex-col items-center justify-center py-10 gap-2">
                <Spinner color="primary" />
                <p className="text-sm text-default-500 italic">Đang kiểm tra yêu cầu...</p>
            </div>
        )
    }

    // --- RENDER 2: ĐÃ CÓ YÊU CẦU (HIỂN THỊ TRẠNG THÁI) ---
    if (existingSwap) {
        const isPending = existingSwap.status === ShiftSwapStatus.PendingTarget
        // || existingSwap.status === ShiftSwapStatus.PendingManager

        return (
            <div className="flex flex-col gap-5 py-2">
                <div className="bg-default-50 border border-default-200 rounded-xl p-5 flex flex-col gap-4">
                    <div className="flex justify-between items-center border-b border-divider pb-3">
                        <span className="font-bold text-default-700">Trạng thái yêu cầu</span>
                        <Chip
                            color={ShiftSwapStatusColors[existingSwap.status]}
                            variant="flat"
                            size="sm"
                            className="font-bold uppercase tracking-wider text-[10px]"
                        >
                            {ShiftSwapStatusLabels[existingSwap.status]}
                        </Chip>
                    </div>

                    <div className="flex items-center justify-between px-2">
                        <User
                            name="Bạn"
                            description="Xin nghỉ"
                            avatarProps={{ src: "", size: "sm", name: "Me" }}
                            className="flex-1 justify-start"
                        />
                        <div className="flex flex-col items-center text-default-800 px-4">
                            <ArrowRight size={20} />
                            <span className="text-[10px] mt-1">Đổi cho</span>
                        </div>
                        <User
                            name={existingSwap.targetUserName}
                            description="Người đi thay"
                            avatarProps={{ src: "", size: "sm" }} // Nếu BE trả về targetUserAvatar thì map vào đây
                            className="flex-1 justify-end"
                        />
                    </div>

                    {existingSwap.reason && (
                        <div className="bg-white p-3 rounded-lg text-sm text-default-600 border border-default-100 italic">
                            &quot;{existingSwap.reason}&quot;
                        </div>
                    )}

                    {/* Lịch sử từ chối (Nếu quản lý/đồng nghiệp từ chối) */}
                    {existingSwap.rejectReason && (
                        <div className="bg-danger-50 p-3 rounded-lg text-sm text-danger-700 border border-danger-100 flex gap-2 items-start mt-2">
                            <AlertCircle size={16} className="mt-0.5 shrink-0" />
                            <span>
                                <span className="font-bold">Lý do từ chối:</span>{" "}
                                {existingSwap.rejectReason}
                            </span>
                        </div>
                    )}
                </div>

                {/* Nếu đang chờ thì cho phép Hủy */}
                {isPending && (
                    <ButtonStyled
                        color="danger"
                        variant="flat"
                        isLoading={isCanceling}
                        onPress={() => cancelWithConfirm(existingSwap.id)}
                        className="w-full font-bold"
                    >
                        Rút lại yêu cầu đổi ca
                    </ButtonStyled>
                )}

                {existingSwap.status === ShiftSwapStatus.Approved && (
                    <div className="text-center text-success-600 font-bold flex items-center justify-center gap-2 mt-2">
                        <CheckCircle2 size={18} />
                        Đã chốt xong! Bạn không cần làm ca này nữa.
                    </div>
                )}
            </div>
        )
    }

    return <CreateSwapFormBody shift={shift} assignmentId={assignmentId} />
}
