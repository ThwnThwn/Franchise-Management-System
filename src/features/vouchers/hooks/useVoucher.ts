import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { voucherApi } from "../services/voucherApi"
import {
    CreateVoucherRequest,
    UpdateVoucherRequest,
    UpdateVoucherStatusRequest
} from "../models/request"

import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { confirmDialog } from "@/hooks"
import { Voucher } from "../models/response"
import { formatCurrency, normalizeCurrency } from "@/utils/helpers/currency"

export const useGetAllVouchers = (
    initialData?: Voucher[],
    { enabled = true }: { enabled?: boolean } = {}
) => {
    return useQuery({
        queryKey: [QUERY_KEYS.VOUCHERS],
        queryFn: () => voucherApi.getAll(),
        enabled: enabled,
        initialData: initialData
    })
}

export const useCreateVoucher = ({ onSuccess }: { onSuccess?: (res: Voucher) => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (req: CreateVoucherRequest) => voucherApi.create(req),
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo voucher mới thành công!",
                color: "success"
            })
            onSuccess?.(res)
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

// 2. Update - Cần làm mới danh sách VOUCHERS
export const useUpdateVoucher = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateVoucherRequest }) =>
            voucherApi.update(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thông tin voucher thành công!",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

// 3. Update Status - Thường dùng cho Switch On/Off voucher
export const useUpdateVoucherStatus = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateVoucherStatusRequest }) =>
            voucherApi.updateStatus(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: "Đã cập nhật",
                description: "Trạng thái voucher đã được thay đổi.",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })

    // Wrapper: Bọc Confirm Dialog thông minh
    const updateStatusWithConfirm = async (id: string, isActive: boolean) => {
        const isConfirmed = await confirmDialog({
            title: isActive ? "Kích hoạt Voucher" : "Tạm ngưng Voucher",
            message: isActive
                ? "Voucher này sẽ được kích hoạt. Khách hàng có thể nhìn thấy và thu thập nó. Bạn đồng ý chứ?"
                : "Bạn muốn tạm ngưng voucher này? Khách hàng sẽ không thể tiếp tục lưu hay sử dụng voucher này nữa.",
            variant: isActive ? "info" : "warning",
            confirmText: isActive ? "Đồng ý kích hoạt" : "Đồng ý tạm ngưng"
        })

        if (isConfirmed) {
            mutation.mutate({ id, req: { isActive } })
        }
    }

    return { ...mutation, updateStatusWithConfirm }
}

// 4. Delete - Xóa xong phải dọn dẹp danh sách
export const useDeleteVoucher = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: string) => voucherApi.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: "Đã xóa",
                description: "Voucher đã được loại bỏ khỏi hệ thống.",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

// 5. Query - Không đổi
export const useGetMyVouchers = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.MY_VOUCHERS],
        queryFn: () => voucherApi.getMyVouchers()
    })
}

// 6. Redeem - Quan trọng: Phải làm mới "Voucher của tôi"
export const useRedeemVoucher = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: (id: string) => voucherApi.redeemVoucher(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_VOUCHERS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.LOYALTY_ACCOUNT] })

            addToast({
                title: "Thành công!",
                description: "Voucher đã được lưu vào túi quà của bạn.",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })

    const redeemWithConfirm = async (voucher: Voucher) => {
        const pointCost = normalizeCurrency(voucher.pointCost || 0)

        const isConfirmed = await confirmDialog({
            title: "Xác nhận đổi Voucher",
            message:
                pointCost > 0
                    ? `Bạn sẽ sử dụng ${formatCurrency(pointCost)} điểm để đổi lấy voucher ${voucher.code}. Bạn có chắc chắn không?`
                    : `Xác nhận nhận miễn phí voucher ${voucher.code} vào túi đồ của bạn?`,
            variant: "info",
            confirmText: "Đổi ngay"
        })

        if (isConfirmed) {
            mutation.mutate(voucher.id)
        }
    }

    return { ...mutation, redeemWithConfirm }
}
