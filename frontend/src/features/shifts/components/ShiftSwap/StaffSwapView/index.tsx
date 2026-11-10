"use client"

import React, { useMemo } from "react"
import { Tabs, Tab, Spinner } from "@heroui/react"
import { useGetMySwaps, useRespondSwap, useCancelSwap } from "@/features/shifts/hooks"
import { ShiftSwapStatus } from "@/features/shifts/constants"
import { SwapRequestCard } from "../SwapRequestCard"
import { useTokenStore } from "@/hooks"

export const StaffSwapView = () => {
    const myUserId = useTokenStore((s) => s.userId)

    // Gọi API lấy ALL đơn liên quan tới mình
    const { data: mySwaps, isLoading } = useGetMySwaps()

    // Khởi tạo các hook xử lý action
    const { respondWithConfirm } = useRespondSwap()
    const { cancelWithConfirm } = useCancelSwap()

    // TAB 1: CẦN MÌNH PHẢN HỒI
    const needsActionSwaps = useMemo(() => {
        if (!mySwaps) return []
        return mySwaps.filter(
            (s) => s.targetUserId === myUserId && s.status === ShiftSwapStatus.PendingTarget
        )
    }, [mySwaps, myUserId])

    // TAB 2: ĐANG TIẾN HÀNH
    const ongoingSwaps = useMemo(() => {
        if (!mySwaps) return []
        return mySwaps.filter((s) => {
            // Case A: Đơn mình TỰ TẠO gửi đi đang chờ (chờ đồng nghiệp hoặc chờ sếp)
            const isMySentPending =
                s.sourceUserId === myUserId &&
                (s.status === ShiftSwapStatus.PendingTarget ||
                    s.status === ShiftSwapStatus.PendingManager)

            // Case B: Đơn mình ĐƯỢC NHỜ, mình ĐÃ ĐỒNG Ý rồi, giờ ngồi rung đùi chờ Sếp duyệt
            const isMyAcceptedPending =
                s.targetUserId === myUserId && s.status === ShiftSwapStatus.PendingManager

            return isMySentPending || isMyAcceptedPending
        })
    }, [mySwaps, myUserId])

    // TAB 3: LỊCH SỬ KHÉP KÍN (Đã xong xuôi)
    const historySwaps = useMemo(() => {
        if (!mySwaps) return []
        const terminalStatuses = [
            ShiftSwapStatus.Approved,
            ShiftSwapStatus.Rejected,
            ShiftSwapStatus.Cancelled,
            ShiftSwapStatus.Expired
        ]
        return mySwaps.filter((s) => terminalStatuses.includes(s.status))
    }, [mySwaps])

    if (isLoading)
        return (
            <div className="flex justify-center p-10">
                <Spinner color="primary" />
            </div>
        )

    return (
        <div className="flex flex-col gap-4">
            <Tabs aria-label="Staff Swap Tabs" color="primary" variant="underlined" fullWidth>
                {/* TAB 1 */}
                <Tab
                    key="action"
                    title={
                        <div className="flex items-center gap-2">
                            <span>Cần phản hồi</span>
                            {needsActionSwaps.length > 0 && (
                                <span className="bg-danger text-white text-xs px-1.5 rounded-full">
                                    {needsActionSwaps.length}
                                </span>
                            )}
                        </div>
                    }
                >
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                        {needsActionSwaps.length === 0 ? (
                            <p className="text-default-400 italic p-4">
                                Bạn không có lời mời đổi ca nào.
                            </p>
                        ) : (
                            needsActionSwaps.map((swap) => (
                                <SwapRequestCard
                                    key={swap.id}
                                    swap={swap}
                                    currentUserId={myUserId!}
                                    onRespond={respondWithConfirm}
                                />
                            ))
                        )}
                    </div>
                </Tab>

                {/* TAB 2 */}
                <Tab key="ongoing" title="Đang xử lý">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                        {ongoingSwaps.length === 0 ? (
                            <p className="text-default-400 italic p-4">
                                Không có đơn đổi ca nào đang diễn ra.
                            </p>
                        ) : (
                            ongoingSwaps.map((swap) => (
                                <SwapRequestCard
                                    key={swap.id}
                                    swap={swap}
                                    currentUserId={myUserId!}
                                    onCancel={cancelWithConfirm}
                                />
                            ))
                        )}
                    </div>
                </Tab>

                {/* TAB 3 */}
                <Tab key="history" title="Lịch sử">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                        {historySwaps.length === 0 ? (
                            <p className="text-default-400 italic p-4">Chưa có lịch sử đổi ca.</p>
                        ) : (
                            historySwaps.map((swap) => (
                                <SwapRequestCard
                                    key={swap.id}
                                    swap={swap}
                                    currentUserId={myUserId!}
                                />
                            ))
                        )}
                    </div>
                </Tab>
            </Tabs>
        </div>
    )
}
