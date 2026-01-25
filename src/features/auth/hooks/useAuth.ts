import { useTokenStore } from "@/hooks"
import { useMutation } from "@tanstack/react-query"
import { authApi } from "../services"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"

export const useLogin = ({
    rememberMe,
    onSuccess
}: {
    rememberMe?: boolean
    onSuccess?: () => void
}) => {
    const setAccessToken = useTokenStore((s) => s.setAccessToken)

    return useMutation({
        mutationFn: authApi.login,
        onSuccess: (data) => {
            setAccessToken(data.accessToken, rememberMe)
            onSuccess?.()
            addToast({
                title: "toast.success",
                description: "success.login",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || "toast.error",
                description: error.detail,
                color: "danger"
            })
        }
    })
}
