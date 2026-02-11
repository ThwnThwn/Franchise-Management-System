import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftAvailabilityApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetAvailabilityParams } from "@/features/shifts/models/request"
import { PaginationParams } from "@/models/common/request"

// --- QUERY HOOKS (Dành cho trang Table lịch sử của Staff) ---

export const useGetMyAvailabilityHistory = ({
    query,
    pagination,
    enabled = true
}: {
    query: GetAvailabilityParams
    pagination: PaginationParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES, QUERY_KEYS.ME, query, pagination],
        queryFn: () => shiftAvailabilityApi.getMyHistory({ query, pagination }),
        enabled
    })
}

// --- MUTATION HOOKS (Tạo/Sửa/Xóa đơn báo rảnh bận) ---

export const useCreateAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftAvailabilityApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Gửi yêu cầu lịch làm việc thành công",
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

export const useUpdateAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftAvailabilityApi.update,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật lịch thành công",
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

export const useDeleteAvailability = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftAvailabilityApi.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_AVAILABILITIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã xóa lịch đăng ký",
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
