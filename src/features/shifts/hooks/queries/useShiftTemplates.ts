import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftTemplateApi } from "@/features/shifts/services/shiftTemplateApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetShiftTemplatesParams } from "@/features/shifts/models/request"

// --- QUERY HOOKS ---

export const useGetShiftTemplates = ({
    params,
    // pagination,
    enabled = true
}: {
    params: GetShiftTemplatesParams
    // pagination?: PaginationParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_TEMPLATES, params],
        queryFn: () => shiftTemplateApi.getAll({ query: params }),
        enabled
    })
}

export const useGetShiftTemplateById = ({
    id,
    enabled = true
}: {
    id: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_TEMPLATES, id],
        queryFn: () => shiftTemplateApi.getById({ id }),
        enabled
    })
}

// --- MUTATION HOOKS ---

export const useCreateShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftTemplateApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo mẫu ca làm việc thành công",
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

export const useUpdateShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftTemplateApi.update,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật mẫu ca thành công",
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

export const useDeleteShiftTemplate = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftTemplateApi.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_TEMPLATES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xoá mẫu ca làm việc",
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
