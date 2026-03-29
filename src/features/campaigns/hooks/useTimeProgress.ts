import { useState, useEffect, useMemo } from "react"
import dayjs from "dayjs"

/**
 * Hook tính toán % thời gian đã trôi qua giữa 2 mốc
 * @param startDate Ngày bắt đầu (ISO String / Date)
 * @param endDate Ngày kết thúc (ISO String / Date)
 * @param interval Khoảng thời gian cập nhật lại (mặc định 1000ms = 1s)
 */
export const useTimeProgress = (
    startDate: string | Date | undefined,
    endDate: string | Date | undefined,
    interval: number = 1000
) => {
    // Dùng state để trigger re-render mỗi giây (giúp thanh progress nhích mượt mà)
    const [now, setNow] = useState(dayjs().unix())

    useEffect(() => {
        if (!endDate) return

        const timer = setInterval(() => {
            setNow(dayjs().unix())
        }, interval)

        return () => clearInterval(timer)
    }, [endDate, interval])

    const progressValue = useMemo(() => {
        if (!startDate || !endDate) return 0

        const start = dayjs(startDate).unix()
        const end = dayjs(endDate).unix()

        const total = end - start
        const elapsed = now - start

        if (total <= 0) return 100 // Tránh chia cho 0 hoặc lỗi logic ngày

        const percentage = (elapsed / total) * 100

        // Giới hạn trong khoảng [0, 100]
        return Math.min(Math.max(percentage, 0), 100)
    }, [startDate, endDate, now])

    return progressValue
}
