"use client"

import { useMemo, useState } from "react"
import {
    AgendaListView,
    AgendaToolbar,
    CurrentShiftCard,
    GoToTimeLogLink,
    StaffShiftCalendar,
    ViewShiftDetailModal
} from "@/features/shifts/components"
import {
    useGetMyAgenda,
    useAgendaLogic,
    useGetMyTimeLogHistory,
    useGetShifts
} from "@/features/shifts/hooks"
import { DashboardHeader } from "@/features/dashboard"
import { CalendarRange } from "lucide-react"
import { Spinner, useDisclosure } from "@heroui/react"
import {
    ShiftResponse,
    StaffShiftAgendaWithLog,
    TimeLogResponse
} from "@/features/shifts/models/response"
import { useTokenStore } from "@/hooks"

export default function StaffAgendaPageView() {
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
    const { data: shifts, isLoading: isShiftDetailLoading } = useGetShifts({
        params: {
            fromDate: dateRange.startDate,
            toDate: dateRange.endDate,
            storeId: storeId
        },
        enabled: !!storeId
    })
    const { data: weeklyShifts, isLoading: isShiftLoading } = useGetMyAgenda(dateRange)
    const { data: weeklyTimelogs, isLoading: isTimeLogLoading } = useGetMyTimeLogHistory({
        query: {
            fromDate: dateRange.startDate,
            toDate: dateRange.endDate
        },
        pagination: { page: 1, pageSize: 100 }
    })
    const isLoading = isShiftLoading || isTimeLogLoading || isShiftDetailLoading

    const enrichedShifts = useMemo((): StaffShiftAgendaWithLog[] => {
        if (!weeklyShifts) return []

        // Tạo Map để tìm kiếm O(1)
        const logMap = new Map<string, TimeLogResponse>()
        weeklyTimelogs?.items?.forEach((log) => {
            // Map theo shiftAssignmentId để khớp chính xác với ID của agenda
            logMap.set(log.shiftAssignmentId, log)
        })

        const shiftMap = new Map<string, ShiftResponse>()
        shifts?.forEach((shift) => {
            shiftMap.set(shift.id, shift)
        })

        // Trộn dữ liệu
        return weeklyShifts.map((shift) => {
            const log = logMap.get(shift.id)
            const item = shiftMap.get(shift.shiftId)

            return {
                ...shift,
                timeLog: log,
                shiftInfo: item
            }
        })
    }, [shifts, weeklyShifts, weeklyTimelogs])

    // Xử lý khi click vào bất kỳ đâu (Card trong List hoặc Event trong Calendar)
    const { isOpen, onOpen, onClose } = useDisclosure()
    const [selectedShift, setSelectedShift] = useState<StaffShiftAgendaWithLog | null>(null)

    const handleShiftClick = (data: StaffShiftAgendaWithLog) => {
        setSelectedShift(data)
        onOpen()
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-3">
            <div className="flex flex-col md:flex-row justify-between">
                <DashboardHeader
                    icon={<CalendarRange className="text-white" />}
                    title="Lịch Làm Việc"
                    description="Theo dõi ca làm và đồng nghiệp trong tuần"
                />
                <GoToTimeLogLink />
            </div>

            <section className="animate-in fade-in slide-in-from-top-4 duration-700">
                <CurrentShiftCard />
            </section>

            <AgendaToolbar
                currentDate={currentDate}
                viewMode={viewMode}
                onViewChange={setViewMode}
                onNext={handleNext}
                onPrev={handlePrev}
                onToday={handleToday}
                onJump={handleJumpToDate}
            />

            <div className="min-h-100">
                {isLoading ? (
                    <div className="block text-center">
                        <Spinner className="my-10" label="Đang lấy thông tin ca làm..." />
                    </div>
                ) : weeklyShifts && weeklyShifts.length > 0 ? (
                    // Trên mobile luôn hiện List, desktop hiện theo viewMode
                    <div className="block">
                        {viewMode === "list" ? (
                            <AgendaListView
                                selectedDate={currentDate}
                                shifts={enrichedShifts}
                                onEditClick={handleShiftClick}
                            />
                        ) : (
                            <div className="bg-white p-4 rounded-2xl shadow-sm border">
                                <StaffShiftCalendar
                                    currentDate={currentDate}
                                    agenda={enrichedShifts}
                                    isLoading={isLoading}
                                    onEventClick={handleShiftClick}
                                />
                            </div>
                        )}
                    </div>
                ) : (
                    <div className="text-center py-20 bg-default-50 rounded-3xl border-2 border-dashed">
                        <p className="text-default-400 font-bold italic">
                            Chưa có ca làm nào trong khoảng thời gian này!
                        </p>
                    </div>
                )}
            </div>
            <ViewShiftDetailModal isOpen={isOpen} onClose={onClose} data={selectedShift} />
        </div>
    )
}
