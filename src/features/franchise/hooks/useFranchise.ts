import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { franchiseApi } from "@/features/franchise/services/franchiseApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { confirmDialog, promptDialog } from "@/hooks"
import {
    GetAllApplicationsRequest,
    GetAllContractsRequest
} from "@/features/franchise/models/request"
import { BackendError } from "@/models/common/respone"
import { ContractSetupStatus, FranchiseApplicationStatus } from "@/features/franchise/constants"
import { QUERY_KEYS } from "@/constants/queryKeys"

// ── HOOKS CHO APPLICATIONS ───────────────────────────────────────────────

export const useGetAllApplications = (query: GetAllApplicationsRequest, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_APPLICATIONS, query],
        queryFn: () => franchiseApi.getAllApplications(query),
        enabled
    })
}

export const useGetApplicationDetail = (id: string, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_APPLICATIONS, id],
        queryFn: () => franchiseApi.getApplicationDetail(id),
        enabled: !!id && enabled
    })
}

export const useSubmitApplication = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: franchiseApi.submitApplication,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.FRANCHISE_APPLICATIONS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Hồ sơ của bạn đã được gửi thành công. Vui lòng chờ phản hồi.",
                color: "success"
            })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Có lỗi xảy ra khi nộp hồ sơ",
                color: "danger"
            })
        }
    })
}

// [Story: Admin Duyệt/Từ chối hồ sơ nhượng quyền]
export const useReviewApplication = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: franchiseApi.reviewApplication,
        onSuccess: (res) => {
            // Refresh danh sách hồ sơ và hợp đồng
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.FRANCHISE_APPLICATIONS] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã xử lý hồ sơ thành công",
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

    // Wrapper: Bọc Prompt Dialog chuẩn bài cho Admin
    const reviewWithConfirm = async (id: string, isApproved: boolean) => {
        const action = isApproved ? "Phê duyệt" : "Từ chối"
        const variant = isApproved ? "success" : "danger"

        // Gọi cái promptDialog xịn của ní nè
        const note = await promptDialog({
            title: `${action} hồ sơ nhượng quyền`,
            message: `Bạn đang thực hiện ${action.toLowerCase()} hồ sơ này.`,
            variant: variant,
            confirmText: `Xác nhận ${action.toLowerCase()}`,
            inputPlaceholder: isApproved
                ? "Nhập lời nhắn gửi đối tác (Không bắt buộc)..."
                : "Vui lòng nhập lý do từ chối (Bắt buộc)...",
            isInputRequired: !isApproved
        })

        // Nếu người dùng không bấm Cancel (note !== false)
        if (note !== false) {
            mutation.mutate({
                id: id,
                req: {
                    action: isApproved
                        ? FranchiseApplicationStatus.Approved
                        : FranchiseApplicationStatus.Rejected,
                    note: note
                }
            })
        }
    }

    return { ...mutation, reviewWithConfirm }
}

// ── HOOKS CHO CONTRACTS ──────────────────────────────────────────────────

export const useGetAllContracts = (query: GetAllContractsRequest, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_CONTRACTS, query],
        queryFn: () => franchiseApi.getAllContracts(query),
        enabled
    })
}

export const useGetContractDetail = (id: string, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_CONTRACTS, id],
        queryFn: () => franchiseApi.getContractDetail(id),
        enabled: !!id && enabled
    })
}

// Xác nhận thanh toán cọc
export const useConfirmPayment = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: franchiseApi.confirmPayment,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.FRANCHISE_CONTRACTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Xác nhận thanh toán thành công",
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

    const confirmWithDialog = async (id: string) => {
        const isConfirmed = await confirmDialog({
            title: "Xác nhận đã nhận cọc",
            message:
                "Bạn xác nhận đối tác đã thanh toán đủ tiền cọc? Hành động này sẽ kích hoạt hợp đồng và tạo Cửa hàng mới.",
            variant: "warning",
            confirmText: "Xác nhận"
        })

        if (isConfirmed) {
            mutation.mutate({ id, req: {} })
        }
    }

    return { ...mutation, confirmWithDialog }
}

// Cập nhật trạng thái Setup (Design -> Construction...)
export const useUpdateSetupStatus = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: franchiseApi.updateSetupStatus,
        onSuccess: (_, variables) => {
            if (variables.req.setupStatus === ContractSetupStatus.Ready) {
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.ME], exact: false })
            }

            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.FRANCHISE_CONTRACTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật tiến độ thành công",
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

// ME
export const useGetMyApplications = ({ enabled = true }: { enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_APPLICATIONS, QUERY_KEYS.ME],
        queryFn: () => franchiseApi.getMyApplications(),
        enabled: enabled
    })
}

export const useGetMyContracts = ({ enabled = true }: { enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.FRANCHISE_CONTRACTS, QUERY_KEYS.ME],
        queryFn: () => franchiseApi.getMyContracts(),
        enabled: enabled
    })
}
