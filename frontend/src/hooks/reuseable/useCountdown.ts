import { useState, useEffect } from "react"
import dayjs from "dayjs"

export const useCountdown = (targetDate: string | Date | null | undefined) => {
    const [timeLeft, setTimeLeft] = useState({
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0,
        isExpired: true
    })

    useEffect(() => {
        if (!targetDate) return

        const timer = setInterval(() => {
            const now = dayjs()
            const target = dayjs(targetDate)
            const diff = target.diff(now)

            if (diff <= 0) {
                setTimeLeft((prev) => ({ ...prev, isExpired: true }))
                clearInterval(timer)
            } else {
                setTimeLeft({
                    days: Math.floor(diff / (1000 * 60 * 60 * 24)),
                    hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
                    minutes: Math.floor((diff / 1000 / 60) % 60),
                    seconds: Math.floor((diff / 1000) % 60),
                    isExpired: false
                })
            }
        }, 1000)

        return () => clearInterval(timer)
    }, [targetDate])

    return timeLeft
}
