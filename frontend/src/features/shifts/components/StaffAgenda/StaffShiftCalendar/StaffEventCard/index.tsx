"use client"

import React, { memo, useMemo } from "react"
import { StaffShiftAgenda } from "@/features/shifts/models/response"
import { Moon, CheckCircle2, Clock, PlayCircle, AlertCircle } from "lucide-react"
import dayjs from "dayjs"

export const StaffEventCard = memo(
    ({ data, timeText }: { data: StaffShiftAgenda; timeText: string }) => {
        // 1. Tính toán trạng thái dựa trên thời gian thực
        const status = useMemo(() => {
            const now = dayjs()
            const start = dayjs(`${data.date} ${data.startTime}`)
            let end = dayjs(`${data.date} ${data.endTime}`)
            if (end.isBefore(start)) end = end.add(1, "day")

            if (data.checkOutAt) return "COMPLETED"
            if (data.checkInAt && !data.checkOutAt) return "ONGOING"

            // Trường hợp chưa có hành động chấm công
            if (now.isBefore(start)) return "UPCOMING"
            const isWorkingHours =
                (now.isAfter(start) || now.isSame(start)) && (now.isBefore(end) || now.isSame(end))
            if (isWorkingHours) return "ONGOING" // Đang trong giờ làm nhưng chưa bấm nút
            return "MISSED" // Đã quá giờ mà chưa thấy check-in
        }, [data])

        // 2. Map Style theo Status
        const configs = {
            UPCOMING: {
                classes: "bg-blue-50 border-blue-400 text-blue-800",
                label: "Sắp tới",
                icon: null
            },
            ONGOING: {
                classes: "bg-emerald-50 border-emerald-500 text-emerald-800 shadow-sm",
                label: "Đang diễn ra",
                icon: <PlayCircle size={12} className="text-emerald-600 animate-pulse" />
            },
            COMPLETED: {
                classes: "bg-slate-50 border-slate-300 text-slate-700 opacity-90",
                label: "Hoàn thành",
                icon: <CheckCircle2 size={12} className="text-slate-400" />
            },
            MISSED: {
                classes: "bg-red-50 border-red-300 text-red-700",
                label: "Bỏ lỡ",
                icon: <AlertCircle size={12} className="text-red-500" />
            }
        }

        const current = configs[status]

        return (
            <div
                className={`hover:cursor-pointer w-full h-full p-1.5 rounded-md border-l-4 flex flex-col justify-between overflow-hidden transition-all duration-200 ${current.classes}`}
            >
                {/* HEADER */}
                <div className="flex items-center justify-between border-b border-black/5 pb-1">
                    <div className="flex items-center gap-1 font-bold text-[10px] tabular-nums">
                        <Clock size={10} className="opacity-60" />
                        <span>{timeText}</span>
                    </div>
                    {data.startTime > data.endTime && <Moon size={10} className="opacity-50" />}
                </div>

                {/* BODY */}
                <div className="flex flex-col grow py-1 justify-center">
                    <span className="text-[11px] font-black uppercase truncate leading-tight">
                        {data.store.name}
                    </span>
                    <span className="text-[9px] opacity-70 truncate font-medium">
                        {data.store.address}
                    </span>
                </div>

                {/* FOOTER */}
                <div className="flex items-center justify-between pt-1 border-t border-black/5">
                    <span className="text-[9px] font-black uppercase tracking-wider opacity-60">
                        {current.label}
                    </span>
                    {current.icon}
                </div>
            </div>
        )
    }
)

StaffEventCard.displayName = "StaffEventCard"
