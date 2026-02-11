"use client"

import React, { useRef, useState } from "react"
import { CalendarApi } from "@fullcalendar/core"
import {
    useCreateShift,
    useDeleteShift,
    useGetShifts,
    useScheduleLogic,
    useUpdateShift
} from "../../hooks"
import { ScheduleToolbar } from "../../components/ScheduleToolbar"
import { ShiftCalendar } from "../../components/ShiftCalendar"
import { confirmDialog, useDay, useTokenStore } from "@/hooks"
import { addToast, useDisclosure } from "@heroui/react"
import { ShiftFormValues } from "../../components/ShiftModal/ShiftForm/schema"
import { ShiftResponse } from "../../models/response"
import dayjs from "dayjs"
import { ShiftModal } from "../../components"
import { CreateShiftRequest, UpdateShiftRequest } from "../../models/request"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { CalendarRange } from "lucide-react"
import { ShiftStatus } from "../../constants"

export default function ShiftSchedulerPageView() {
    const { formatDate, formatToApiUTC, formatDateTime } = useDay({ defaultFormat: "HH:mm" })

    // 1. Logic Hook
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

    // 2. Ref để giữ CalendarApi (để gọi hàm next/prev từ toolbar)
    const calendarApiRef = useRef<CalendarApi | null>(null)

    // 3. Gọi API
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
    const [selectedShift, setSelectedShift] = useState<Partial<ShiftFormValues> | null>(null)
    const [selectedShiftId, setSelectedShiftId] = useState<string | null>(null) // Lưu ID để update/delete

    // API Mutations
    const createMutation = useCreateShift({ onSuccess: onClose })
    const updateMutation = useUpdateShift({ onSuccess: onClose })
    const deleteMutation = useDeleteShift({ onSuccess: onClose })

    // ================================
    // --- HANDLERS ---
    // ================================

    // 1. Click vào ô trống -> Mở Modal tạo mới
    const handleDateClick = (date: Date) => {
        setSelectedShiftId(null)
        setSelectedShift({
            id: undefined,
            shiftDate: date,
            startTime: "08:00", // Default
            endTime: "12:00"
        })
        onOpen()
    }

    // 2. Click vào Event có sẵn -> Mở Modal Edit
    const handleEventClick = (shift: ShiftResponse) => {
        setSelectedShiftId(shift.id)
        setSelectedShift({
            id: shift.id,
            shiftDate: new Date(shift.shiftDate),
            startTime: formatDateTime({ date: shift.startTime }), // Lấy HH:mm từ ISO string
            endTime: formatDateTime({ date: shift.endTime }),
            requiredStaffCount: shift.requiredStaffCount,
            breakDurationMinutes: shift.breakDurationMinutes,
            note: shift.note,
            status: shift.status,
            shiftTemplateId: shift.shiftTemplateId
        })
        onOpen()
    }

    // 3. Kéo thả (Drag & Drop) -> Update ngay lập tức
    const handleEventDrop = (
        shiftId: string,
        newStart: Date,
        newEnd: Date,
        dropInfo: any // Thêm tham số này
    ) => {
        const targetShift = shiftsData?.find((s) => s.id === shiftId)

        const isSubmitable =
            targetShift?.status === ShiftStatus.Draft ||
            targetShift?.status === ShiftStatus.Published

        if (!isSubmitable) {
            // 1. Thực hiện REVERT ngay lập tức trên UI
            if (dropInfo && typeof dropInfo.revert === "function") {
                dropInfo.revert()
            }

            // 2. Báo lỗi cho User
            addToast({
                title: "Không thể di chuyển",
                description: "Ca làm việc đã hoàn thành hoặc đã hủy không thể thay đổi thời gian.",
                color: "warning"
            })

            return
        }

        // --- Logic update API ---
        const startISO = formatToApiUTC(newStart)
        const endISO = formatToApiUTC(newEnd)

        if (!startISO || !endISO) {
            dropInfo?.revert()
            return
        }

        updateMutation.mutate(
            {
                id: shiftId,
                req: {
                    startTime: startISO,
                    endTime: endISO,
                    shiftDate: formatDate(newStart)
                }
            },
            {
                onSettled: (_, error) => {
                    if (error) {
                        // Chỉ revert khi có lỗi
                        dropInfo?.revert()
                    }
                }
            }
        )
    }

    // 4. Submit Form (Create or Update)
    const handleModalSubmit = (data: ShiftFormValues) => {
        // 1. Format Shift Date (Dùng hàm có sẵn của bạn)
        const dateStr = formatDate(data.shiftDate) // Ra "2026-02-10"

        // 2. Logic tính toán giờ (Vẫn cần làm ở đây vì liên quan business logic ca đêm)
        const startDateTime = dayjs(`${dateStr}T${data.startTime}:00`)
        let endDateTime = dayjs(`${dateStr}T${data.endTime}:00`)

        if (endDateTime.isBefore(startDateTime) || endDateTime.isSame(startDateTime)) {
            endDateTime = endDateTime.add(1, "day")
        }

        // 3. Convert sang UTC để gửi API (Dùng hàm MỚI)
        const startISO = formatToApiUTC(startDateTime)
        const endISO = formatToApiUTC(endDateTime)

        if (selectedShiftId) {
            // Update Mode
            const payload: UpdateShiftRequest = {
                // shiftDate vẫn giữ nguyên là ngày bắt đầu ca (để BE biết ca này thuộc ngày nào)
                shiftDate: dateStr,
                startTime: startISO,
                endTime: endISO,
                requiredStaffCount: data.requiredStaffCount,
                breakDurationMinutes: data.breakDurationMinutes,
                note: data.note || "",
                status: data.status,
                shiftTemplateId: data.shiftTemplateId || undefined
            }

            updateMutation.mutate({ id: selectedShiftId, req: payload })
        } else {
            // Create Mode
            const payload: CreateShiftRequest = {
                storeId: storeId!,
                shiftDate: dateStr,
                startTime: startISO!,
                endTime: endISO!,
                requiredStaffCount: data.requiredStaffCount,
                breakDurationMinutes: data.breakDurationMinutes,
                note: data.note || "",
                shiftTemplateId: data.shiftTemplateId || undefined
            }
            createMutation.mutate(payload)
        }
    }

    // 5. Delete Handler
    const handleDelete = async () => {
        if (!selectedShiftId) return

        const isConfirmed = await confirmDialog({
            title: "Xóa Ca làm việc",
            message: "Hành động này không thể hoàn tác. Bạn chắc chứ?",
            variant: "danger"
        })
        if (isConfirmed) {
            deleteMutation.mutate({ id: selectedShiftId })
        }
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            {/* Header */}
            <DashboardHeader
                icon={<CalendarRange className="w-6 h-6 text-white" />}
                title="Xếp Lịch Làm Việc"
                description="Quản lý ca làm và phân công nhân sự"
            />

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
            />

            {/* Main Calendar Area */}
            <div className="flex-1 bg-white rounded-xl shadow-sm border border-gray-200 p-4 overflow-hidden">
                <ShiftCalendar
                    shifts={shiftsData || []}
                    isLoading={isLoading}
                    viewMode={viewMode}
                    onCalendarApiReady={(api) => (calendarApiRef.current = api)}
                    onDateClick={handleDateClick}
                    onEventClick={handleEventClick}
                    onEventDrop={handleEventDrop}
                />
            </div>

            <ShiftModal
                isOpen={isOpen}
                onClose={onClose}
                onSubmit={handleModalSubmit}
                initialData={selectedShift}
                isSubmitting={createMutation.isPending || updateMutation.isPending}
                onDelete={selectedShiftId ? handleDelete : undefined}
            />
        </div>
    )
}
