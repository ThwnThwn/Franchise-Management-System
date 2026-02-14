"use client"

import React, { useEffect, useMemo, useRef } from "react"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import timeGridPlugin from "@fullcalendar/timegrid"
import interactionPlugin from "@fullcalendar/interaction"
import dayjs from "dayjs"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogEventCard } from "./TimeLogEventCard"

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
        return logs.map((log) => ({
            id: log.id,
            // Nếu đang làm (Processing) thì end time là "hiện tại" để thanh lịch kéo dài ra
            start: log.checkInAt,
            end: log.checkOutAt || dayjs().toISOString(),
            extendedProps: log
        }))
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
