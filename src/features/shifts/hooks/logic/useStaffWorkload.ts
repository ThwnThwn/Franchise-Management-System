import dayjs from "dayjs"
import { useMemo } from "react"
import { ShiftResponse } from "@/features/shifts/models/response"
import { useGetShiftRuleSettings } from "@/features/shifts/hooks"
import { useUserHelper } from "@/hooks"
import { getShiftDateTime } from "@/features/shifts/utils"

// Định nghĩa Interface để "diệt" any
interface StaffStat {
    id: string
    name: string
    avatar: string
    totalHours: number
    isOverworked: boolean
}

export const useStaffWorkload = (shiftsData: ShiftResponse[], storeId: string) => {
    const { toFullName } = useUserHelper()
    const { data: ruleSettings } = useGetShiftRuleSettings({ storeId })

    const currentRule = useMemo(() => {
        const storeRule = ruleSettings?.stores.find((s) => s.storeId === storeId)
        return storeRule || ruleSettings?.global
    }, [ruleSettings, storeId])

    const maxHours = currentRule?.maxHoursPerWeek || 40

    const { staffStats, overworkCount } = useMemo(() => {
        const stats: Record<string, StaffStat> = {}

        shiftsData.forEach((shift) => {
            const start = getShiftDateTime(shift.shiftDate, shift.startTime)
            const end = getShiftDateTime(shift.shiftDate, shift.endTime, shift.startTime)

            // Tính số giờ của ca (trừ phút nghỉ)
            const duration =
                dayjs(end).diff(start, "hour", true) - (shift.breakDurationMinutes || 0) / 60

            ;(shift.assignments || []).forEach((assign) => {
                if (!stats[assign.userId]) {
                    stats[assign.userId] = {
                        id: assign.userId,
                        name: toFullName({
                            firstName: assign.firstName,
                            lastName: assign.lastName
                        }),
                        avatar: assign.avatarUrl || "",
                        totalHours: 0,
                        isOverworked: false
                    }
                }
                stats[assign.userId].totalHours += duration
            })
        })

        // Chuyển sang mảng, check quá tải và sort theo giờ làm giảm dần
        const result = Object.values(stats)
            .map((s) => ({
                ...s,
                isOverworked: s.totalHours > maxHours
            }))
            .sort((a, b) => b.totalHours - a.totalHours)

        const overCount = result.filter((s) => s.isOverworked).length

        return { staffStats: result, overworkCount: overCount }
    }, [shiftsData, maxHours, toFullName])

    return {
        staffStats,
        overworkCount,
        maxHours
    }
}
