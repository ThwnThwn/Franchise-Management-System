"use client"

import React from "react"
import { Globe } from "lucide-react"

export const SystemDefaultHint = ({
    value,
    type = "number"
}: {
    value: any
    type?: "number" | "boolean"
}) => {
    const displayValue =
        type === "boolean"
            ? value
                ? "Đang bật"
                : "Đang tắt"
            : `${value} ${type === "number" ? "giờ" : ""}`

    if (value === undefined || value === null) return null

    return (
        <div className="flex items-center gap-1.5 mt-2 px-2.5 py-1.5 bg-default-50 rounded-lg border border-default-100 w-fit">
            <Globe size={12} className="text-default-400" />
            <span className="text-[10px] font-medium text-default-500 uppercase tracking-tight">
                Mặc định hệ thống: <b className="text-default-700">{displayValue}</b>
            </span>
        </div>
    )
}
