"use client"

import React from "react"
import {
    Button,
    ButtonGroup,
    CalendarDate,
    Popover,
    PopoverTrigger,
    Calendar,
    PopoverContent,
    Badge,
    Divider,
    Tooltip
} from "@heroui/react"
import {
    ChevronLeft,
    ChevronRight,
    Calendar as CalendarIcon,
    CalendarDays,
    Trash2,
    CopyPlus,
    Send,
    Globe
} from "lucide-react"

import dayjs from "dayjs"
import isoWeek from "dayjs/plugin/isoWeek"
import { useDay } from "@/hooks"
import { ButtonIconStyled } from "@/components"
// import { ShiftResponse } from "@/features/shifts/models/response"
import { ViewMode } from "@/features/shifts/hooks"
// import { ShiftStatus } from "@/features/shifts/constants"

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

    forDeleteCount: number
    forPublishCount: number
    forOpenCount: number

    onBulkCreate: () => void
    onBulkPublish: () => Promise<void>
    onBulkDelete: () => Promise<void>
    onBulkOpen: () => Promise<void>
    isBulkProcessing: boolean
}

export const ScheduleToolbar = ({
    currentDate,
    viewMode,
    onNext,
    onPrev,
    onToday,
    onViewChange,
    onJumpToDate,
    //
    forDeleteCount,
    forPublishCount,
    forOpenCount,
    //
    onBulkCreate,
    onBulkPublish,
    onBulkDelete,
    onBulkOpen,
    isBulkProcessing
}: ScheduleToolbarProps) => {
    const { toDate } = useDay()

    return (
        <div className="flex flex-col xl:flex-row justify-between items-center gap-4 bg-white p-3 rounded-xl shadow-sm border border-gray-100">
            {/* LEFT: Date Picker */}
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
                        <Calendar
                            aria-label="Select Date"
                            value={toDate(currentDate) || undefined}
                            onChange={onJumpToDate}
                            showMonthAndYearPickers
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

            {/* CENTER: Navigation */}
            <div className="flex items-center gap-2">
                <ButtonIconStyled size="sm" variant="light" onPress={onPrev}>
                    <ChevronLeft size={22} />
                </ButtonIconStyled>

                <div className="flex flex-col items-center min-w-30">
                    <span className="font-bold text-lg leading-tight">
                        {viewMode === "dayGridMonth"
                            ? "Lịch Tháng"
                            : `Tuần ${currentDate.isoWeek()}`}
                    </span>
                    {viewMode === "timeGridWeek" && (
                        <span className="text-[10px] text-gray-500 font-bold uppercase tracking-wider">
                            {currentDate.startOf("week").add(1, "d").format("DD/MM")} -{" "}
                            {currentDate.endOf("week").add(1, "d").format("DD/MM")}
                        </span>
                    )}
                </div>

                <ButtonIconStyled size="sm" variant="light" onPress={onNext}>
                    <ChevronRight size={22} />
                </ButtonIconStyled>
            </div>

            {/* RIGHT: View Switcher & Bulk Actions */}
            <div className="flex items-center gap-3 w-full xl:w-auto justify-center md:justify-end">
                {/* View Switcher */}
                <ButtonGroup size="sm" variant="flat" className="bg-default-100 rounded-lg p-1">
                    <Button
                        className={
                            viewMode === "timeGridWeek"
                                ? "bg-white shadow-sm text-primary font-bold"
                                : "bg-transparent"
                        }
                        onPress={() => onViewChange("timeGridWeek")}
                    >
                        Tuần
                    </Button>
                    <Button
                        className={
                            viewMode === "dayGridMonth"
                                ? "bg-white shadow-sm text-primary font-bold"
                                : "bg-transparent"
                        }
                        onPress={() => onViewChange("dayGridMonth")}
                    >
                        Tháng
                    </Button>
                </ButtonGroup>

                <Divider orientation="vertical" className="h-8 hidden md:block" />

                {/* BULK ACTIONS GROUP */}
                <div className="flex items-center gap-2">
                    {/* 1. Nút Tạo Hàng Loạt */}
                    <Button
                        size="sm"
                        color="primary"
                        onPress={onBulkCreate}
                        startContent={<CopyPlus size={18} />}
                        className="font-bold"
                    >
                        Tạo nhanh
                    </Button>

                    {/* 2. Nút Xoá Nháp */}
                    <Badge
                        content={forDeleteCount}
                        color="danger"
                        shape="circle"
                        size="sm"
                        isInvisible={forDeleteCount <= 0}
                    >
                        <Tooltip
                            content={`Xóa ${forDeleteCount} ca nháp`}
                            color="danger"
                            placement="top"
                        >
                            <Button
                                size="sm"
                                color="danger"
                                variant="flat"
                                isIconOnly
                                onPress={onBulkDelete}
                                isDisabled={isBulkProcessing || forDeleteCount <= 0}
                            >
                                <Trash2 size={18} />
                            </Button>
                        </Tooltip>
                    </Badge>

                    {/* 3. Nút Đẩy Lên Chợ Ca (Open) */}
                    <Badge
                        content={forOpenCount || forDeleteCount}
                        color="warning"
                        shape="circle"
                        size="sm"
                        isInvisible={(forOpenCount || forDeleteCount) <= 0}
                    >
                        <Tooltip
                            content={`Mở ${forOpenCount || forDeleteCount} ca ra Chợ`}
                            color="warning"
                            placement="top"
                        >
                            <Button
                                size="sm"
                                color="warning"
                                variant="flat"
                                isIconOnly
                                onPress={onBulkOpen}
                                isDisabled={
                                    isBulkProcessing || (forOpenCount || forDeleteCount) <= 0
                                }
                            >
                                <Globe size={18} className="text-warning-700" />
                            </Button>
                        </Tooltip>
                    </Badge>

                    {/* 4. Nút Chốt Lịch (Publish) */}
                    <Badge
                        content={forPublishCount}
                        color="danger"
                        shape="circle"
                        size="sm"
                        isInvisible={forPublishCount <= 0}
                    >
                        <Tooltip
                            content={`Chốt ${forPublishCount} ca làm`}
                            color="success"
                            placement="top"
                        >
                            <Button
                                size="sm"
                                color="success"
                                className="text-white shadow-sm"
                                isIconOnly
                                onPress={onBulkPublish}
                                isLoading={isBulkProcessing}
                                isDisabled={isBulkProcessing || forPublishCount <= 0}
                            >
                                {!isBulkProcessing && <Send size={18} />}
                            </Button>
                        </Tooltip>
                    </Badge>
                </div>
            </div>
        </div>
    )
}
