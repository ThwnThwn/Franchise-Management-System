import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { shiftApi } from "@/features/shifts/services/shiftApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetShiftsParams } from "@/features/shifts/models/request"
import { AutoFillShiftsResponse } from "../../models/response"
import { confirmDialog } from "@/hooks"
import dayjs from "dayjs"

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

    const mutation = useMutation({
        mutationFn: shiftApi.autoFill,
        onSuccess: (data) => {
            // 1. Refresh lại toàn bộ danh sách ca làm
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            // 2. Bắn toast báo cáo kết quả
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: `Tự động xếp ca thành công! Đã điền ${data.filledCount}/${data.totalSlots} vị trí.`,
                color: "success"
            })

            // 3. Callback để UI xử lý tiếp (ví dụ: bôi đỏ các ca unfilled)
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

    // BỔ SUNG: Helper function bọc sẵn Confirm Dialog
    const autoFillWithConfirm = async (startDate: string, endDate: string) => {
        // Hỏi quản lý trước khi thi triển phép thuật
        const isConfirmed = await confirmDialog({
            title: "Tự động xếp ca",
            message: `Hệ thống sẽ chạy tự động phân công nhân viên vào các ca làm việc từ ${dayjs(startDate).format("DD/MM")} đến ${dayjs(endDate).format("DD/MM")}. Bạn có chắc chắn muốn tiếp tục?`,
            variant: "info",
            confirmText: "Thực hiện ngay"
        })

        // Nếu bấm Hủy -> Trả về lỗi ngầm để cắt luồng
        if (!isConfirmed) return Promise.reject(new Error("USER_CANCELLED"))

        // Nếu đồng ý -> Kích hoạt lõi mutation
        return mutation.mutateAsync({ startDate, endDate })
    }

    return {
        ...mutation,
        autoFillWithConfirm
    }
}
