import { QUERY_KEYS } from "@/constants/queryKeys"
import { useTokenStore } from "@/hooks"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { profileApi } from "../service"
import { UserUpdateReq } from "../models/request"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { User } from "@/models/user"
import { RoleSlug } from "@/constants/enums"

export const useGetMyLoyalty = (email?: string) => {
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)

    return useQuery({
        queryKey: [QUERY_KEYS.LOYALTY_ACCOUNT, email],
        queryFn: () => profileApi.getMyLoyalty(email!),
        enabled: isCustomer && !!email,
        staleTime: 5 * 60 * 1000,
        select: (data) => data?.[0] ?? undefined
    })
}

export const useGetMembershipTiers = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.MEMBERSHIP_TIERS],
        queryFn: () => profileApi.getMembershipTiers(),
        staleTime: 10 * 60 * 1000 // 10 phút vì ít thay đổi
    })
}

export const useGetMyPointLogs = (params?: any) => {
    const isLogined = useTokenStore((s) => !!s.accessToken)
    return useQuery({
        queryKey: [QUERY_KEYS.POINT_LOGS, params],
        queryFn: () => profileApi.getMyPointLogs(params),
        enabled: isLogined,
        staleTime: 3 * 60 * 1000 // 3 phút
    })
}

export const useGetMe = ({ enabled }: { enabled?: boolean } = { enabled: true }) => {
    const isLogined = useTokenStore((s) => !!s.accessToken)

    const query = useQuery({
        queryKey: [QUERY_KEYS.ME],
        queryFn: profileApi.getMe,
        enabled: isLogined && enabled,
        staleTime: 14 * 60 * 1000,
        refetchOnMount: false,
        refetchOnWindowFocus: false
    })
    return query
}

export const useUpdateMe = ({
    onSuccess,
    showToast = true
}: { onSuccess?: () => void; showToast?: boolean } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: async (req: UserUpdateReq) => {
            await profileApi.updateMe(req)
            return req
        },
        onSuccess: (data) => {
            queryClient.setQueryData<User>([QUERY_KEYS.ME], (prev) => {
                if (!prev) return prev
                return {
                    ...prev,
                    ...data
                }
            })
            onSuccess?.()
            if (showToast) {
                addToast({
                    title: STRINGS.NOTIFICATION.SUCCESS,
                    description: "Cập nhật thành công",
                    color: "success"
                })
            }
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

export const useUploadMyAvatar = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const key = [QUERY_KEYS.ME]

    return useMutation({
        mutationFn: async (formData: FormData) => {
            return await profileApi.uploadAvatar(formData)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: key, exact: false })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật ảnh thành công",
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

export const useDeleteMyAvatar = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const key = [QUERY_KEYS.ME]

    return useMutation({
        mutationFn: async () => {
            await profileApi.deleteAvatar()
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: key, exact: false })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Xoá ảnh thành công",
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
