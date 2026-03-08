"use client"

import React, { useState } from "react"
import { Tabs, Tab, Spinner } from "@heroui/react"
import { useGetAllSwaps, useReviewSwap } from "@/features/shifts/hooks"
import { ShiftSwapStatus } from "@/features/shifts/constants"
import { SwapRequestCard } from "../SwapRequestCard"
import { useTokenStore } from "@/hooks"

export const ManagerSwapView = () => {
    const myUserId = useTokenStore((s) => s.userId)

    // Tab 1: Cần duyệt (PendingManager), Tab 2: Lịch sử (Tất cả)
    const [selectedTab, setSelectedTab] = useState<string>("pending")

    const queryParams = selectedTab === "pending" ? { status: ShiftSwapStatus.PendingManager } : {}

    const { data: swaps, isLoading } = useGetAllSwaps({ query: queryParams })
    const { reviewSwapWithConfirm } = useReviewSwap()

    if (isLoading)
        return (
            <div className="flex justify-center p-10">
                <Spinner color="primary" />
            </div>
        )

    return (
        <div className="flex flex-col gap-4">
            <Tabs
                aria-label="Manager Swap Tabs"
                color="primary"
                variant="underlined"
                selectedKey={selectedTab}
                onSelectionChange={(key) => setSelectedTab(key as string)}
            >
                <Tab key="pending" title="Cần Sếp duyệt">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                        {!swaps || swaps.length === 0 ? (
                            <p className="text-default-400 italic p-4">
                                Không có đơn nào cần duyệt.
                            </p>
                        ) : (
                            swaps.map((swap) => (
                                <SwapRequestCard
                                    key={swap.id}
                                    swap={swap}
                                    currentUserId={myUserId!}
                                    onReview={reviewSwapWithConfirm}
                                />
                            ))
                        )}
                    </div>
                </Tab>

                <Tab key="history" title="Toàn bộ Lịch sử">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                        {!swaps || swaps.length === 0 ? (
                            <p className="text-default-400 italic p-4">Không có dữ liệu.</p>
                        ) : (
                            swaps.map((swap) => (
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
