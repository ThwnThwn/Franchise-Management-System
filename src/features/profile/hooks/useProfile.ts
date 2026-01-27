import { QUERY_KEYS } from "@/constants/queryKeys"
import { useTokenStore } from "@/hooks"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { profileApi } from "../service"
import { UserUpdateReq } from "../models/request"
import { UserProfileViewRes } from "../models/respone"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

export const useGetMe = () => {
    const isLogined = useTokenStore((s) => !!s.accessToken)

    const query = useQuery({
        queryKey: QUERY_KEYS.ME,
        queryFn: profileApi.getMe,
        enabled: isLogined,
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
            queryClient.setQueryData<UserProfileViewRes>(QUERY_KEYS.ME, (prev) => {
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
                    description: "success.update",
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
