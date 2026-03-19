"use client"

import { useState } from "react"
import { useDisclosure, Spinner } from "@heroui/react"
import { Megaphone } from "lucide-react"

import {
    ShiftPoolListView,
    ShiftPoolDetailModal,
    AgendaToolbar
} from "@/features/shifts/components"
import { DashboardHeader } from "@/features/dashboard"

// Import hooks
import { useAgendaLogic, useGetShifts } from "@/features/shifts/hooks"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"
import { useTokenStore } from "@/hooks"

export default function ShiftPoolPageView() {
    const {
        currentDate,
        viewMode,
        setViewMode,
        dateRange,
        handleNext,
        handlePrev,
        handleToday,
        handleJumpToDate
    } = useAgendaLogic()

    const storeId = useTokenStore((s) => s.storeId) || ""

    // Lấy danh sách ca đang OPEN
    const { data: shifts, isLoading } = useGetShifts({
        params: {
            fromDate: dateRange.startDate,
            toDate: dateRange.endDate,
            storeId: storeId,
            status: ShiftStatus.Open
        },
        enabled: !!storeId
    })

    // Modal chi tiết ca
    const { isOpen, onOpen, onClose } = useDisclosure()
    const [selectedShift, setSelectedShift] = useState<ShiftResponse | null>(null)

    const handleCardClick = (shift: ShiftResponse) => {
        setSelectedShift(shift)
        onOpen()
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-4">
            <DashboardHeader
                icon={<Megaphone className="text-white" />}
                title="Chợ Ca Làm Việc"
                description="Tìm kiếm và đăng ký các ca làm đang còn trống"
            />

            <AgendaToolbar
                currentDate={currentDate}
                viewMode={viewMode}
                onViewChange={setViewMode}
                onNext={handleNext}
                onPrev={handlePrev}
                onToday={handleToday}
                onJump={handleJumpToDate}
            />

            <div className="min-h-100 mt-4">
                {isLoading ? (
                    <div className="block text-center">
                        <Spinner className="my-10" label="Đang quét chợ ca..." color="primary" />
                    </div>
                ) : (
                    <ShiftPoolListView
                        shifts={shifts || []}
                        viewMode={viewMode}
                        selectedDate={currentDate}
                        dateRange={dateRange}
                        onCardClick={handleCardClick}
                        // onApplyClick={handleApplyShift}
                        // isApplyingId={applyingShiftId}
                    />
                )}
            </div>

            <ShiftPoolDetailModal isOpen={isOpen} onClose={onClose} shift={selectedShift} />
        </div>
    )
}
