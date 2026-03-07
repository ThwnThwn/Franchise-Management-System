import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { myShiftApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import {
    AttendancePayload,
    GetAgendaParams,
    GetTimeLogsParams
} from "@/features/shifts/models/request"
import { PaginationParams } from "@/models/common/request"

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
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.TIME_LOGS],
                exact: false
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
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.TIME_LOGS],
                exact: false
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

/**
 * Hook lấy danh sách lịch làm việc của nhân viên theo khoảng ngày.
 * Tự động kích hoạt lại khi startDate hoặc endDate thay đổi.
 * @param params - Đối tượng chứa ngày bắt đầu và kết thúc (YYYY-MM-DD)
 * @param enabled - Flag cho phép/ngăn chặn việc tự động fetch (VD: Chỉ fetch khi đã có Role Staff)
 */
export const useGetMyAgenda = (params: GetAgendaParams, enabled = true) => {
    return useQuery({
        // Key bao gồm params để TanStack Query tự động phân tách cache theo từng khoảng ngày khác nhau
        queryKey: [
            QUERY_KEYS.ME,
            QUERY_KEYS.SHIFTS,
            QUERY_KEYS.AGENDA,
            params.startDate,
            params.endDate
        ],

        queryFn: () => myShiftApi.getAgenda(params),

        // Chỉ chạy khi thỏa mãn: Role hợp lệ, có đủ ngày bắt đầu và kết thúc
        enabled: enabled && !!params.startDate && !!params.endDate,

        /**
         * Lịch làm việc thường cố định, ít thay đổi đột ngột.
         * Để staleTime 5 phút giúp tiết kiệm tài nguyên hệ thống, tránh fetch lại khi nhân viên chuyển tab.
         */
        staleTime: 1000 * 60 * 5,

        // Giữ lại dữ liệu cũ trong lúc fetch dữ liệu mới để UI không bị giật (Loading skeleton)
        placeholderData: (previousData) => previousData
    })
}

/**
 * Hook lấy lịch sử chấm công của bản thân (Staff)
 * @param query - Filter (fromDate, toDate, status...)
 * @param pagination - Phân trang (page, pageSize)
 */
export const useGetMyTimeLogHistory = ({
    query,
    pagination,
    enabled = true
}: {
    query: GetTimeLogsParams
    pagination: PaginationParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.ME, QUERY_KEYS.SHIFTS, QUERY_KEYS.TIME_LOGS, query, pagination],
        queryFn: () => myShiftApi.getTimeLogHistory({ query, pagination }),
        enabled: enabled,
        staleTime: 1000 * 60 * 2, // 2 phút vì dữ liệu lịch sử ít biến động
        placeholderData: (previousData) => previousData
    })
}

/**
 * Hook lấy danh sách yêu cầu đổi ca của tôi (Người gửi hoặc Người nhận)
 * @param enabled - Cho phép fetch dữ liệu hay không (mặc định là true)
 */
export const useGetMySwaps = ({ enabled = true }: { enabled?: boolean } = {}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME],
        queryFn: () => myShiftApi.getMySwaps(),
        enabled
    })
}
