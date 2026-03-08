"use client"

import React, { useState } from "react"
import { Card, CardBody, User, Chip, Divider } from "@heroui/react"
import { ArrowRight, Clock, AlertCircle } from "lucide-react"
import dayjs from "dayjs"

import { ButtonStyled } from "@/components"
import { SwapRequestItem } from "@/features/shifts/models/response"
import {
    ShiftSwapStatus,
    ShiftSwapStatusLabels,
    ShiftSwapStatusColors
} from "@/features/shifts/constants"

interface SwapRequestCardProps {
    swap: SwapRequestItem
    currentUserId: string
    // Các hàm callback truyền từ View xuống
    onRespond?: (swapId: string, isAccepted: boolean) => Promise<void>
    onReview?: (swapId: string, isApproved: boolean) => Promise<void>
    onCancel?: (swapId: string) => Promise<void>
}

export const SwapRequestCard = ({
    swap,
    currentUserId,
    onRespond,
    onReview,
    onCancel
}: SwapRequestCardProps) => {
    const [actioning, setActioning] = useState<"approve" | "reject" | "cancel" | null>(null)

    // Xác định vai trò của User hiện tại đối với cái Đơn này
    const isSource = currentUserId === swap.sourceUserId
    const isTarget = currentUserId === swap.targetUserId

    // Logic hiển thị nút
    const canRespond = isTarget && swap.status === ShiftSwapStatus.PendingTarget && !!onRespond
    const canReview = swap.status === ShiftSwapStatus.PendingManager && !!onReview
    const canCancel = isSource && swap.status === ShiftSwapStatus.PendingTarget && !!onCancel

    const handleAction = async (
        type: "approve" | "reject" | "cancel",
        actionFn: () => Promise<void>
    ) => {
        setActioning(type)
        try {
            await actionFn()
        } finally {
            setActioning(null)
        }
    }

    return (
        <Card className="border border-default-100 shadow-sm hover:shadow-md transition-shadow w-full">
            <CardBody className="p-4 flex flex-col gap-4">
                {/* Header: Ngày giờ & Trạng thái */}
                <div className="flex justify-between items-center border-b border-divider pb-3">
                    <div className="flex items-center gap-2 text-primary-700 font-bold bg-primary-50 px-3 py-1.5 rounded-lg">
                        <Clock size={16} />
                        <span>
                            {dayjs(swap.shiftDate).format("DD/MM/YYYY")} (
                            {swap.startTime.substring(0, 5)} - {swap.endTime.substring(0, 5)})
                        </span>
                    </div>
                    <Chip
                        size="sm"
                        variant="flat"
                        color={ShiftSwapStatusColors[swap.status]}
                        className="font-bold tracking-wide uppercase text-[10px]"
                    >
                        {ShiftSwapStatusLabels[swap.status]}
                    </Chip>
                </div>

                {/* Body: Luồng đổi ca (A -> B) */}
                <div className="flex items-center justify-between px-2">
                    <User
                        name={swap.sourceUserName}
                        description="Xin nghỉ"
                        avatarProps={{
                            size: "sm",
                            color: "danger",
                            name: swap.sourceUserName.charAt(0)
                        }}
                        className="flex-1 justify-start"
                    />
                    <div className="flex flex-col items-center px-4 text-default-300">
                        <ArrowRight size={20} />
                        <span className="text-[10px] mt-1">Đổi cho</span>
                    </div>
                    <User
                        name={swap.targetUserName}
                        description="Đi làm thay"
                        avatarProps={{
                            size: "sm",
                            color: "success",
                            name: swap.targetUserName.charAt(0)
                        }}
                        className="flex-1 justify-end"
                    />
                </div>

                {/* Lý do & Lịch sử từ chối */}
                {(swap.reason || swap.rejectReason) && (
                    <div className="flex flex-col gap-2 mt-1">
                        {swap.reason && (
                            <div className="bg-default-50 p-2.5 rounded-lg text-sm text-default-600 border border-default-100 italic">
                                💬 &quot;{swap.reason}&quot;
                            </div>
                        )}
                        {swap.rejectReason && (
                            <div className="bg-danger-50 p-2.5 rounded-lg text-sm text-danger-700 border border-danger-100 flex gap-2 items-start">
                                <AlertCircle size={16} className="mt-0.5 shrink-0" />
                                <span>
                                    <span className="font-bold">Từ chối vì:</span>{" "}
                                    {swap.rejectReason}
                                </span>
                            </div>
                        )}
                    </div>
                )}

                {/* Footer: Dàn nút Action (Inline) */}
                {(canRespond || canReview || canCancel) && (
                    <>
                        <Divider className="my-1" />
                        <div className="flex items-center justify-end gap-2 pt-1">
                            {/* Nút cho Target (Lính nhận ca) */}
                            {canRespond && (
                                <>
                                    <ButtonStyled
                                        size="sm"
                                        color="danger"
                                        variant="flat"
                                        isLoading={actioning === "reject"}
                                        isDisabled={actioning === "approve"}
                                        onPress={() =>
                                            handleAction("reject", () => onRespond(swap.id, false))
                                        }
                                    >
                                        Từ chối
                                    </ButtonStyled>
                                    <ButtonStyled
                                        size="sm"
                                        color="primary"
                                        className="font-bold text-white shadow-sm"
                                        isLoading={actioning === "approve"}
                                        isDisabled={actioning === "reject"}
                                        onPress={() =>
                                            handleAction("approve", () => onRespond(swap.id, true))
                                        }
                                    >
                                        Đồng ý nhận ca
                                    </ButtonStyled>
                                </>
                            )}

                            {/* Nút cho Manager (Sếp duyệt) */}
                            {canReview && (
                                <>
                                    <ButtonStyled
                                        size="sm"
                                        color="danger"
                                        variant="flat"
                                        isLoading={actioning === "reject"}
                                        isDisabled={actioning === "approve"}
                                        onPress={() =>
                                            handleAction("reject", () => onReview(swap.id, false))
                                        }
                                    >
                                        Từ chối yêu cầu
                                    </ButtonStyled>
                                    <ButtonStyled
                                        size="sm"
                                        color="success"
                                        className="font-bold text-white shadow-sm"
                                        isLoading={actioning === "approve"}
                                        isDisabled={actioning === "reject"}
                                        onPress={() =>
                                            handleAction("approve", () => onReview(swap.id, true))
                                        }
                                    >
                                        Phê duyệt đổi ca
                                    </ButtonStyled>
                                </>
                            )}

                            {/* Nút cho Source (Lính xin đổi) */}
                            {canCancel && (
                                <ButtonStyled
                                    size="sm"
                                    color="default"
                                    variant="flat"
                                    isLoading={actioning === "cancel"}
                                    onPress={() => handleAction("cancel", () => onCancel(swap.id))}
                                >
                                    Rút lại yêu cầu
                                </ButtonStyled>
                            )}
                        </div>
                    </>
                )}
            </CardBody>
        </Card>
    )
}
