import { useTokenStore } from "@/hooks"
import { useMutation } from "@tanstack/react-query"
import { authApi } from "@/features/auth/services"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { useInvalidateMeQuery, useRemoveMeQuery } from "@/hooks/queries/useProfile"
import { AUTH_TEXT } from "../constants"
import { STRINGS } from "@/constants/strings"

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
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: AUTH_TEXT.STRINGS.LOGIN_SUCCESS,
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

export const useLogout = ({ onSuccess }: { onSuccess?: () => void }) => {
    const removeAccessToken = useTokenStore((s) => s.removeAccessToken)
    const removeMeQuery = useRemoveMeQuery()

    return useMutation({
        mutationFn: authApi.logout,
        onSuccess: () => {
            onSuccess?.()
            removeAccessToken()
            removeMeQuery()
            addToast({
                title: AUTH_TEXT.RESPONE_TEXT.SUCCESS,
                description: AUTH_TEXT.STRINGS.SUCCESS_LOGOUT,
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || AUTH_TEXT.RESPONE_TEXT.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

export const useRegister = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.regsiter,
        onSuccess: onSuccess,
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.NOTIFICATION.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

export const useRegisterVerify = ({ onSuccess }: { onSuccess?: () => void }) => {
    const invalidateMeQuery = useInvalidateMeQuery()

    return useMutation({
        mutationFn: authApi.registerVerify,
        onSuccess: () => {
            invalidateMeQuery()
            onSuccess?.()
            addToast({
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: AUTH_TEXT.STRINGS.REGISTER_SUCCESS,
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

export const useForgotPassword = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.forgetPassword,
        onSuccess: () => {
            onSuccess?.()
            addToast({
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: "Mã OTP đã được gửi đến email của bạn",
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

export const useForgotPasswordVerify = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.forgotPasswordVerify,
        onSuccess: () => {
            onSuccess?.()
            addToast({
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: "Xác thực OTP thành công",
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

export const useResetPassword = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.resetPassword,
        onSuccess: () => {
            onSuccess?.()
            addToast({
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: "Đặt lại mật khẩu thành công",
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

export const useResendForgotPasswordOtp = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.resendForgotPasswordOtp,
        onSuccess: () => {
            onSuccess?.()
            addToast({
                title: STRINGS.NOTIFICATION.SUCCESS,
                description: "Đã gửi lại mã OTP",
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
