"use client"

import React, { useEffect, useMemo, useRef } from "react"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import timeGridPlugin from "@fullcalendar/timegrid"
import interactionPlugin from "@fullcalendar/interaction"
import { StaffShiftAgenda } from "@/features/shifts/models/response"
import { mapStaffAgendaToEvents } from "@/features/shifts/utils"
import { StaffEventCard } from "./StaffEventCard"
import dayjs from "dayjs"

interface StaffShiftCalendarProps {
    agenda: StaffShiftAgenda[]
    currentDate: dayjs.Dayjs // Thêm prop này để đồng bộ
    isLoading?: boolean
    onEventClick?: (agenda: StaffShiftAgenda) => void
}

export const StaffShiftCalendar = ({
    agenda,
    currentDate, // Nhận currentDate từ hook useAgendaLogic
    isLoading,
    onEventClick
}: StaffShiftCalendarProps) => {
    // 1. Tạo Ref để "túm" lấy cái instance của FullCalendar
    const calendarRef = useRef<FullCalendar>(null)

    // 2. "Vít" logic đồng bộ: Cứ currentDate đổi là bắt lịch nhảy theo
    useEffect(() => {
        const calendarApi = calendarRef.current?.getApi()

        if (calendarApi) {
            // Sử dụng requestAnimationFrame hoặc setTimeout(..., 0)
            // để đẩy việc update lịch sang một "micro-task" khác
            requestAnimationFrame(() => {
                const targetDate = currentDate.toDate()

                // Chỉ gotoDate nếu ngày thực sự khác biệt để tránh loop vô tận
                if (calendarApi.getDate().getTime() !== targetDate.getTime()) {
                    calendarApi.gotoDate(targetDate)
                }
            })
        }
    }, [currentDate])
    const events = useMemo(() => mapStaffAgendaToEvents(agenda), [agenda])

    return (
        <div
            className={`staff-calendar-container ${isLoading ? "opacity-50 pointer-events-none" : ""}`}
        >
            <FullCalendar
                ref={calendarRef}
                plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
                initialView="timeGridWeek"
                headerToolbar={false} // Dùng Toolbar tự chế ở ngoài
                events={events}
                locale="vi"
                firstDay={1} // Thứ 2 là đầu tuần
                // Cấu hình hiển thị 24h
                eventTimeFormat={{ hour: "2-digit", minute: "2-digit", hour12: false }}
                slotLabelFormat={{ hour: "2-digit", minute: "2-digit", hour12: false }}
                // Cấu hình khung nhìn
                allDaySlot={false}
                height="550px"
                stickyHeaderDates={true}
                slotDuration="01:00:00"
                scrollTime="08:00:00"
                // STAFF ONLY: Disable all editing features
                editable={false}
                selectable={false}
                // Render nội dung tùy chỉnh
                eventContent={(eventInfo) => (
                    <StaffEventCard
                        data={eventInfo.event.extendedProps as StaffShiftAgenda}
                        timeText={eventInfo.timeText}
                    />
                )}
                // Xử lý sự kiện click để xem chi tiết ca
                eventClick={(info) => onEventClick?.(info.event.extendedProps as StaffShiftAgenda)}
                eventClassNames="border-none shadow-sm transition-transform active:scale-95"
            />

            <style
                dangerouslySetInnerHTML={{
                    __html: `
                .fc-v-event { background-color: transparent !important; border: none !important; }
                .fc-timegrid-slot { height: 3.5rem !important; border-bottom: 1px dashed #f3f4f6 !important; }
                .fc-col-header-cell { padding: 12px 0; background: #fafafa; }
                .fc-timegrid-col-events { margin: 0 2px !important; }
            `
                }}
            />
        </div>
    )
}
