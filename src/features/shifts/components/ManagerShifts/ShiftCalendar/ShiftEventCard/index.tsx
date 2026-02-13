"use client"

import React, { memo } from "react"
import { Users, AlertCircle, CheckCircle2, Ban } from "lucide-react"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"
import { ShiftStatusLabels } from "@/features/shifts/constants/labels"
import { ShiftFullStyle, ShiftStatusStyles } from "@/features/shifts/constants/styles"

interface ShiftEventCardProps {
    shift: ShiftResponse
    timeText: string
}

// Bọc memo để tối ưu hiệu năng tối đa
export const ShiftEventCard = memo(({ shift, timeText }: ShiftEventCardProps) => {
    const isFull = shift.assignedStaffCount >= shift.requiredStaffCount
    const statusLabel = ShiftStatusLabels[shift.status]

    let statusClasses = ShiftStatusStyles[shift.status]
    if (shift.status === ShiftStatus.Published && isFull) {
        statusClasses = ShiftFullStyle
    }

    return (
        <div
            className={`w-full h-full p-1.5 rounded-md border-l-4 text-[11px] flex flex-col justify-between overflow-hidden shadow-sm transition-all ${statusClasses}`}
        >
            {/* HEADER */}
            <div className="flex items-center gap-1 font-bold text-[12px] leading-tight border-b border-black/5 pb-0.5">
                <span>{timeText}</span>
            </div>

            {/* BODY */}
            <div className="flex flex-col grow py-1 justify-center">
                <div className="flex items-center gap-1.5 mb-0.5">
                    {shift.status === ShiftStatus.Cancelled ? (
                        <Ban size={11} className="opacity-60" />
                    ) : isFull ? (
                        <CheckCircle2 size={11} className="text-success" />
                    ) : (
                        <AlertCircle size={11} />
                    )}
                    <span className="font-extrabold text-[12px] tabular-nums">
                        {shift.assignedStaffCount}/{shift.requiredStaffCount}
                    </span>
                    <Users size={11} className="opacity-70" />
                </div>
                <div className="truncate text-[10px] opacity-80 italic">
                    {shift.shiftTemplateId ? "📋 Theo mẫu" : shift.note || "📝 Ca tùy chỉnh"}
                </div>
            </div>

            {/* FOOTER */}
            <div className="flex items-center justify-between pt-0.5 border-t border-black/5">
                <span className="uppercase text-[9px] font-black tracking-widest opacity-80">
                    {statusLabel}
                </span>
                {shift.status === ShiftStatus.Published && !isFull && (
                    <span className="text-[9px] font-medium text-orange-600 animate-pulse">
                        Cần người
                    </span>
                )}
            </div>
        </div>
    )
})

ShiftEventCard.displayName = "ShiftEventCard"
