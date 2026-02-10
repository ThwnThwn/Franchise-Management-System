import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftApi } from "@/features/shifts/services/shiftApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { PaginationParams } from "@/models/common/request"
import { GetShiftsParams } from "@/features/shifts/models/request"

// --- QUERY HOOKS (GET DATA) ---

export const useGetShifts = ({
    params,
    pagination = {},
    enabled = true
}: {
    params: GetShiftsParams
    pagination?: PaginationParams
    enabled?: boolean
}) => {
    return useQuery({
        // Key bao gồm params và pagination để tự động refetch khi filter thay đổi
        queryKey: [QUERY_KEYS.SHIFTS, params, pagination],
        queryFn: () => shiftApi.getAll({ query: params, pagination }),
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

export const useAssignStaffToShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.assignStaff,
        onSuccess: (_, variables) => {
            // Quan trọng: Invalidate đúng ID của shift để cập nhật lại list assigned staff ngay lập tức
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS, variables.id] })
            // Cũng nên invalidate list phòng trường hợp list có hiển thị số lượng staff
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Phân công nhân viên thành công",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail, // VD: Cannot assign more than required...
                color: "danger"
            })
        }
    })
}

export const useRemoveStaffFromShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApi.removeStaff,
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS, variables.id] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xóa nhân viên khỏi ca",
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
