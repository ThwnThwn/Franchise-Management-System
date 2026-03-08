import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { shiftApi, shiftSwapApi } from "@/features/shifts/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { confirmDialog, promptDialog, useTokenStore } from "@/hooks"
import { GetAllSwapsParams } from "../../models/request"
import { StaffAssignmentCandidate } from "../../models/response"
import { useMemo } from "react"

export const useGetAllSwaps = ({
    query,
    enabled = true
}: {
    query: GetAllSwapsParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SHIFT_SWAPS, query],
        queryFn: () => shiftSwapApi.getAll(query),
        enabled
    })
}

export const useGetSwapCandidates = (shiftId: string, enabled = true) => {
    const storeId = useTokenStore((s) => s.storeId) || ""

    const { data: candidates = [], isLoading } = useQuery<StaffAssignmentCandidate[]>({
        queryKey: [QUERY_KEYS.SHIFT_CANDIDATES, QUERY_KEYS.SHIFT_SWAPS, storeId, shiftId],
        queryFn: () => shiftApi.getAvailableStaffs({ storeId: storeId, shiftId: shiftId! }),
        enabled: !!storeId && !!shiftId && enabled
    })

    const availableCandidates = useMemo(() => {
        if (!candidates.length) return []

        return (
            candidates
                // 1. Loại bỏ bản thân và những người đã gán vào ca
                .filter((c) => !c.isAssigned)
                // 2. Loại bỏ những người dính "Hard Block" (Conflict lịch hoặc khác cửa hàng)
                .filter((c) => !c.isConflict && !c.validation?.isShuttleInvalid)
                // 3. Xếp hạng: Ai sạch sẽ nhất thì lên đầu
                .sort((a, b) => {
                    const getScore = (c: StaffAssignmentCandidate) => {
                        if (!c.isAvailable) return 3
                        if (c.validation?.isRestTimeInvalid || c.validation?.isOverLimit) return 2
                        return 1
                    }
                    return getScore(a) - getScore(b)
                })
        )
    }, [candidates])

    return { availableCandidates, isLoading }
}

// [Story: Staff gửi yêu cầu đổi ca cho đồng nghiệp]
export const useRequestSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: shiftSwapApi.request,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã gửi yêu cầu đổi ca thành công",
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

// [Story: Đồng nghiệp Phản hồi Đồng ý/Từ chối]
export const useRespondSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftSwapApi.respond,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã ghi nhận phản hồi của bạn",
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

    // Wrapper: Bọc Prompt Dialog
    const respondWithConfirm = async (swapId: string, isAccepted: boolean) => {
        const action = isAccepted ? "Đồng ý" : "Từ chối"
        const variant = isAccepted ? "success" : "danger"

        const note = await promptDialog({
            title: `Phản hồi đổi ca`,
            message: `Bạn đang chọn ${action.toLowerCase()} yêu cầu đổi ca này.`,
            variant: variant,
            confirmText: `Xác nhận ${action.toLowerCase()}`,
            inputPlaceholder: "Nhập lời nhắn cho đồng nghiệp (Không bắt buộc)...",
            isInputRequired: false // Đồng ý hay Từ chối giữa đồng nghiệp có thể không bắt buộc ép nhập
        })

        if (note !== false) {
            mutation.mutate({
                swapId: swapId,
                req: {
                    accept: isAccepted,
                    rejectReason: note
                }
            })
        }
    }

    return { ...mutation, respondWithConfirm }
}

// [Story: Manager Duyệt/Từ chối kèo đổi ca]
export const useReviewSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftSwapApi.approve, // Hoặc shiftSwapApi.review tùy backend
        onSuccess: (res) => {
            // Duyệt xong đổi ca -> Lịch làm việc biến động -> Refresh cả Lịch chung
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã xử lý yêu cầu đổi ca",
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

    // Wrapper: Bọc Prompt Dialog (Chuẩn logic Manager)
    const reviewSwapWithConfirm = async (swapId: string, isApproved: boolean) => {
        const action = isApproved ? "Phê duyệt" : "Từ chối"
        const variant = isApproved ? "success" : "danger"

        const note = await promptDialog({
            title: `${action} đổi ca`,
            message: `Bạn đang ${action.toLowerCase()} yêu cầu đổi ca này.`,
            variant: variant,
            confirmText: `Xác nhận ${action.toLowerCase()}`,
            inputPlaceholder: isApproved
                ? "Ghi chú (Không bắt buộc)..."
                : "Vui lòng nhập lý do từ chối (Bắt buộc)...",
            isInputRequired: !isApproved // LOGIC: Từ chối thì ép nhập lý do
        })

        if (note !== false) {
            mutation.mutate({
                swapId: swapId,
                req: {
                    approve: isApproved,
                    rejectReason: note
                }
            })
        }
    }

    return { ...mutation, reviewSwapWithConfirm }
}

// [Story: Người gửi tự Hủy yêu cầu]
export const useCancelSwap = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftSwapApi.cancel,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFT_SWAPS, QUERY_KEYS.ME] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Đã hủy yêu cầu đổi ca",
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

    // Wrapper: Bọc Confirm Dialog
    const cancelWithConfirm = async (swapId: string) => {
        const isConfirmed = await confirmDialog({
            title: "Hủy yêu cầu đổi ca",
            message: "Bạn có chắc chắn muốn rút lại yêu cầu đổi ca này không?",
            variant: "danger",
            confirmText: "Đồng ý hủy"
        })

        if (isConfirmed) {
            mutation.mutate({ swapId })
        }
    }

    return { ...mutation, cancelWithConfirm }
}

// [Story: Staff nhả ca về]
export const useReleaseShift = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: shiftSwapApi.release,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS], exact: false })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã đẩy ca về Chợ ca thành công",
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

    // Wrapper: Bọc Confirm Dialog
    const releaseWithConfirm = async (assignmentId: string) => {
        const isConfirmed = await confirmDialog({
            title: "Nhả ca về Chợ",
            message:
                "Bạn có chắc chắn muốn nhả ca này về Chợ Ca? Ca làm việc này sẽ được mở lại để người khác đăng ký và bạn sẽ bị xóa khỏi ca này.",
            variant: "warning",
            confirmText: "Đồng ý nhả ca"
        })

        if (isConfirmed) {
            mutation.mutate({ assignmentId })
        }
    }

    return { ...mutation, releaseWithConfirm }
}
