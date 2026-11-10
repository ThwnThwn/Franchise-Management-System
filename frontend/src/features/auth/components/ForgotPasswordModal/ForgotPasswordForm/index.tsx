"use client"

import { ButtonStyled, InputStyled, LogoStyled, ButtonToggleVisibility } from "@/components"
import { AUTH_ERRORS } from "@/features/auth/constants"
import {
    useForgotPassword,
    useForgotPasswordVerify,
    useResetPassword,
    useResendForgotPasswordOtp
} from "@/features/auth/hooks"
import { useFormik } from "formik"
import React, { useState } from "react"
import * as Yup from "yup"
import { useLoginDiscloresureSingleton } from "@/hooks"
import { InputOtp, Link } from "@heroui/react"

export function ForgotPasswordForm({ onSuccess }: { onSuccess?: () => void }) {
    const [step, setStep] = useState(0) // 0: Email, 1: OTP, 2: New Password
    const [savedEmail, setSavedEmail] = useState("")
    const [isVisible, setIsVisible] = useState(false)
    const [isConfirmVisible, setIsConfirmVisible] = useState(false)

    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()

    const handleBackToLogin = () => {
        if (onSuccess) onSuccess()
        onOpenLogin()
    }

    const forgotPasswordMutation = useForgotPassword({
        onSuccess: () => {
            setStep(1)
        }
    })

    const verifyOtpMutation = useForgotPasswordVerify({
        onSuccess: () => {
            setStep(2)
        }
    })

    const resetPasswordMutation = useResetPassword({
        onSuccess: () => {
            onSuccess?.()
            onOpenLogin()
        }
    })

    const resendOtpMutation = useResendForgotPasswordOtp({})

    const emailFormik = useFormik({
        initialValues: { email: "" },
        validationSchema: Yup.object().shape({
            email: Yup.string()
                .required(AUTH_ERRORS.AUTH.EMAIL_REQUIRED)
                .email(AUTH_ERRORS.AUTH.INVALID_EMAIL)
        }),
        onSubmit: async (values) => {
            setSavedEmail(values.email)
            await forgotPasswordMutation.mutateAsync(values.email)
        }
    })

    const otpFormik = useFormik({
        initialValues: { otp: "" },
        validationSchema: Yup.object().shape({
            otp: Yup.string().required("Vui lòng nhập mã OTP")
        }),
        onSubmit: async (values) => {
            await verifyOtpMutation.mutateAsync({ otp: values.otp, email: savedEmail })
        }
    })

    const passwordFormik = useFormik({
        initialValues: { password: "", confirmPassword: "" },
        validationSchema: Yup.object().shape({
            password: Yup.string()
                .required(AUTH_ERRORS.AUTH.PASSWORD_REQUIRED)
                .min(6, "Mật khẩu phải có ít nhất 6 ký tự"),
            confirmPassword: Yup.string()
                .oneOf([Yup.ref("password")], "Mật khẩu không khớp")
                .required("Vui lòng xác nhận mật khẩu")
        }),
        onSubmit: async (values) => {
            await resetPasswordMutation.mutateAsync({
                newPassword: values.password,
                confirmPassword: values.confirmPassword
            })
        }
    })

    const toggleVisibility = () => setIsVisible(!isVisible)
    const toggleConfirmVisibility = () => setIsConfirmVisible(!isConfirmVisible)

    const handleResendOtp = async () => {
        await resendOtpMutation.mutateAsync(savedEmail)
    }

    const renderStepContent = () => {
        switch (step) {
            case 0:
                return (
                    <form
                        onSubmit={emailFormik.handleSubmit}
                        className="w-full flex flex-col gap-4"
                    >
                        <InputStyled
                            variant="bordered"
                            label="Email"
                            value={emailFormik.values.email}
                            onValueChange={(value) => emailFormik.setFieldValue("email", value)}
                            isInvalid={!!(emailFormik.touched.email && emailFormik.errors.email)}
                            errorMessage={emailFormik.errors.email}
                            onBlur={() => emailFormik.setFieldTouched("email")}
                        />
                        <ButtonStyled
                            className="w-full mt-4"
                            type="submit"
                            isLoading={forgotPasswordMutation.isPending}
                            color="primary"
                            isDisabled={!emailFormik.isValid}
                            // onPress={() => emailFormik.submitForm()}
                        >
                            Gửi yêu cầu
                        </ButtonStyled>
                    </form>
                )
            case 1:
                return (
                    <form onSubmit={otpFormik.handleSubmit} className="w-full flex flex-col gap-4">
                        <div className="text-center text-sm mb-2">
                            <p>
                                Mã OTP đã được gửi đến <strong>{savedEmail}</strong>.
                            </p>
                            <p>Vui lòng kiểm tra email của bạn.</p>
                        </div>
                        {/* <InputStyled
                            variant="bordered"
                            label="Mã OTP"
                            value={otpFormik.values.otp}
                            onValueChange={(value) => otpFormik.setFieldValue("otp", value)}
                            isInvalid={!!(otpFormik.touched.otp && otpFormik.errors.otp)}
                            errorMessage={otpFormik.errors.otp}
                            onBlur={() => otpFormik.setFieldTouched("otp")}
                        /> */}
                        <InputOtp
                            size="lg"
                            length={6}
                            label="Mã OTP"
                            value={otpFormik.values.otp}
                            onValueChange={(value) => otpFormik.setFieldValue("otp", value)}
                            isInvalid={!!(otpFormik.touched.otp && otpFormik.errors.otp)}
                            errorMessage={otpFormik.errors.otp}
                            onBlur={() => otpFormik.setFieldTouched("otp")}
                            className="mx-auto"
                        />
                        <ButtonStyled
                            className="w-full mt-4"
                            type="submit"
                            isLoading={verifyOtpMutation.isPending}
                            color="primary"
                            isDisabled={!otpFormik.isValid}
                            // onPress={() => otpFormik.submitForm()}
                        >
                            Xác thực OTP
                        </ButtonStyled>
                        <div className="text-center mt-2">
                            <Link
                                size="sm"
                                className="cursor-pointer"
                                onPress={handleResendOtp}
                                isDisabled={resendOtpMutation.isPending}
                            >
                                Gửi lại mã OTP
                            </Link>
                        </div>
                        <div className="text-center">
                            <Link
                                size="sm"
                                className="cursor-pointer text-gray-500"
                                onPress={() => setStep(0)}
                            >
                                Chọn email khác
                            </Link>
                        </div>
                    </form>
                )
            case 2:
                return (
                    <form
                        onSubmit={passwordFormik.handleSubmit}
                        className="w-full flex flex-col gap-4"
                    >
                        <InputStyled
                            variant="bordered"
                            label="Mật khẩu mới"
                            type={isVisible ? "text" : "password"}
                            value={passwordFormik.values.password}
                            onValueChange={(value) =>
                                passwordFormik.setFieldValue("password", value)
                            }
                            isInvalid={
                                !!(
                                    passwordFormik.touched.password &&
                                    passwordFormik.errors.password
                                )
                            }
                            errorMessage={passwordFormik.errors.password}
                            onBlur={() => passwordFormik.setFieldTouched("password")}
                            endContent={
                                <ButtonToggleVisibility
                                    isVisible={isVisible}
                                    toggleVisibility={toggleVisibility}
                                />
                            }
                        />
                        <InputStyled
                            variant="bordered"
                            label="Xác nhận mật khẩu"
                            type={isConfirmVisible ? "text" : "password"}
                            value={passwordFormik.values.confirmPassword}
                            onValueChange={(value) =>
                                passwordFormik.setFieldValue("confirmPassword", value)
                            }
                            isInvalid={
                                !!(
                                    passwordFormik.touched.confirmPassword &&
                                    passwordFormik.errors.confirmPassword
                                )
                            }
                            errorMessage={passwordFormik.errors.confirmPassword}
                            onBlur={() => passwordFormik.setFieldTouched("confirmPassword")}
                            endContent={
                                <ButtonToggleVisibility
                                    isVisible={isConfirmVisible}
                                    toggleVisibility={toggleConfirmVisibility}
                                />
                            }
                        />
                        <ButtonStyled
                            className="w-full mt-4"
                            type="submit"
                            isLoading={resetPasswordMutation.isPending}
                            color="primary"
                            isDisabled={!passwordFormik.isValid}
                            // onPress={() => passwordFormik.submitForm()}
                        >
                            Đổi mật khẩu
                        </ButtonStyled>
                    </form>
                )
        }
    }

    return (
        <div className="h-fit w-full max-w-sm mx-auto flex flex-col gap-4">
            <div className="flex w-full flex-col gap-4">
                <div className="flex flex-col items-center pb-6">
                    <LogoStyled className="mb-3" />
                    <p className="text-xl font-medium">
                        {step === 0
                            ? "Quên mật khẩu?"
                            : step === 1
                              ? "Xác thực OTP"
                              : "Đặt lại mật khẩu"}
                    </p>
                    {step === 0 && (
                        <p className="text-small text-default-500 text-center">
                            Nhập email của bạn để đặt lại mật khẩu
                        </p>
                    )}
                </div>

                {renderStepContent()}
            </div>

            <p className="text-small text-center mt-2">
                <Link isBlock onPress={handleBackToLogin} className="cursor-pointer">
                    Quay lại đăng nhập
                </Link>
            </p>
        </div>
    )
}
