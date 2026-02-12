import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { myShiftApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { AttendancePayload } from "@/features/shifts/models/request"

// --- 1. QUERY HOOK: Lấy trạng thái ca làm hiện tại ---
export const useGetCurrentShift = (enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT],
        queryFn: () => myShiftApi.getCurrent(),
        enabled,
        // Tự động refetch khi quay lại tab (để cập nhật serverTime hoặc trạng thái ca)
        refetchOnWindowFocus: true,
        // Có thể thêm staleTime nếu không muốn gọi API quá dày
        staleTime: 1000 * 60
    })
}

// --- 2. MUTATION HOOK: Check-in (Vào ca) ---
export const useCheckIn = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({
            assignmentId,
            payload
        }: {
            assignmentId: string
            payload: AttendancePayload
        }) => myShiftApi.checkIn(assignmentId, payload),
        onSuccess: () => {
            // Quan trọng: Refresh lại thông tin ca để nút bấm chuyển sang "Tan ca"
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT]
            })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Vào ca làm việc thành công. Chúc bạn một ngày làm việc vui vẻ!",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description:
                    error.detail || "Không thể Check-in. Vui lòng kiểm tra lại vị trí GPS.",
                color: "danger"
            })
        }
    })
}

// --- 3. MUTATION HOOK: Check-out (Tan ca) ---
export const useCheckOut = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ logId, payload }: { logId: string; payload: AttendancePayload }) =>
            myShiftApi.checkOut(logId, payload),
        onSuccess: () => {
            // Refresh để UI ẩn banner hoặc chuyển trạng thái "Đã hoàn thành"
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.CURRENT]
            })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tan ca thành công. Hẹn gặp lại bạn vào ca làm tiếp theo!",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Lỗi khi tan ca. Vui lòng thử lại.",
                color: "danger"
            })
        }
    })
}
