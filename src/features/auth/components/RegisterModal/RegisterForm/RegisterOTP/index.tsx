"use client"
import { ButtonStyled } from "@/components"
import { AUTH_ERRORS, AUTH_TEXT } from "@/features/auth/constants"
import { useRegisterVerify } from "@/features/auth/hooks"
import { InputOtp } from "@heroui/react"
import { useFormik } from "formik"
import React, { useCallback } from "react"
import * as Yup from "yup"

interface RegisterOTPProps {
    email: string
    onBack: () => void
    onSuccess?: () => void
}

export function RegisterOTP({ email, onBack, onSuccess }: RegisterOTPProps) {
    const registerVerifyMutation = useRegisterVerify({ onSuccess })

    const handleRegisterVerify = useCallback(
        async (values: { otp: string; email: string }) => {
            await registerVerifyMutation.mutateAsync({ ...values })
        },
        [registerVerifyMutation]
    )

    const formik = useFormik({
        initialValues: {
            email: email,
            otp: ""
        },
        validationSchema: Yup.object({
            otp: Yup.string()
                .required(AUTH_ERRORS.AUTH.OTP_REQUIRED)
                .matches(/^[0-9]{6}$/, AUTH_ERRORS.AUTH.INVALID_OTP)
        }),
        onSubmit: handleRegisterVerify
    })

    return (
        <form onSubmit={formik.handleSubmit} className="flex flex-col">
            {/* Title */}
            <div className="mx-12 mt-2 mb-2">
                <div className="text-center">{AUTH_TEXT.STRINGS.VERIFY_IDENTITY}</div>
            </div>
            {/* Input OTP */}
            <div className=" mx-auto">
                <InputOtp
                    size="lg"
                    length={6}
                    value={formik.values.otp}
                    onValueChange={(value) => formik.setFieldValue("otp", value)}
                    isInvalid={!!(formik.touched.otp && formik.errors.otp)}
                    errorMessage={formik.errors.otp}
                    onBlur={() => {
                        formik.setFieldTouched("otp")
                    }}
                />
            </div>

            <div className="flex mx-auto gap-4 mt-4">
                <ButtonStyled onPress={onBack} className="w-2 h-10 mx-auto mt-0">
                    {"Quay lại"}
                </ButtonStyled>

                <ButtonStyled
                    type="submit"
                    color="primary"
                    isLoading={formik.isSubmitting}
                    isDisabled={!formik.isValid}
                    className="w-4 h-10 mx-auto mt-0"
                >
                    {formik.isSubmitting ? "" : "Xác nhận"}
                </ButtonStyled>
            </div>
        </form>
    )
}
