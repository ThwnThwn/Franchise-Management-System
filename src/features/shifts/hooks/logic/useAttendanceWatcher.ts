"use client"

import { useMemo, useEffect, useState } from "react"
import dayjs from "dayjs"
import { useTokenStore, useDay } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { ATTENDANCE_CONFIG } from "@/features/shifts/constants/config"
import { useGetCurrentShift } from "@/features/shifts/hooks"

export const useAttendanceWatcher = () => {
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const { dateNow } = useDay()
    const {
        data: shift,
        isLoading,
        refetch
    } = useGetCurrentShift(!!roleSlug && roleSlug === RoleSlug.Staff)
    const [now, setNow] = useState(dayjs())

    useEffect(() => {
        const timer = setInterval(() => setNow(dayjs()), 60000)
        return () => clearInterval(timer)
    }, [])

    const status = useMemo(() => {
        if (!shift || !shift.hasShiftToday) return null

        const today = dateNow()
        let start = dayjs(`${today} ${shift.startTime}`)
        let end = dayjs(`${today} ${shift.endTime}`)

        // 1. Xử lý ca xuyên đêm (Vít lỗi End < Start)
        if (end.isBefore(start)) {
            end = end.add(1, "day")
        }

        // 2. XỬ LÝ BẪY NỬA ĐÊM:
        // Nếu bây giờ là 00:40 và ca làm là 22:00 - 06:00
        // Lúc này 'start' (22:00 hôm nay) đang ở tương lai, nhưng thực tế ca đã bắt đầu từ hôm qua.
        // Điều kiện: Nếu (Bây giờ < End) VÀ (Bây giờ < Start) VÀ (Ca này là ca xuyên đêm)
        if (now.isBefore(start) && now.isBefore(end) && shift.endTime < shift.startTime) {
            start = start.subtract(1, "day")
            end = end.subtract(1, "day")
        }

        // 3. NGƯỠNG THỜI GIAN
        const checkInThreshold = start.subtract(ATTENDANCE_CONFIG.CHECK_IN_BUFFER_MINUTES, "minute")
        const checkOutDeadline = end.add(ATTENDANCE_CONFIG.CHECK_OUT_BUFFER_HOURS, "hour")

        // 4. TRẠNG THÁI LOGIC
        const isOngoing = !!shift.checkInAt && !shift.checkOutAt
        const isShiftExpired = now.isAfter(checkOutDeadline)
        const canStartCheckIn = now.isAfter(checkInThreshold)

        return {
            shift,
            isComingUp: now.isBefore(start) && canStartCheckIn,
            isLate: now.isAfter(start.add(15, "minute")) && !shift.checkInAt && !isShiftExpired,
            isOngoing,
            isOvertime: isOngoing && now.isAfter(end),
            shouldWarn: (!shift.checkInAt && canStartCheckIn && !isShiftExpired) || isOngoing,
            deadlineMinutes: checkOutDeadline.diff(now, "minute")
        }
    }, [shift, dateNow, now])

    return { status, isLoading, refetch }
}
