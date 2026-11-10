"use client"

import React from "react"
import {
    Button,
    ButtonGroup,
    Popover,
    PopoverTrigger,
    PopoverContent,
    Calendar as HeroCalendar
} from "@heroui/react"
import {
    ChevronLeft,
    ChevronRight,
    CalendarDays,
    List,
    Calendar as CalendarIcon
} from "lucide-react"
import dayjs from "dayjs"
import { useDay } from "@/hooks"
import { AgendaViewMode } from "@/features/shifts/hooks"
import { ButtonIconStyled } from "@/components"
import { DayOfWeek } from "@/constants/enums"
import { DayOfWeekLabel } from "@/constants/labels"

interface AgendaToolbarProps {
    currentDate: dayjs.Dayjs
    viewMode: AgendaViewMode
    onViewChange: (mode: AgendaViewMode) => void
    onNext: () => void
    onPrev: () => void
    onToday: () => void
    onJump: (date: any) => void
}

export const AgendaToolbar = ({
    currentDate,
    viewMode,
    onViewChange,
    onNext,
    onPrev,
    onToday,
    onJump
}: AgendaToolbarProps) => {
    const { toDate } = useDay()

    return (
        <div className="flex flex-col md:flex-row justify-between items-center gap-4 bg-white p-3 rounded-2xl shadow-sm border border-divider">
            <div className="flex gap-2 items-center w-full xl:w-auto justify-between md:justify-start">
                <Popover placement="bottom-start" showArrow offset={10}>
                    <PopoverTrigger>
                        <Button
                            variant="flat"
                            className="bg-default-100 hover:bg-default-200 font-semibold text-gray-700 min-w-40"
                            startContent={<CalendarDays size={20} className="text-primary" />}
                        >
                            <span className="capitalize">{currentDate.format("MMMM, YYYY")}</span>
                        </Button>
                    </PopoverTrigger>
                    <PopoverContent className="p-0">
                        <HeroCalendar
                            aria-label="Jump to date"
                            value={toDate(currentDate) || undefined}
                            onChange={onJump}
                            showMonthAndYearPickers
                            firstDayOfWeek="mon"
                        />
                    </PopoverContent>
                </Popover>

                {/* Nút Today dành cho Mobile ẩn đi, Desktop hiện */}
                <Button
                    size="sm"
                    variant="ghost"
                    onPress={onToday}
                    startContent={<CalendarIcon size={16} />}
                    className="md:flex hidden"
                >
                    Hôm nay
                </Button>
            </div>

            <div className="flex items-center gap-2">
                <ButtonIconStyled size="sm" variant="light" onPress={onPrev}>
                    <ChevronLeft size={22} />
                </ButtonIconStyled>

                <div className="flex flex-col items-center min-w-36 text-center">
                    {viewMode === "list" ? (
                        <>
                            <span className="font-extrabold text-lg leading-tight capitalize text-primary">
                                {DayOfWeekLabel[currentDate.day() as DayOfWeek]}
                            </span>
                            <span className="text-[11px] text-gray-500 font-bold tracking-tight">
                                Ngày {currentDate.format("DD/MM/YYYY")}
                            </span>
                        </>
                    ) : (
                        <>
                            <span className="font-bold text-lg leading-tight">
                                {`Tuần ${currentDate.isoWeek()}`}
                            </span>
                            <span className="text-[10px] text-gray-500 font-bold uppercase tracking-wider">
                                {currentDate.startOf("isoWeek").format("DD/MM")} -{" "}
                                {currentDate.endOf("isoWeek").format("DD/MM")}
                            </span>
                        </>
                    )}
                </div>

                <ButtonIconStyled size="sm" variant="light" onPress={onNext}>
                    <ChevronRight size={22} />
                </ButtonIconStyled>
            </div>

            {/* Switcher: Chỉ hiện trên Desktop (md:flex) */}
            <ButtonGroup size="sm" className="hidden md:flex bg-default-100 p-1 rounded-xl">
                <Button
                    startContent={<CalendarIcon size={16} />}
                    className={
                        viewMode === "calendar"
                            ? "bg-white shadow-sm font-bold text-primary"
                            : "bg-transparent"
                    }
                    onPress={() => onViewChange("calendar")}
                >
                    Theo tuần
                </Button>
                <Button
                    startContent={<List size={16} />}
                    className={
                        viewMode === "list"
                            ? "bg-white shadow-sm font-bold text-primary"
                            : "bg-transparent"
                    }
                    onPress={() => onViewChange("list")}
                >
                    Theo ngày
                </Button>
            </ButtonGroup>
        </div>
    )
}
