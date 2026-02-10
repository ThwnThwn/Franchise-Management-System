"use client"

import React from "react"
import {
    Button,
    ButtonGroup,
    CalendarDate,
    Popover,
    PopoverTrigger,
    Calendar,
    PopoverContent
} from "@heroui/react"
import { ChevronLeft, ChevronRight, Calendar as CalendarIcon, CalendarDays } from "lucide-react"
import { ViewMode } from "../../hooks/useScheduleLogic"

import dayjs from "dayjs"
import isoWeek from "dayjs/plugin/isoWeek"
import { useDay } from "@/hooks"
import { ButtonIconStyled } from "@/components"

dayjs.extend(isoWeek)

interface ScheduleToolbarProps {
    currentDate: dayjs.Dayjs
    viewMode: ViewMode
    onNext: () => void
    onPrev: () => void
    onToday: () => void
    onViewChange: (mode: ViewMode) => void
    // onDateSelect: (type: "year" | "month", value: number) => void
    onJumpToDate: (date: CalendarDate) => void
}

export const ScheduleToolbar = ({
    currentDate,
    viewMode,
    onNext,
    onPrev,
    onToday,
    onViewChange,
    onJumpToDate
}: ScheduleToolbarProps) => {
    const { toDate } = useDay()

    return (
        <div className="flex flex-col md:flex-row justify-between items-center gap-4 mb-4 bg-white p-3 rounded-xl shadow-sm border border-gray-100">
            {/* LEFT: Date Picker Popover (Cách 2) */}
            <div className="flex gap-2 items-center w-full md:w-auto">
                <Popover placement="bottom-start" showArrow offset={10}>
                    <PopoverTrigger>
                        <Button
                            variant="flat"
                            className="bg-default-100 hover:bg-default-200 font-semibold text-gray-700 min-w-40"
                            startContent={<CalendarDays size={20} className="text-primary" />}
                        >
                            {/* Hiển thị dạng: Tháng 2, 2026 */}
                            <span className="capitalize">{currentDate.format("MMMM, YYYY")}</span>
                        </Button>
                    </PopoverTrigger>
                    <PopoverContent className="p-0">
                        <Calendar
                            aria-label="Select Date"
                            // Set giá trị hiện tại để calendar focus đúng ngày
                            value={toDate(currentDate) || undefined}
                            onChange={onJumpToDate}
                            // Tắt showMonthAndYearPickers nếu muốn đơn giản, hoặc bật để chọn năm nhanh
                            showMonthAndYearPickers
                        />
                    </PopoverContent>
                </Popover>
            </div>

            {/* CENTER: Navigation Title & Buttons */}
            <div className="flex items-center gap-2">
                <ButtonIconStyled size="sm" variant="light" onPress={onPrev}>
                    <ChevronLeft size={22} />
                </ButtonIconStyled>

                {/* Title hiển thị chi tiết (Tuần này là tuần mấy) */}
                <div className="flex flex-col items-center min-w-45">
                    <span className="font-bold text-lg">
                        {viewMode === "dayGridMonth"
                            ? "Lịch Tháng"
                            : `Tuần ${currentDate.isoWeek()}`}
                    </span>
                    {viewMode === "timeGridWeek" && (
                        <span className="text-xs text-gray-500 font-medium">
                            {currentDate.startOf("week").add(1, "d").format("DD/MM")} -{" "}
                            {currentDate.endOf("week").add(1, "d").format("DD/MM")}
                        </span>
                    )}
                </div>

                <ButtonIconStyled size="sm" variant="light" onPress={onNext}>
                    <ChevronRight size={22} />
                </ButtonIconStyled>
            </div>

            {/* RIGHT: View Switcher & Actions */}
            <div className="flex gap-2">
                <Button
                    size="sm"
                    variant="ghost"
                    onPress={onToday}
                    startContent={<CalendarIcon size={16} />}
                    className="hidden sm:flex"
                >
                    Hôm nay
                </Button>
                <ButtonGroup size="sm" variant="flat">
                    <Button
                        className={
                            viewMode === "dayGridMonth"
                                ? "bg-primary text-white font-medium"
                                : "font-medium"
                        }
                        onPress={() => onViewChange("dayGridMonth")}
                    >
                        Tháng
                    </Button>
                    <Button
                        className={
                            viewMode === "timeGridWeek"
                                ? "bg-primary text-white font-medium"
                                : "font-medium"
                        }
                        onPress={() => onViewChange("timeGridWeek")}
                    >
                        Tuần
                    </Button>
                </ButtonGroup>
            </div>
        </div>
    )
}
