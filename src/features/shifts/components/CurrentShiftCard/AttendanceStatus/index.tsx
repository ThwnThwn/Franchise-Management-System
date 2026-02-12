"use client"

import React from "react"
import { MapPin, Navigation } from "lucide-react"

/**
 * ATTENDANCE STATUS
 * Hiển thị khoảng cách thực tế so với Store
 */
interface AttendanceStatusProps {
    distanceM: number | null
    isWithinRange: boolean
    isLoading: boolean
    error?: string
}

export const AttendanceStatus = ({
    distanceM,
    isWithinRange,
    isLoading,
    error
}: AttendanceStatusProps) => {
    if (isLoading)
        return (
            <span className="text-tiny text-default-400 animate-pulse">
                Đang xác định vị trí...
            </span>
        )

    if (error)
        return (
            <div className="flex items-center gap-1 text-danger">
                <MapPin size={14} />
                <span className="text-tiny">{error}</span>
            </div>
        )

    return (
        <div
            className={`flex items-center gap-1 ${isWithinRange ? "text-success" : "text-danger"}`}
        >
            <Navigation size={14} className={isWithinRange ? "" : "animate-bounce"} />
            <span className="text-tiny font-medium">
                {distanceM !== null ? `Cách cửa hàng ${distanceM}m` : "Chưa có vị trí"}
            </span>
            <span className="text-tiny">{isWithinRange ? "(Hợp lệ)" : "(Quá xa)"}</span>
        </div>
    )
}
