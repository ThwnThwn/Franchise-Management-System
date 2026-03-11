"use client"

import React, { useRef } from "react"
import { CalendarApi } from "@fullcalendar/core"

import {
    ShiftModal,
    ScheduleToolbar,
    ShiftCalendar,
    ScheduleStats,
    BulkShiftModal,
    StaffWorkloadSummary,
    GoToTimeLogLink
} from "@/features/shifts/components"
import {
    useGetShifts,
    useScheduleLogic,
    useScheduleBulkActions,
    useShiftActions
} from "@/features/shifts/hooks"

import { DashboardHeader } from "@/features/dashboard"
import { useScrollSnap, useTokenStore } from "@/hooks"
import { useDisclosure } from "@heroui/react"
import { CalendarRange } from "lucide-react"

export default function ShiftSchedulerPageView() {
    // Logic Hook
    const {
        currentDate,
        viewMode,
        dateRange,
        handleNext,
        handlePrev,
        handleToday,
        // handleDateSelect,
        handleViewChange,
        handleJumpToDate
    } = useScheduleLogic()

    // Ref để giữ CalendarApi (để gọi hàm next/prev từ toolbar)
    const calendarApiRef = useRef<CalendarApi | null>(null)

    // Gọi API
    const storeId = useTokenStore((s) => s.storeId)
    const { data: shiftsData, isLoading } = useGetShifts({
        params: {
            storeId: storeId || "",
            fromDate: dateRange.fromDate,
            toDate: dateRange.toDate
        },
        enabled: !!storeId
    })

    // ================================
    // State cho Modal
    const { isOpen, onOpen, onClose } = useDisclosure()
    const { isOpen: isBulkOpen, onOpen: onBulkOpen, onClose: onBulkClose } = useDisclosure()

    // Hook Xử lý ca lẻ (Single Actions)
    const shiftActions = useShiftActions({
        storeId: storeId!,
        shiftsData: shiftsData || [],
        onOpenModal: onOpen,
        onCloseModal: onClose
    })

    // Hook Xử lý Bulk (Bulk Actions)
    const {
        bulkActionIds,
        handleBulkPublish,
        handleBulkDelete,
        handleBulkOpen,
        handleAutoFill,
        isProcessing
    } = useScheduleBulkActions({
        // currentDate: currentDate,
        dateRange: dateRange,
        shiftsData: shiftsData
    })

    const calendarSnapRef = useScrollSnap({
        thresholdTop: -30,
        offset: 100
    })

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            {/* Header */}
            <div className="flex flex-col md:flex-row justify-between">
                <DashboardHeader
                    icon={<CalendarRange className="w-6 h-6 text-white" />}
                    title="Xếp Lịch Làm Việc"
                    description="Quản lý ca làm và phân công nhân sự"
                />
                <GoToTimeLogLink />
            </div>

            <div className="space-y-3">
                {/* Toolbar */}
                <ScheduleToolbar
                    currentDate={currentDate}
                    viewMode={viewMode}
                    onNext={() => handleNext(calendarApiRef.current!)}
                    onPrev={() => handlePrev(calendarApiRef.current!)}
                    onToday={() => handleToday(calendarApiRef.current!)}
                    onViewChange={(mode) => handleViewChange(mode, calendarApiRef.current!)}
                    // onDateSelect={(type, val) => handleDateSelect(type, val, calendarApiRef.current!)}
                    onJumpToDate={(date) => handleJumpToDate(date, calendarApiRef.current!)}
                    //
                    forDeleteCount={bulkActionIds.forDelete.length}
                    forPublishCount={bulkActionIds.forPublish.length}
                    forOpenCount={bulkActionIds.forOpen.length}
                    forAutoFillCount={bulkActionIds.forAutoFill.length}
                    //
                    onBulkCreate={onBulkOpen}
                    onBulkPublish={handleBulkPublish}
                    onBulkDelete={handleBulkDelete}
                    onBulkOpen={handleBulkOpen}
                    onAutoFill={handleAutoFill}
                    isBulkProcessing={isProcessing}
                />
                {/* Stats */}
                <ScheduleStats shifts={shiftsData || []} />

                {/* Overwork */}
                <StaffWorkloadSummary shifts={shiftsData || []} storeId={storeId!} />

                {/* Main Calendar Area */}
                <div
                    ref={calendarSnapRef}
                    className="bg-white rounded-xl shadow-sm border border-gray-200 p-4 overflow-hidden"
                >
                    <ShiftCalendar
                        shifts={shiftsData || []}
                        isLoading={isLoading}
                        viewMode={viewMode}
                        onCalendarApiReady={(api) => (calendarApiRef.current = api)}
                        onDateClick={shiftActions.handleDateClick}
                        onEventClick={shiftActions.handleEventClick}
                        onEventDrop={shiftActions.handleEventDrop}
                    />
                </div>
            </div>

            {/* Modals */}
            <>
                <ShiftModal
                    isOpen={isOpen}
                    onClose={onClose}
                    onSubmit={shiftActions.handleModalSubmit}
                    initialData={shiftActions.selectedShift}
                    isSubmitting={shiftActions.isSubmitting}
                    onDelete={shiftActions.selectedShiftId ? shiftActions.handleDelete : undefined}
                />

                <BulkShiftModal
                    isOpen={isBulkOpen}
                    onClose={onBulkClose}
                    storeId={storeId || ""}
                    // currentDate={currentDate}
                    dateRange={dateRange}
                />
            </>
        </div>
    )
}
