"use client"

import React, { useEffect, useMemo, useRef } from "react"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import timeGridPlugin from "@fullcalendar/timegrid"
import interactionPlugin from "@fullcalendar/interaction"
import dayjs from "dayjs"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogEventCard } from "./TimeLogEventCard"
import { ATTENDANCE_CONFIG } from "@/features/shifts/constants/config"

interface Props {
    logs: TimeLogResponse[]
    currentDate: dayjs.Dayjs
    isLoading?: boolean
    onEventClick?: (log: TimeLogResponse) => void
}

export const TimeLogCalendar = ({ logs, currentDate, isLoading, onEventClick }: Props) => {
    const calendarRef = useRef<FullCalendar>(null)

    // Đồng bộ ngày khi Toolbar thay đổi
    useEffect(() => {
        const calendarApi = calendarRef.current?.getApi()
        if (calendarApi) {
            requestAnimationFrame(() => {
                const targetDate = currentDate.toDate()
                if (calendarApi.getDate().getTime() !== targetDate.getTime()) {
                    calendarApi.gotoDate(targetDate)
                }
            })
        }
    }, [currentDate])

    // Map TimeLog sang Event của FullCalendar
    const events = useMemo(() => {
        const now = dayjs()
        const { CHECK_OUT_BUFFER_HOURS } = ATTENDANCE_CONFIG

        return logs.map((log) => {
            const { shiftDate, startTime, endTime } = log.shift

            // 1. Logic thời gian (vẫn cần để FullCalendar biết đặt card ở đâu)
            // const expStart = dayjs(`${shiftDate}T${startTime}`)
            let expEnd = dayjs(`${shiftDate}T${endTime}`)
            if (endTime < startTime) expEnd = expEnd.add(1, "day")

            let displayEnd
            if (log.checkOutAt) {
                displayEnd = log.checkOutAt
            } else {
                const bufferDeadline = expEnd.add(CHECK_OUT_BUFFER_HOURS, "hour")
                displayEnd = now.isBefore(bufferDeadline) ? now.toISOString() : expEnd.toISOString()
            }

            // 2. Trả về object tối giản
            return {
                id: log.id,
                start: log.checkInAt,
                end: displayEnd,
                // 🚩 Xóa sạch backgroundColor, borderColor, title...
                // Chỉ truyền data gốc vào extendedProps
                extendedProps: log
            }
        })
    }, [logs])

    return (
        <div
            className={`staff-calendar-container ${isLoading ? "opacity-50 pointer-events-none" : ""}`}
        >
            <FullCalendar
                ref={calendarRef}
                plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
                initialView="timeGridWeek"
                headerToolbar={false}
                events={events}
                locale="vi"
                firstDay={1}
                eventTimeFormat={{ hour: "2-digit", minute: "2-digit", hour12: false }}
                slotLabelFormat={{ hour: "2-digit", minute: "2-digit", hour12: false }}
                allDaySlot={false}
                height="550px"
                stickyHeaderDates={true}
                slotDuration="01:00:00"
                scrollTime="08:00:00"
                editable={false}
                selectable={false}
                eventContent={(eventInfo) => (
                    <TimeLogEventCard
                        log={eventInfo.event.extendedProps as TimeLogResponse}
                        timeText={eventInfo.timeText}
                    />
                )}
                eventClick={(info) => onEventClick?.(info.event.extendedProps as TimeLogResponse)}
                eventClassNames="border-none shadow-sm transition-transform active:scale-95 cursor-pointer"
            />
            {/* Giữ nguyên Style cũ của bro */}
            <style
                dangerouslySetInnerHTML={{
                    __html: `
                .fc-v-event { background-color: transparent !important; border: none !important; }
                .fc-timegrid-slot { height: 3.5rem !important; border-bottom: 1px dashed #f3f4f6 !important; }
                .fc-col-header-cell { padding: 12px 0; background: #fafafa; }
            `
                }}
            />
        </div>
    )
}
