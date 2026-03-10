"use client"

import React from "react"
import {
    Tabs,
    Tab,
    Input,
    Button,
    Popover,
    PopoverTrigger,
    PopoverContent,
    Calendar as HeroCalendar,
    ButtonGroup
} from "@heroui/react"
import {
    CalendarDays,
    Search,
    Calendar as CalendarIcon,
    ChevronLeft,
    ChevronRight,
    List
} from "lucide-react"
import dayjs from "dayjs"
import { ButtonIconStyled } from "@/components"
import { TimeLogStatus } from "@/features/shifts/constants"
import { TimeLogStatusLabels } from "@/features/shifts/constants/labels"
import { useDay } from "@/hooks"
import { DayOfWeekLabel } from "@/constants/labels"
import { DayOfWeek } from "@/constants/enums"

interface Props {
    isManager: boolean
    currentDate: dayjs.Dayjs
    status?: TimeLogStatus
    onStatusChange: (status: any) => void
    viewMode: "list" | "calendar"
    onViewChange: (mode: "list" | "calendar") => void
    onNext: () => void
    onPrev: () => void
    onToday: () => void
    onJump: (date: any) => void
    // Chỉ Manager mới dùng cái này
    staffSearch?: string
    onStaffSearchChange?: (val: string) => void
}

export const TimeLogToolbar = (props: Props) => {
    const {
        isManager,
        currentDate,
        status,
        onStatusChange,
        viewMode,
        onViewChange,
        onNext,
        onPrev,
        onToday,
        onJump,
        staffSearch,
        onStaffSearchChange
    } = props

    const { toDate } = useDay()

    return (
        <div className="flex flex-col gap-4 bg-white p-4 rounded-3xl shadow-sm border border-divider">
            {/* Hàng 1: Điều hướng thời gian & View Mode */}
            <div className="flex flex-col md:flex-row justify-between items-center gap-4">
                <div className="flex gap-2 items-center w-full md:w-auto">
                    <Popover placement="bottom-start" showArrow offset={10}>
                        <PopoverTrigger>
                            <Button
                                variant="flat"
                                className="bg-default-100 hover:bg-default-200 font-semibold text-gray-700 min-w-40"
                                startContent={<CalendarDays size={20} className="text-primary" />}
                            >
                                <span className="capitalize">
                                    {currentDate.format("MMMM, YYYY")}
                                </span>
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="p-0">
                            <HeroCalendar
                                aria-label="Jump"
                                value={toDate(currentDate) || undefined}
                                onChange={onJump}
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

                <div className="flex items-center gap-4">
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

                <ButtonGroup size="sm" className="hidden md:flex bg-default-100 p-1 rounded-xl">
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
                </ButtonGroup>
            </div>

            {/* Hàng 2: Lọc Status & Search Staff (Chỉ Manager thấy hoặc hiện giới hạn cho Staff) */}
            <div className="flex flex-col xl:flex-row justify-between items-center gap-4 pt-4 border-t border-divider border-dashed">
                <Tabs
                    fullWidth
                    selectedKey={status?.toString() || "all"}
                    onSelectionChange={(key) =>
                        onStatusChange(key === "all" ? undefined : Number(key))
                    }
                    variant="underlined"
                    color="primary"
                    classNames={{ tabList: "gap-6", cursor: "w-full", tab: "max-w-fit px-0 h-8" }}
                >
                    <Tab key="all" title="Tất cả" />
                    <Tab
                        key={TimeLogStatus.Processing.toString()}
                        title={TimeLogStatusLabels[TimeLogStatus.Processing]}
                    />
                    <Tab
                        key={TimeLogStatus.Valid.toString()}
                        title={TimeLogStatusLabels[TimeLogStatus.Valid]}
                    />
                    <Tab
                        key={TimeLogStatus.Invalid.toString()}
                        title={TimeLogStatusLabels[TimeLogStatus.Invalid]}
                    />
                </Tabs>

                {isManager && (
                    <Input
                        isClearable
                        className="w-full xl:max-w-72"
                        placeholder="Tìm theo tên nhân viên..."
                        startContent={<Search size={18} className="text-default-400" />}
                        value={staffSearch}
                        onValueChange={onStaffSearchChange}
                        variant="flat"
                    />
                )}
            </div>
        </div>
    )
}
