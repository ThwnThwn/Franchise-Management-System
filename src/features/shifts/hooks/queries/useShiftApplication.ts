import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { shiftApplicationApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

/**
 * Lấy danh sách nhân viên đang chờ nhận một ca cụ thể
 * Thường dùng cho Manager khi bấm vào một ca ở Chợ Ca
 */
export const useGetApplicationsByShiftId = ({
    shiftId,
    enabled = true
}: {
    shiftId: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS, shiftId],
        queryFn: () => shiftApplicationApi.getByShiftId({ shiftId }),
        enabled: !!shiftId && enabled
    })
}

// --- [Story: Staff nộp đơn nhận ca] ---
export const useApplyForShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApplicationApi.apply,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã gửi yêu cầu nhận ca thành công!",
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

// --- [Story: Manager duyệt/từ chối đơn] ---
export const useReviewShiftApplication = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftApplicationApi.review,
        onSuccess: (res) => {
            // 1. Refresh lại danh sách ứng viên của ca đó
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_APPLICATIONS] })
            // 2. Refresh lại lịch (vì duyệt xong thì ca đó có thể bị đóng hoặc gán người)
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã xử lý đơn đăng ký thành công",
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
