"use client"

import React from "react"
import {
    Button,
    Popover,
    PopoverTrigger,
    PopoverContent,
    Calendar as HeroCalendar
} from "@heroui/react"
import { ChevronLeft, ChevronRight, CalendarDays, Calendar as CalendarIcon } from "lucide-react"
import dayjs from "dayjs"
import { useDay } from "@/hooks"
import { ButtonIconStyled } from "@/components"
import { DayOfWeek } from "@/constants/enums"
import { DayOfWeekLabel } from "@/constants/labels"

interface ShiftPoolToolbarProps {
    currentDate: dayjs.Dayjs
    onNext: () => void
    onPrev: () => void
    onToday: () => void
    onJump: (date: any) => void
}

export const ShiftPoolToolbar = ({
    currentDate,
    onNext,
    onPrev,
    onToday,
    onJump
}: ShiftPoolToolbarProps) => {
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
                        />
                    </PopoverContent>
                </Popover>

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
                    <span className="font-extrabold text-lg leading-tight capitalize text-primary">
                        {DayOfWeekLabel[currentDate.day() as DayOfWeek]}
                    </span>
                    <span className="text-[11px] text-gray-500 font-bold tracking-tight">
                        Ngày {currentDate.format("DD/MM/YYYY")}
                    </span>
                </div>

                <ButtonIconStyled size="sm" variant="light" onPress={onNext}>
                    <ChevronRight size={22} />
                </ButtonIconStyled>
            </div>

            {/* Giữ bố cục cân đối bằng 1 block ẩn nếu cần, hoặc để tự do */}
            {/* <div className="hidden md:block min-w-30"></div> */}
        </div>
    )
}
