import { confirmDialog, useTokenStore } from "@/hooks"
import { useMutation } from "@tanstack/react-query"
import { authApi } from "@/features/auth/services"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { useInvalidateMeQuery, useRemoveMeQuery } from "@/hooks/queries/useProfile"
import { AUTH_TEXT } from "../constants"
import { STRINGS } from "@/constants/strings"
import { ChangePasswordReq } from "../models/request"

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

export const useLogout = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const removeAccessToken = useTokenStore((s) => s.removeAccessToken)
    const removeMeQuery = useRemoveMeQuery()

    return useMutation({
        mutationFn: authApi.logout,
        onSettled: () => {
            removeAccessToken()
            removeMeQuery()

            addToast({
                title: AUTH_TEXT.RESPONE_TEXT.SUCCESS,
                description: AUTH_TEXT.STRINGS.SUCCESS_LOGOUT,
                color: "success"
            })

            onSuccess?.()
        }
    })
}

export const useRegister = ({ onSuccess }: { onSuccess?: () => void }) => {
    return useMutation({
        mutationFn: authApi.register,
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

export const useChangePassword = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const removeAccessToken = useTokenStore((s) => s.removeAccessToken)
    const removeMeQuery = useRemoveMeQuery()

    const mutation = useMutation({
        mutationFn: authApi.changePassword,
        onSuccess: (_data, variables) => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Mật khẩu đã được thay đổi thành công.",
                color: "success"
            })

            if (variables.forgetAll) {
                removeAccessToken()
                removeMeQuery()

                addToast({
                    title: "Đã đăng xuất thiết bị",
                    description: "Bạn đã được đăng xuất khỏi tất cả thiết bị theo yêu cầu.",
                    color: "warning"
                })
            }

            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail || "Không thể đổi mật khẩu, vui lòng thử lại.",
                color: "danger"
            })
        }
    })

    const changeWithConfirm = async (data: ChangePasswordReq) => {
        const isConfirmed = await confirmDialog({
            title: "Xác nhận đổi mật khẩu",
            message: data.forgetAll
                ? "Hệ thống sẽ đổi mật khẩu và đăng xuất bạn khỏi tất cả các thiết bị. Tiếp tục?"
                : "Bạn có chắc chắn muốn thay đổi mật khẩu không?",
            variant: "danger",
            confirmText: "Xác nhận đổi"
        })

        if (isConfirmed) {
            mutation.mutate(data)
        }
    }

    return { ...mutation, changeWithConfirm }
}
