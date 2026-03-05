import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { timeLogApi } from "@/features/shifts/services/timeLogApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetTimeLogsParams, UpdateTimeLogRequest } from "@/features/shifts/models/request"
import { PaginationParams } from "@/models/common/request"

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
        queryFn: () => timeLogApi.getAllTimeLogs({ query, pagination }),
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
            timeLogApi.updateTimeLog(id, req),
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
