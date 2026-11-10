import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { STRINGS } from "@/constants/strings"
import { withdrawalApi } from "@/features/wallets/services"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { confirmDialog, promptDialog } from "@/hooks"
import {
    GetAllWithdrawalsReq,
    GetMyWithdrawalsReq,
    CreateWithdrawalReq
} from "@/features/wallets/models/request"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { AlertStyled } from "@/components"

// ==========================================
// --- NHÓM END-USER (KHÁCH HÀNG) ---
// ==========================================

export const useGetMyWithdrawals = (query: GetMyWithdrawalsReq, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.WITHDRAWALS, QUERY_KEYS.ME, query],
        queryFn: () => withdrawalApi.getMyWithdrawals(query),
        enabled
    })
}

/**
 * [Story: Khách hàng tạo yêu cầu rút tiền]
 * Bọc confirm để khách check lại số tiền trước khi bị trừ ví.
 */
export const useCreateWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: (req: CreateWithdrawalReq) => withdrawalApi.create(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WALLETS, QUERY_KEYS.ME] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Yêu cầu rút tiền đã được gửi. Vui lòng chờ xử lý.",
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

    const createWithConfirm = async (req: CreateWithdrawalReq) => {
        const isConfirmed = await confirmDialog({
            title: "Xác nhận tạo yêu cầu",
            message: (
                <div className="flex flex-col gap-4 py-2">
                    {/* Phần thông tin chính */}
                    <div className="bg-default-50 p-4 rounded-xl border-2 border-dashed border-default-200">
                        <p className="text-default-500 text-xs uppercase font-bold tracking-widest mb-1 text-center">
                            Số tiền rút
                        </p>
                        <p className="text-3xl font-black text-primary-700 text-center tabular-nums">
                            {formatCurrencyWithSymbol(req.amount)}
                        </p>
                    </div>

                    {/* Phần cảnh báo nhẹ nhàng */}
                    <AlertStyled
                        color="warning"
                        title="Lưu ý số dư"
                        description={
                            <p className="text-xs text-warning-600 leading-relaxed">
                                Số tiền này sẽ được{" "}
                                <span className="font-bold underline">tạm khóa</span> ngay lập tức
                                để hệ thống thực hiện lệnh chuyển khoản.
                            </p>
                        }
                    />

                    <p className="text-default-500 text-xs italic text-center">
                        Vui lòng kiểm tra kỹ thông tin ngân hàng trước khi xác nhận.
                    </p>
                </div>
            ),
            variant: "warning",
            confirmText: "Xác nhận rút"
        })

        if (isConfirmed) {
            mutation.mutate(req)
        }
    }

    return { ...mutation, createWithConfirm }
}

export const useCancelMyWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    const mutation = useMutation({
        mutationFn: withdrawalApi.cancelMyWithdrawal,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WALLETS, QUERY_KEYS.ME] })
            addToast({ title: STRINGS.COMMON.SUCCESS, description: res.message, color: "success" })
            onSuccess?.()
        }
    })

    const cancelWithConfirm = async (id: string) => {
        const reason = await promptDialog({
            title: "Hủy yêu cầu rút tiền",
            message: "Bạn có chắc muốn hủy yêu cầu này? Tiền sẽ được hoàn lại ví ngay lập tức.",
            inputPlaceholder: "Lý do hủy...",
            isInputRequired: true,
            confirmText: "Xác nhận hủy",
            variant: "danger"
        })
        if (reason) mutation.mutate({ id, req: { cancellationReason: reason } })
    }

    return { ...mutation, cancelWithConfirm }
}

// ==========================================
// --- NHÓM ADMIN (QUẢN TRỊ VIÊN) ---
// ==========================================

export const useGetAllWithdrawals = (query: GetAllWithdrawalsReq, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.WITHDRAWALS, query],
        queryFn: () => withdrawalApi.getAll(query),
        enabled
    })
}

export const useGetWithdrawalById = (id: string, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.WITHDRAWALS, "detail", id],
        queryFn: () => withdrawalApi.getById(id),
        enabled: !!id && enabled
    })
}

export const useApproveWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    const mutation = useMutation({
        mutationFn: withdrawalApi.approve,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            addToast({ title: STRINGS.COMMON.SUCCESS, description: res.message, color: "success" })
            onSuccess?.()
        }
    })

    const approveWithConfirm = async (id: string) => {
        const note = await promptDialog({
            title: "Phê duyệt yêu cầu",
            message: "Chuyển trạng thái lệnh sang APPROVED.",
            inputPlaceholder: "Ghi chú duyệt...",
            confirmText: "Duyệt lệnh",
            variant: "success"
        })
        if (note !== false) mutation.mutate({ id, req: { approvalNotes: note } })
    }

    return { ...mutation, approveWithConfirm }
}

export const useRejectWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    const mutation = useMutation({
        mutationFn: withdrawalApi.reject,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WALLETS] })
            addToast({ title: STRINGS.COMMON.SUCCESS, description: res.message, color: "success" })
            onSuccess?.()
        }
    })

    const rejectWithConfirm = async (id: string) => {
        const reason = await promptDialog({
            title: "Từ chối yêu cầu",
            message: "Tiền sẽ được hoàn trả lại ví cho khách.",
            inputPlaceholder: "Lý do từ chối (bắt buộc)...",
            isInputRequired: true,
            confirmText: "Xác nhận từ chối",
            variant: "danger"
        })
        if (reason) mutation.mutate({ id, req: { rejectionReason: reason } })
    }

    return { ...mutation, rejectWithConfirm }
}

/**
 * [Story: Admin chủ động Hủy yêu cầu]
 * Áp dụng cho lệnh ở trạng thái Pending/Approved/Processing.
 * Tiền được hoàn lại ví cho khách.
 */
export const useCancelWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: withdrawalApi.cancel,
        onSuccess: (res) => {
            // Refresh cả danh sách rút tiền và ví (vì có hoàn tiền)
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WALLETS] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã hủy lệnh rút tiền thành công.",
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

    const cancelWithConfirm = async (id: string) => {
        const reason = await promptDialog({
            title: "Admin Hủy yêu cầu rút tiền",
            message:
                "Hành động này sẽ hủy bỏ lệnh rút tiền và hoàn trả số dư về ví của khách hàng.",
            inputPlaceholder: "Vui lòng nhập lý do hủy lệnh (Bắt buộc)...",
            isInputRequired: true, // Ép Admin phải nhập lý do
            confirmText: "Xác nhận hủy lệnh",
            variant: "danger"
        })

        if (reason) {
            mutation.mutate({
                id,
                req: { cancellationReason: reason }
            })
        }
    }

    return { ...mutation, cancelWithConfirm }
}

/**
 * [Story: Admin Hoàn tất - CÓ UPLOAD BILL]
 * Thêm confirm để Admin rà soát lại Bill và mã giao dịch lần cuối.
 */
export const useCompleteWithdrawal = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: withdrawalApi.complete,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WITHDRAWALS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã hoàn tất lệnh rút tiền!",
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

    // Sửa lại tham số nhận vào: chỉ cần ID và File ảnh
    const completeWithConfirm = async (id: string, receiptFile: File) => {
        // Dùng promptDialog ép nhập Mã GD
        const bankTransactionId = await promptDialog({
            title: "Hoàn tất rút tiền",
            message:
                "Vui lòng nhập Mã Giao Dịch Ngân Hàng để đối soát. Lệnh sẽ chuyển sang COMPLETED và không thể hoàn tác.",
            inputPlaceholder: "Nhập mã giao dịch (VD: FT212345678)...",
            isInputRequired: true, // Bắt buộc nhập
            variant: "success",
            confirmText: "Xác nhận hoàn tất"
        })

        if (bankTransactionId) {
            try {
                await mutation.mutateAsync({
                    id,
                    req: {
                        bankTransactionId: bankTransactionId,
                        receiptFile: receiptFile
                    }
                })
            } catch {
                /* empty */
            }
        }
    }

    return { ...mutation, completeWithConfirm }
}
