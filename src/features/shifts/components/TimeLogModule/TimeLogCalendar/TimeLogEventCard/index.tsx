"use client"

import React from "react"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatusStyles } from "@/features/shifts/constants/styles"
import { useUserHelper } from "@/hooks"

interface Props {
    log: TimeLogResponse
    timeText: string
}

export const TimeLogEventCard = ({ log, timeText }: Props) => {
    const { toFullName } = useUserHelper()
    const styleClass = TimeLogStatusStyles[log.status]

    return (
        <div
            className={`
                w-full h-full p-1.5 rounded-lg border-l-4 flex flex-col justify-center gap-0.5 
                shadow-sm transition-all hover:brightness-95
                ${styleClass}
            `}
        >
            {/* Tên nhân viên - Tự động kế thừa màu chữ từ styleClass */}
            <span className="text-[10px] font-black uppercase truncate leading-tight">
                {toFullName({
                    firstName: log.staff.firstName,
                    lastName: log.staff.lastName
                })}
            </span>

            {/* Giờ giấc - Cho mờ đi một chút (opacity) để tạo độ sâu phân cấp */}
            <div className="flex items-center gap-1 opacity-80">
                <span className="text-[9px] font-bold tabular-nums tracking-tighter">
                    {timeText}
                </span>
            </div>

            {/* Nếu có Manager Note thì hiện một dấu chấm nhỏ để sếp biết mà click vào soi */}
            {log.managerNote && (
                <div className="absolute top-1 right-1 w-1 h-1 rounded-full bg-current opacity-50" />
            )}
        </div>
    )
}
