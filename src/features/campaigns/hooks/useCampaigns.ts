import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { confirmDialog } from "@/hooks"
import { campaignApi } from "../services/campaignApi"

// [Query: Lấy danh sách tất cả chiến dịch]
export const useGetAllCampaigns = (enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.CAMPAIGNS],
        queryFn: campaignApi.getAll,
        enabled
    })
}

export const useGetActiveCampaigns = (enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.CAMPAIGNS, "active"],
        queryFn: campaignApi.getActive,
        enabled
    })
}

// [Query: Lấy chi tiết một chiến dịch]
export const useGetCampaignById = (id: string, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.CAMPAIGNS, id],
        queryFn: () => campaignApi.getById({ id }),
        enabled: !!id && enabled
    })
}

// [Mutation: Tạo mới chiến dịch]
export const useCreateCampaign = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: campaignApi.create,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CAMPAIGNS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Tạo chiến dịch mới thành công",
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

// [Mutation: Bật/Tắt trạng thái hoạt động của chiến dịch (Có Confirm Dialog)]
export const useUpdateCampaignActiveStatus = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    const mutation = useMutation({
        mutationFn: campaignApi.updateActive,
        onSuccess: (res) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CAMPAIGNS], exact: false })

            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: res.message || "Đã cập nhật trạng thái chiến dịch",
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

    // Wrapper: Bọc Confirm Dialog trước khi gọi API
    const updateStatusWithConfirm = async (id: string, currentIsActive: boolean) => {
        // Nếu hiện tại đang Active (true) -> Hành động là Tắt (false)
        const targetActiveStatus = !currentIsActive

        const action = targetActiveStatus ? "Kích hoạt" : "Ngừng hoạt động"
        const variant = targetActiveStatus ? "success" : "danger"

        const isConfirmed = await confirmDialog({
            title: `${action} chiến dịch`,
            message: `Bạn có chắc chắn muốn ${action.toLowerCase()} chiến dịch này không?`,
            variant: variant,
            confirmText: `Đồng ý ${action.toLowerCase()}`
        })

        if (isConfirmed) {
            mutation.mutate({
                id: id,
                req: { isActive: targetActiveStatus }
            })
        }
    }

    return { ...mutation, updateStatusWithConfirm }
}

export const useRedeemCampaignVoucher = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ campaignId, voucherId }: { campaignId: string; voucherId: string }) =>
            campaignApi.redeemVoucher(campaignId, voucherId),

        onSuccess: (_, variables) => {
            addToast({
                title: STRINGS.COMMON.SUCCESS || "Tuyệt vời!",
                description: "Đổi thành công!",
                color: "success"
            })

            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.CAMPAIGNS, variables.campaignId]
            })
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
