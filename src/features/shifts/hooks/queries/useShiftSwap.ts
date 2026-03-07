import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { shiftSwapApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

// [Story: Staff gửi yêu cầu đổi ca cho đồng nghiệp]
export const useRequestSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.request,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã gửi yêu cầu đổi ca thành công",
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

// [Story: Đồng nghiệp Phản hồi Đồng ý/Từ chối]
export const useRespondSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.respond,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã ghi nhận phản hồi của bạn",
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

// [Story: Manager Duyệt/Từ chối kèo đổi ca]
export const useApproveSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.approve,
        onSuccess: (res) => {
            // Duyệt xong thì lịch làm việc thay đổi nên cần refresh SHIFTS
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã phê duyệt yêu cầu đổi ca",
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

// [Story: Người gửi tự Hủy yêu cầu]
export const useCancelSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.cancel,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã hủy yêu cầu đổi ca",
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

// [Story: Staff nhả ca về Chợ ca]
export const useReleaseShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.release,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã nhả ca về Chợ ca thành công",
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
