"use client"

import React, { useMemo } from "react"
import dayjs from "dayjs"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatusStyles } from "@/features/shifts/constants/styles"
import { TimeLogStatus } from "@/features/shifts/constants"
import { useUserHelper } from "@/hooks"
import { AlertCircle } from "lucide-react"

interface Props {
    log: TimeLogResponse
    timeText: string
}

export const TimeLogEventCard = ({ log, timeText }: Props) => {
    const { toFullName } = useUserHelper()

    // 1. Xác định trạng thái Processing
    const isProcessing = log.status === TimeLogStatus.Processing

    // 2. Logic kiểm tra quá giờ (Overdue)
    const isOverdue = useMemo(() => {
        if (!isProcessing) return false
        const { shiftDate, startTime, endTime } = log.shift
        let expEnd = dayjs(`${shiftDate}T${endTime}`)
        if (endTime < startTime) expEnd = expEnd.add(1, "day")

        // Nếu hiện tại đã vượt quá giờ kết thúc ca dự kiến
        return dayjs().isAfter(expEnd)
    }, [log.shift, isProcessing])

    // 3. Lấy style nền tảng (bg, text, border-color) từ constants
    const styleClass = TimeLogStatusStyles[log.status]

    return (
        <div
            className={`
                relative w-full h-full p-1.5 rounded-lg border-l-4 flex flex-col justify-center gap-0.5 
                shadow-sm transition-all hover:brightness-95 overflow-hidden
                ${styleClass}
                ${!isProcessing ? "event-striped-bg opacity-70 grayscale-[0.2]" : "shadow-md scale-[1.02] z-20"} 
                ${isOverdue ? "ring-2 ring-inset ring-red-500/60" : ""}
            `}
        >
            {/* Tên nhân viên - Z-index để nổi lên trên lớp sọc */}
            <div className="flex items-center gap-1 z-10">
                <span className="text-[10px] font-black uppercase truncate leading-tight flex-1">
                    {toFullName({
                        firstName: log.staff.firstName,
                        lastName: log.staff.lastName
                    })}
                </span>
                {isOverdue && (
                    <AlertCircle className="w-2.5 h-2.5 text-red-600 animate-pulse shrink-0" />
                )}
            </div>

            {/* Giờ giấc & Pulse icon cho ca đang chạy */}
            <div className="flex items-center gap-1 opacity-90 z-10">
                <span className="text-[9px] font-bold tabular-nums tracking-tighter">
                    {timeText}
                </span>
                {isProcessing && !isOverdue && (
                    <span className="relative flex h-1.5 w-1.5">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-current opacity-75"></span>
                        <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-current"></span>
                    </span>
                )}
            </div>

            {/* Chấm nhỏ báo hiệu có Manager Note */}
            {log.managerNote && (
                <div
                    title={log.managerNote}
                    className="absolute bottom-1 right-1 w-1 h-1 rounded-full bg-current opacity-40 z-10"
                />
            )}
        </div>
    )
}
