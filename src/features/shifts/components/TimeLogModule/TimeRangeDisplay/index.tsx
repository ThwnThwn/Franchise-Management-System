import React from "react"
import { Clock } from "lucide-react"
import { useDay } from "@/hooks"

interface Props {
    checkInAt?: string
    checkOutAt?: string
    totalWorkMinutes?: number
}

export const TimeRangeDisplay = ({ checkInAt, checkOutAt, totalWorkMinutes }: Props) => {
    const { formatDateTime } = useDay({ defaultFormat: "HH:mm" })
    console.log(checkInAt)

    return (
        <div className="flex flex-col gap-0.5">
            <div className="flex items-center gap-1.5 font-bold tabular-nums">
                <span>{formatDateTime({ date: checkInAt })}</span>
                <span className="text-default-400">-</span>
                <span>{formatDateTime({ date: checkOutAt })}</span>
            </div>
            {totalWorkMinutes !== undefined && (
                <div className="flex items-center gap-1 text-[11px] text-default-400 font-medium">
                    <Clock size={12} />
                    <span>{totalWorkMinutes} phút làm việc</span>
                </div>
            )}
        </div>
    )
}
