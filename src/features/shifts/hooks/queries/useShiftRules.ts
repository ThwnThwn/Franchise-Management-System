"use client"

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { shiftRuleApi } from "@/features/shifts/services/shiftRuleApi"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { STRINGS } from "@/constants/strings"

export const useGetShiftRuleSettings = (storeId?: string) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_RULES, storeId],
        queryFn: () => shiftRuleApi.getSettings({ storeId })
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
