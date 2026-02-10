"use client"

import React, { useRef, useEffect } from "react"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import timeGridPlugin from "@fullcalendar/timegrid"
import interactionPlugin from "@fullcalendar/interaction"
import { CalendarApi } from "@fullcalendar/core"
import { Users, AlertCircle, CheckCircle2 } from "lucide-react"
import { ShiftResponse } from "../../models/response"
import { ShiftStatus } from "../../constants"

interface ShiftCalendarProps {
    shifts: ShiftResponse[]
    isLoading: boolean
    viewMode: "dayGridMonth" | "timeGridWeek"
    onCalendarApiReady: (api: CalendarApi) => void // Callback để trả API về cha

    onDateClick: (date: Date) => void
    onEventClick: (shift: ShiftResponse) => void
    onEventDrop: (shiftId: string, newStart: Date, newEnd: Date) => void
}

export const ShiftCalendar = ({
    shifts,
    isLoading,
    viewMode,
    onCalendarApiReady,
    onDateClick,
    onEventClick,
    onEventDrop
}: ShiftCalendarProps) => {
    const calendarRef = useRef<FullCalendar>(null)

    // Gửi Calendar API ra ngoài khi mount
    useEffect(() => {
        if (calendarRef.current) {
            onCalendarApiReady(calendarRef.current.getApi())
        }
    }, [onCalendarApiReady])

    // --- 1. Map Data API -> FullCalendar Events ---
    const events = shifts.map((shift) => ({
        id: shift.id,
        title: shift.note || "Ca làm việc", // Fallback title
        start: shift.startTime, // String ISO ok
        end: shift.endTime,
        extendedProps: shift, // Lưu toàn bộ object gốc để dùng khi render
        // Logic màu sắc cơ bản (Custom Render sẽ ghi đè cái này nhưng giữ làm fallback)
        backgroundColor: shift.status === ShiftStatus.Draft ? "#f3f4f6" : "#ecfccb",
        borderColor: "transparent",
        textColor: "#000"
    }))

    // --- 2. Custom Render Event Content ---
    const renderEventContent = (eventInfo: any) => {
        const shift = eventInfo.event.extendedProps as ShiftResponse
        const isDraft = shift.status === ShiftStatus.Draft
        const isFull = shift.assignedStaffCount >= shift.requiredStaffCount

        // Dynamic Styles
        const containerClass = `
            w-full h-full p-1 rounded-md border-l-4 text-xs flex flex-col justify-between overflow-hidden
            ${
                isDraft
                    ? "bg-gray-100 border-gray-400 text-gray-500 border-dashed border-1"
                    : isFull
                      ? "bg-green-100 border-green-500 text-green-800"
                      : "bg-orange-50 border-orange-400 text-orange-800"
            }
        `

        return (
            <div className={containerClass}>
                <div className="font-semibold truncate">{eventInfo.timeText}</div>

                {/* Note/Template Name */}
                <div className="truncate font-medium">
                    {shift.shiftTemplateId ? "Theo mẫu" : shift.note || "Ca tùy chỉnh"}
                </div>

                {/* Staff Counter Footer */}
                <div className="flex items-center gap-1 mt-1">
                    {isDraft ? (
                        <span className="italic text-[10px]">Bản nháp</span>
                    ) : (
                        <>
                            {isFull ? <CheckCircle2 size={10} /> : <AlertCircle size={10} />}
                            <span className="font-bold">
                                {shift.assignedStaffCount}/{shift.requiredStaffCount}
                            </span>
                            <Users size={10} />
                        </>
                    )}
                </div>
            </div>
        )
    }

    // Helper: Xử lý khi Drop (Kéo thả)
    const handleDrop = (info: any) => {
        const shiftId = info.event.id
        const newStart = info.event.start
        const newEnd = info.event.end

        // Nếu view tháng, drop chỉ đổi ngày, giữ nguyên giờ cũ -> logic này FullCalendar tự lo 1 phần
        // Nhưng ta cần gọi API update
        onEventDrop(shiftId, newStart, newEnd)
    }

    return (
        <div className={`calendar-wrapper ${isLoading ? "opacity-50 pointer-events-none" : ""}`}>
            <FullCalendar
                ref={calendarRef}
                plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
                initialView={viewMode}
                headerToolbar={false} // Tắt toolbar mặc định để dùng cái mình tự build
                events={events}
                eventContent={renderEventContent}
                // Settings
                // 1. Ép định dạng 24h cho text hiển thị trên Event
                eventTimeFormat={{
                    hour: "2-digit",
                    minute: "2-digit",
                    meridiem: false,
                    hour12: false
                }}
                // 2. Ép định dạng 24h cho cột thời gian bên trái (TimeGrid)
                slotLabelFormat={{
                    hour: "2-digit",
                    minute: "2-digit",
                    hour12: false
                }}
                // 3. (Tùy chọn) Chuyển sang tiếng Việt để mặc định dùng 24h và text header thân thiện hơn
                locale="vi"
                firstDay={1} // Thứ 2 là đầu tuần
                slotMinTime="00:00:00"
                slotMaxTime="24:00:00"
                scrollTime="07:00:00"
                nextDayThreshold="00:00:00"
                allDaySlot={false} // Tắt dòng all-day
                height="auto"
                slotDuration="01:00:00" // Mỗi ô là 1 tiếng
                expandRows={true}
                stickyHeaderDates={true}
                //===============================
                // Interaction Settings
                editable={true}
                droppable={true}
                selectable={true}
                // Event Handlers
                dateClick={(info) => onDateClick(info.date)}
                eventClick={(info) => onEventClick(info.event.extendedProps as ShiftResponse)}
                eventDrop={handleDrop}
                eventResize={handleDrop}
                //===============================
                // Styling text
                eventClassNames="cursor-pointer hover:opacity-90 transition-opacity shadow-sm"
            />

            {/* Style override global cho FullCalendar đẹp hơn */}
            <style
                dangerouslySetInnerHTML={{
                    __html: `
                        .fc-timegrid-slot { height: 3rem !important; }
                        .fc-col-header-cell { background: #f9fafb; padding: 8px 0; }
                        .fc-theme-standard td, .fc-theme-standard th { border-color: #f3f4f6; }
                        .fc-timegrid-now-indicator-line { border-color: #ef4444; }
                        .fc-timegrid-now-indicator-arrow { border-color: #ef4444; }
                    `
                }}
            />
        </div>
    )
}
