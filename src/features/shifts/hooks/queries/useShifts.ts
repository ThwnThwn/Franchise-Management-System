import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftApi } from "@/features/shifts/services/shiftApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetShiftsParams } from "@/features/shifts/models/request"
import { AutoFillShiftsResponse } from "../../models/response"

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

export const useAutoFillShifts = ({
    onSuccess
}: {
    onSuccess?: (data: AutoFillShiftsResponse) => void
} = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.autoFill,
        onSuccess: (data) => {
            // 1. Refresh lại toàn bộ danh sách ca làm để cập nhật trạng thái đã gán người
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            // 2. Bắn toast báo cáo kết quả (ví dụ: "Đã xếp được 10/12 ca")
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: `Tự động xếp ca thành công! Đã điền ${data.filledCount}/${data.totalSlots} vị trí.`,
                color: "success"
            })

            // 3. Callback để đóng Modal hoặc xử lý thêm tại UI
            onSuccess?.(data)
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Không thể thực hiện tự động xếp ca lúc này",
                color: "danger"
            })
        }
    })
}
