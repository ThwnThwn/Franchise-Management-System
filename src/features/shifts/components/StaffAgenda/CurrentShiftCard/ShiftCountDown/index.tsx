"use client"

import React, { useState, useEffect } from "react"
import { Clock } from "lucide-react"
import dayjs from "dayjs"

interface ShiftCountDownProps {
    targetTime: dayjs.Dayjs // ISO string từ server
    label: string
}

export const ShiftCountDown = ({ targetTime, label }: ShiftCountDownProps) => {
    const [timeLeft, setTimeLeft] = useState<string>("--:--:--")

    useEffect(() => {
        const timer = setInterval(() => {
            const now = dayjs()
            const diff = targetTime.diff(now)

            if (diff <= 0) {
                setTimeLeft("00:00:00")
                clearInterval(timer)
            } else {
                const hours = Math.floor(diff / (1000 * 60 * 60))
                    .toString()
                    .padStart(2, "0")
                const mins = Math.floor((diff / (1000 * 60)) % 60)
                    .toString()
                    .padStart(2, "0")
                const secs = Math.floor((diff / 1000) % 60)
                    .toString()
                    .padStart(2, "0")
                setTimeLeft(`${hours}:${mins}:${secs}`)
            }
        }, 1000)

        return () => clearInterval(timer)
    }, [targetTime])

    return (
        <div className="flex flex-col items-center p-3 rounded-2xl border border-secondary">
            <span className="text-[10px] uppercase tracking-wider text-secondary font-bold">
                {label}
            </span>
            <div className="flex items-center gap-2 text-primary font-mono text-xl font-black">
                <Clock size={18} />
                {timeLeft}
            </div>
        </div>
    )
}
