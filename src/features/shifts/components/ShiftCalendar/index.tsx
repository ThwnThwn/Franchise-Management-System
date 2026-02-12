"use client"

import React, { useRef, useEffect, useCallback, useMemo } from "react"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import timeGridPlugin from "@fullcalendar/timegrid"
import interactionPlugin from "@fullcalendar/interaction"
import { CalendarApi } from "@fullcalendar/core"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftEventCard } from "./ShiftEventCard"

interface ShiftCalendarProps {
    shifts: ShiftResponse[]
    isLoading: boolean
    viewMode: "dayGridMonth" | "timeGridWeek"
    onCalendarApiReady: (api: CalendarApi) => void // Callback để trả API về cha

    onDateClick: (date: Date) => void
    onEventClick: (shift: ShiftResponse) => void
    onEventDrop: (shiftId: string, newStart: Date, newEnd: Date, dropInfo: any) => void
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

    // 1. Chặn Drag & Drop trên Mobile (Check đơn giản bằng window width)
    // Hoặc bro có thể dùng một custom hook useMediaQuery xịn hơn
    const isMobile = typeof window !== "undefined" && window.innerWidth < 768

    useEffect(() => {
        if (calendarRef.current) {
            onCalendarApiReady(calendarRef.current.getApi())
        }
    }, [onCalendarApiReady])

    // 2. Map Data - Dùng useMemo để không map lại mảng nếu shifts không đổi
    const events = useMemo(
        () =>
            shifts.map((shift) => ({
                id: shift.id,
                title: shift.note || "Ca làm việc", // Fallback title
                start: shift.startTime, // String ISO ok
                end: shift.endTime,
                extendedProps: shift, // Lưu toàn bộ object gốc để dùng khi render
                backgroundColor: "transparent",
                borderColor: "transparent"
            })),
        [shifts]
    )

    // 3. Memoize renderEventContent
    const renderEventContent = useCallback((eventInfo: any) => {
        return (
            <ShiftEventCard
                shift={eventInfo.event.extendedProps as ShiftResponse}
                timeText={eventInfo.timeText}
            />
        )
    }, [])

    const handleDrop = (info: any) => {
        onEventDrop(info.event.id, info.event.start, info.event.end, info)
    }

    return (
        <div className={`calendar-wrapper ${isLoading && "opacity-50 pointer-events-none"}`}>
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
                allDaySlot={false} // Tắt dòng all-day
                height="550px"
                slotDuration="01:00:00" // Mỗi ô là 1 tiếng
                expandRows={true}
                stickyHeaderDates={true}
                //===============================
                // Interaction Settings
                // Chỉ cho phép kéo thả/chỉnh size nếu không phải Mobile
                editable={!isMobile}
                droppable={!isMobile}
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

                        /* Tùy chỉnh scrollbar cho các view timeGrid (Tuần/Ngày) */
                        .fc-scroller::-webkit-scrollbar {
                            width: 10px;
                        }
                        .fc-scroller::-webkit-scrollbar-track {
                            background: transparent;
                        }
                        .fc-scroller::-webkit-scrollbar-thumb {
                            background: #cbd5e1; /* slate-300 */
                            border-radius: 10px;
                        }
                        .fc-scroller::-webkit-scrollbar-thumb:hover {
                            background: #94a3b8; /* slate-400 */
                        }

                        /* Đảm bảo sticky header hoạt động tốt khi scroll */
                        .fc .fc-scroller-harness {
                            overflow: hidden !important;
                        }
                    `
                }}
            />
        </div>
    )
}
