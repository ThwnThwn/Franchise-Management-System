import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { STRINGS } from "@/constants/strings"
import { walletApi } from "@/features/wallets/services"
import { WalletStatus, WalletStatusLabels } from "@/features/wallets/constants"
import { TopUpWalletRequest } from "@/features/wallets/models/request"
import { WalletResponse } from "@/features/wallets/models/response"
import { BackendError } from "@/models/common/respone"
import { addToast } from "@heroui/react"
import { promptDialog } from "@/hooks"

/**
 * HOOKS CHO WALLET
 * Quản lý trạng thái ví, nạp tiền và các thao tác của Admin.
 */

// ==========================================
// --- NHÓM END-USER (KHÁCH HÀNG) ---
// ==========================================

/**
 * [Story: Khách xem ví của mình]
 * Lấy thông tin số dư, trạng thái ví của người dùng đang đăng nhập.
 */
export const useGetMyWallet = (enabled = true) => {
    return useQuery<WalletResponse>({
        queryKey: [QUERY_KEYS.WALLETS, QUERY_KEYS.ME],
        queryFn: () => walletApi.getMyWallet(),
        enabled
    })
}

/**
 * [Story: Khách nạp tiền qua VNPay]
 * Tạo link thanh toán. Việc redirect sang VNPay để Component cha lo.
 */
export const useTopUpWallet = ({ onSuccess }: { onSuccess?: (url: string) => void } = {}) => {
    return useMutation({
        mutationFn: (req: TopUpWalletRequest) => walletApi.topUpMyWallet(req),
        onSuccess: (res) => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo liên kết nạp tiền thành công.",
                color: "success"
            })
            onSuccess?.(res.paymentUrl)
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Không thể khởi tạo giao dịch nạp tiền.",
                color: "danger"
            })
        }
    })
}

// ==========================================
// --- NHÓM ADMIN (QUẢN TRỊ VIÊN) ---
// ==========================================

/**
 * [Story: Admin xem ví của một User cụ thể]
 * Dùng khi Admin cần kiểm tra số dư của khách để xử lý khiếu nại hoặc rút tiền.
 */
export const useGetWalletByUserId = (userId: string, enabled = true) => {
    return useQuery<WalletResponse>({
        queryKey: [QUERY_KEYS.WALLETS, userId],
        queryFn: () => walletApi.getByUserId(userId),
        enabled: !!userId && enabled
    })
}

/**
 * [Story: Admin Khóa/Mở/Đình chỉ ví]
 * Wrapper bọc promptDialog để Admin nhập lý do trước khi thay đổi trạng thái ví.
 */
export const useUpdateWalletStatus = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: walletApi.updateStatus,
        onSuccess: (res) => {
            // Refresh lại cache ví để UI cập nhật trạng thái mới nhất ngay lập tức
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.WALLETS] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã cập nhật trạng thái ví thành công.",
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

    // Wrapper để gọi từ UI Table/Action
    const updateStatusWithConfirm = async (walletId: string, newStatus: WalletStatus) => {
        const statusName = WalletStatusLabels[newStatus]
        const isDangerous =
            newStatus === WalletStatus.Locked || newStatus === WalletStatus.Suspended

        const reason = await promptDialog({
            title: `Thay đổi trạng thái ví`,
            message: `Bạn đang chuyển trạng thái ví sang: "${statusName.toUpperCase()}".`,
            variant: isDangerous ? "danger" : "info",
            confirmText: "Xác nhận thay đổi",
            inputPlaceholder: "Vui lòng nhập lý do can thiệp ví khách hàng...",
            isInputRequired: true
        })

        if (reason) {
            mutation.mutate({
                id: walletId,
                req: {
                    newStatus: newStatus,
                    reason: reason,
                    adminNotes: `Hành động thực hiện bởi Admin.`
                }
            })
        }
    }

    return { ...mutation, updateStatusWithConfirm }
}
