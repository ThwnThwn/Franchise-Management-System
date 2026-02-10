"use client"

import { useState, useMemo } from "react"
import dayjs from "dayjs"
import { CalendarApi } from "@fullcalendar/core"
import { CalendarDate } from "@heroui/react"

export type ViewMode = "dayGridMonth" | "timeGridWeek"

export const useScheduleLogic = () => {
    const [currentDate, setCurrentDate] = useState(dayjs())
    const [viewMode, setViewMode] = useState<ViewMode>("timeGridWeek")

    // 1. Tính toán Range để gọi API (Dựa trên View hiện tại)
    const dateRange = useMemo(() => {
        if (viewMode === "dayGridMonth") {
            return {
                fromDate: currentDate.startOf("month").format("YYYY-MM-DD"),
                toDate: currentDate.endOf("month").format("YYYY-MM-DD")
            }
        } else {
            // Tuần: Lấy từ T2 đến CN
            return {
                fromDate: currentDate.startOf("week").add(1, "day").format("YYYY-MM-DD"), // StartOf week là CN, +1 là T2
                toDate: currentDate.endOf("week").add(1, "day").format("YYYY-MM-DD")
            }
        }
    }, [currentDate, viewMode])

    // 2. Handlers điều hướng (Sync giữa State React và FullCalendar API)
    const handleNext = (calendarApi?: CalendarApi) => {
        const newDate =
            viewMode === "dayGridMonth" ? currentDate.add(1, "month") : currentDate.add(1, "week")

        setCurrentDate(newDate)
        calendarApi?.gotoDate(newDate.toDate()) // Bắt FullCalendar nhảy theo
    }

    const handlePrev = (calendarApi?: CalendarApi) => {
        const newDate =
            viewMode === "dayGridMonth"
                ? currentDate.subtract(1, "month")
                : currentDate.subtract(1, "week")

        setCurrentDate(newDate)
        calendarApi?.gotoDate(newDate.toDate())
    }

    const handleToday = (calendarApi?: CalendarApi) => {
        const now = dayjs()
        setCurrentDate(now)
        calendarApi?.today()
    }

    // Xử lý khi chọn Dropdown Tháng/Năm
    const handleDateSelect = (type: "year" | "month", value: number, calendarApi?: CalendarApi) => {
        let newDate = currentDate.clone()
        if (type === "year") newDate = newDate.year(value)
        if (type === "month") newDate = newDate.month(value) // 0-11

        setCurrentDate(newDate)
        calendarApi?.gotoDate(newDate.toDate())
    }

    // Xử lý khi đổi View (Tháng <-> Tuần)
    const handleViewChange = (mode: ViewMode, calendarApi?: CalendarApi) => {
        setViewMode(mode)
        calendarApi?.changeView(mode)
    }

    const handleJumpToDate = (date: CalendarDate, calendarApi?: CalendarApi) => {
        // HeroUI trả về: year, month (1-12), day
        // Dayjs nhận month (0-11)
        const newDate = dayjs()
            .year(date.year)
            .month(date.month - 1)
            .date(date.day)

        setCurrentDate(newDate)
        calendarApi?.gotoDate(newDate.toDate())
    }

    return {
        currentDate,
        viewMode,
        dateRange,
        handleNext,
        handlePrev,
        handleToday,
        handleDateSelect,
        handleViewChange,
        handleJumpToDate
    }
}
