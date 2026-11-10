"use client"

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { shiftRuleApi } from "@/features/shifts/services/shiftRuleApi"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { STRINGS } from "@/constants/strings"
import { useTokenStore } from "@/hooks"
import { ShiftRuleResponse } from "../../models/response"
import { useMemo } from "react"

export const useGetShiftRuleSettings = ({
    storeId,
    enabled = true
}: {
    storeId?: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_RULES, storeId],
        queryFn: () => shiftRuleApi.getSettings({ storeId }),
        enabled: enabled,
        staleTime: 30 * 60 * 1000,
        refetchOnMount: false,
        refetchOnWindowFocus: false
    })
}

export const useSaveShiftRule = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: shiftRuleApi.save,
        onSuccess: () => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thành công",
                color: "success"
            })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_RULES] })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

const DEFAULT_SHIFT_RULE: Partial<ShiftRuleResponse> = {
    allowShuttle: false,
    allowShiftSwap: true,
    minHoursBetweenShifts: 8,
    maxHoursPerWeek: 48,
    gracePeriodMinutes: 15,
    dailyOtThresholdHours: 8,
    weeklyOtThresholdHours: 40
}

export const useActiveShiftRule = (
    { enabled = true }: { enabled?: boolean } = { enabled: true }
) => {
    const storeId = useTokenStore((s) => s.storeId) || undefined

    const { data, isLoading, isError, refetch } = useGetShiftRuleSettings({
        storeId: storeId,
        enabled: enabled
    })

    const activeRule = useMemo(() => {
        // Nếu chưa có data thì dùng hàng "phòng thủ" Default
        if (!data) return DEFAULT_SHIFT_RULE as ShiftRuleResponse

        // ƯU TIÊN: Cửa hàng (Store) > Hệ thống (Global)
        const rule = data.stores?.[0] || data.global

        return {
            ...rule, // Spread toàn bộ field từ BE (id, updatedAt,...)
            // Dùng Nullish Coalescing để đảm bảo ko bao giờ bị undefined các field quan trọng
            allowShuttle: rule?.allowShuttle ?? DEFAULT_SHIFT_RULE.allowShuttle,
            allowShiftSwap: rule?.allowShiftSwap ?? DEFAULT_SHIFT_RULE.allowShiftSwap,
            minHoursBetweenShifts:
                rule?.minHoursBetweenShifts ?? DEFAULT_SHIFT_RULE.minHoursBetweenShifts,
            maxHoursPerWeek: rule?.maxHoursPerWeek ?? DEFAULT_SHIFT_RULE.maxHoursPerWeek,
            gracePeriodMinutes: rule?.gracePeriodMinutes ?? DEFAULT_SHIFT_RULE.gracePeriodMinutes,
            dailyOtThresholdHours:
                rule?.dailyOtThresholdHours ?? DEFAULT_SHIFT_RULE.dailyOtThresholdHours,
            weeklyOtThresholdHours:
                rule?.weeklyOtThresholdHours ?? DEFAULT_SHIFT_RULE.weeklyOtThresholdHours
        } as ShiftRuleResponse
    }, [data])

    return {
        rule: activeRule,
        isLoading,
        isError,
        refetch
    }
}
