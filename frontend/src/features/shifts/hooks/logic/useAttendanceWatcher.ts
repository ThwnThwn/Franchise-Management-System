"use client"

import { useMemo, useEffect, useState } from "react"
import dayjs from "dayjs"
import { useTokenStore, useDay } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { useActiveShiftRule, useGetCurrentShift } from "@/features/shifts/hooks"
import { AttendanceStatus } from "../../types"

export const useAttendanceWatcher = () => {
    const roleSlug = useTokenStore((s) => s.roleSlug)

    const { rule, isLoading: isRuleLoading } = useActiveShiftRule({
        enabled: roleSlug === RoleSlug.Staff
    })

    const {
        data: shift,
        isLoading: isShiftLoading,
        refetch
    } = useGetCurrentShift(!!roleSlug && roleSlug === RoleSlug.Staff)

    const isLoading = isRuleLoading || isShiftLoading
    const { dateNow } = useDay()
    const [now, setNow] = useState(dayjs())

    useEffect(() => {
        const timer = setInterval(() => setNow(dayjs()), 60000)
        return () => clearInterval(timer)
    }, [])

    const status = useMemo<AttendanceStatus | undefined>(() => {
        if (!shift || !shift.hasShiftToday) return undefined

        const grace = rule?.gracePeriodMinutes ?? 15

        const today = dateNow()
        let start = dayjs(`${today} ${shift.startTime}`)
        let end = dayjs(`${today} ${shift.endTime}`)

        if (end.isBefore(start)) {
            end = end.add(1, "day")
        }

        // 2. CÁC MỐC THỜI GIAN QUAN TRỌNG
        const checkInStart = start.subtract(grace, "minute") // CHẶN DƯỚI: Mốc sớm nhất được vào
        const checkInLateLimit = start.add(grace, "minute") // Mốc tính đi muộn

        const checkOutStart = end.subtract(grace, "minute") // Mốc sớm nhất được ra
        const checkOutDeadline = end.add(grace, "minute") // Mốc cuối cùng phải ra

        // 3. TRẠNG THÁI LOGIC
        const isOngoing = !!shift.checkInAt && !shift.checkOutAt
        const isShiftExpired = now.isAfter(checkOutDeadline)

        // canStartCheckIn: Phải sau mốc Chặn dưới VÀ chưa hết hạn ca (Không check chặn trên check-in)
        // const canStartCheckIn = !shift.checkInAt && now.isAfter(checkInStart) && !isShiftExpired

        return {
            shift,
            // isComingUp: Đã đến giờ được phép chuẩn bị vào ca (nằm trong khoảng buffer trước start)
            isComingUp: now.isAfter(checkInStart) && now.isBefore(start) && !shift.checkInAt,

            // isLate: Đã quá giờ bắt đầu + grace mà vẫn chưa quẹt thẻ (nhưng vẫn bấm được nút)
            isLate: now.isAfter(checkInLateLimit) && !shift.checkInAt && !isShiftExpired,

            isOngoing,

            // isOvertime: Đang làm và đã quá giờ kết thúc ca (vẫn trong window checkout)
            isOvertime: isOngoing && now.isAfter(end) && now.isBefore(checkOutDeadline),

            // shouldWarn:
            // - Chưa check-in: Nhắc nhở từ lúc Start cho đến khi hết hạn
            // - Đang làm: Nhắc nhở khi đã lọt vào Window Checkout [End - Grace, End + Grace]
            shouldWarn:
                (!shift.checkInAt && now.isAfter(start) && !isShiftExpired) ||
                (isOngoing && now.isAfter(checkOutStart) && !isShiftExpired),

            canCheckOut: isOngoing && !isShiftExpired,

            // Deadline là mốc cuối cùng có thể quẹt thẻ ra
            deadlineMinutes: checkOutDeadline.diff(now, "minute")
        }
    }, [shift, rule, dateNow, now])

    return { status, isLoading, refetch }
}
