import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftApi } from "@/features/shifts/services/shiftApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import {
    GetShiftsParams,
    GetTimeLogsParams,
    UpdateTimeLogRequest
} from "@/features/shifts/models/request"
import { PaginationParams } from "@/models/common/request"

// --- QUERY HOOKS (GET DATA) ---

export const useGetShifts = ({
    params,
    enabled = true
}: {
    params: GetShiftsParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, params],
        queryFn: () => shiftApi.getAll({ query: params }),
        enabled
    })
}

export const useGetShiftById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, id],
        queryFn: () => shiftApi.getById({ id }),
        enabled
    })
}

// --- MUTATION HOOKS (ACTION DATA) ---

// [Story 1] Create
export const useCreateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.create,
        onSuccess: () => {
            // Refresh lại danh sách shift sau khi tạo mới
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo ca làm việc thành công",
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

export const useUpdateShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.update,
        onSuccess: () => {
            // Refresh toàn bộ list và detail của shift vừa sửa
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật ca làm việc thành công",
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

export const useDeleteShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xoá ca làm việc",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail, // VD: Cannot delete shift in progress...
                color: "danger"
            })
        }
    })
}

export const usePublishShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.publish,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xuất bản ca làm việc",
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
 * Hook dành cho Manager quản lý danh sách chấm công của nhân viên
 */
export const useGetAllTimeLogs = ({
    query,
    pagination,
    enabled
}: {
    query: GetTimeLogsParams
    pagination: PaginationParams
    enabled: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFTS, QUERY_KEYS.TIME_LOGS, query, pagination],
        queryFn: () => shiftApi.getAllTimeLogs({ query, pagination }),
        enabled: enabled && !!query.storeId, // Chỉ fetch khi đã chọn Store
        staleTime: 1000 * 30, // 30 giây để đảm bảo Manager thấy data mới nhất
        placeholderData: (previousData) => previousData
    })
}

/**
 * Dùng để Manager phê duyệt, bác bỏ hoặc để lại ghi chú.
 */
export const useUpdateTimeLog = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateTimeLogRequest }) =>
            shiftApi.updateTimeLog(id, req),
        onSuccess: () => {
            // Làm tươi toàn bộ cache liên quan đến Time Logs
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.SHIFTS, QUERY_KEYS.TIME_LOGS],
                exact: false
            })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật chấm công thành công!",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Không thể cập nhật. Vui lòng thử lại.",
                color: "danger"
            })
        }
    })
}
