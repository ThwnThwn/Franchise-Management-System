"use client"

import { useState, useMemo } from "react"
import dayjs from "dayjs"
import { CalendarDate } from "@internationalized/date"

export type AgendaViewMode = "list" | "calendar"

export const useAgendaLogic = () => {
    const [currentDate, setCurrentDate] = useState(dayjs())
    const [viewMode, setViewMode] = useState<AgendaViewMode>("list")

    const dateRange = useMemo(() => {
        return {
            startDate: currentDate.startOf("month").format("YYYY-MM-DD"),
            endDate: currentDate.endOf("month").format("YYYY-MM-DD")
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentDate.format("YYYY-MM")])

    // ĐIỀU HƯỚNG THÔNG MINH:
    // - List mode: Nhảy 1 ngày
    // - Calendar mode: Nhảy 1 tuần
    const handleNext = () => {
        const unit = viewMode === "list" ? "day" : "week"
        setCurrentDate((prev) => prev.add(1, unit))
    }

    const handlePrev = () => {
        const unit = viewMode === "list" ? "day" : "week"
        setCurrentDate((prev) => prev.subtract(1, unit))
    }

    const handleToday = () => setCurrentDate(dayjs())
    const handleJumpToDate = (date: CalendarDate) => setCurrentDate(dayjs(date.toString()))

    return {
        currentDate,
        viewMode,
        setViewMode,
        dateRange,
        handleNext,
        handlePrev,
        handleToday,
        handleJumpToDate
    }
}
