import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { voucherApi } from "../services/voucherApi"
import {
    CreateVoucherRequest,
    UpdateVoucherRequest,
    UpdateVoucherStatusRequest
} from "../models/voucher"

import { QUERY_KEYS } from "@/constants/queryKeys"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

export const useGetAllVouchers = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.VOUCHERS],
        queryFn: () => voucherApi.getAll()
    })
}
export const useCreateVoucher = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (req: CreateVoucherRequest) => voucherApi.create(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo voucher mới thành công!",
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
export const useUpdateVoucherStatus = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateVoucherStatusRequest }) =>
            voucherApi.updateStatus(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            addToast({
                title: "Đã cập nhật",
                description: "Trạng thái voucher đã được thay đổi.",
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

    return useMutation({
        mutationFn: (id: string) => voucherApi.redeemVoucher(id),
        onSuccess: () => {
            // Làm mới cả danh sách chung và danh sách của cá nhân
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.VOUCHERS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_VOUCHERS] })

            addToast({
                title: "Đổi Voucher thành công!",
                description: "Voucher đã được thêm vào kho quà của bạn.",
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
