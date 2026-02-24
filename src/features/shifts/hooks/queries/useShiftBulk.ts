import { useMutation, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftBulkApi } from "@/features/shifts/services/shiftBulkApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

// --- BULK MUTATION HOOKS ---

/**
 * Hook tạo hàng loạt ca làm việc thủ công
 */
export const useBulkCreateManualShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftBulkApi.createManual,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo hàng loạt ca làm việc thành công",
                color: "success"
            })
            onSuccess?.()
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

/**
 * Hook tạo hàng loạt ca làm việc từ Template (Mẫu)
 */
export const useBulkCreateFromTemplateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftBulkApi.createFromTemplate,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã đổ ca làm việc từ mẫu thành công",
                color: "success"
            })
            onSuccess?.()
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

/**
 * Hook công khai hàng loạt ca làm việc
 */
export const useBulkPublishShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (ids: string[]) => shiftBulkApi.publish(ids),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã chốt và công khai các ca làm việc đã chọn",
                color: "success"
            })
            onSuccess?.()
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

/**
 * Hook xóa hàng loạt ca làm việc (Chỉ xóa Draft)
 */
export const useBulkDeleteShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (ids: string[]) => shiftBulkApi.delete(ids),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã dọn dẹp các ca làm việc nháp thành công",
                color: "success"
            })
            onSuccess?.()
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
