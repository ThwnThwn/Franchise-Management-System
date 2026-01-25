"use client"

import { ButtonStyled, ButtonToggleVisibility, InputStyled, LogoStyled } from "@/components"
import { EMAIL_REGEX } from "@/constants/regexs"
import { ERRORS, TEXT } from "@/features/auth/constants"
import { useLogin } from "@/features/auth/hooks/useAuth"
import { Checkbox, Divider } from "@heroui/react"
import { useFormik } from "formik"
import React, { useCallback, useState } from "react"
import * as Yup from "yup"

export function LoginForm({ onSuccess }: { onSuccess?: () => void }) {
    const [isVisible, setIsvisible] = useState(false)
    const [rememberMe, setRememberMe] = useState(true)
    const loginMutation = useLogin({ rememberMe, onSuccess })

    // const { onClose: onCloseLogin } = useLoginDiscloresureSingleton()

    const toggleVisibility = () => setIsvisible(!isVisible)

    const handleLogin = useCallback(
        async (values: { email: string; password: string }) => {
            await loginMutation.mutateAsync(values)
        },
        [loginMutation]
    )

    const formik = useFormik({
        initialValues: {
            email: "",
            password: ""
            // rememberMe: false
        },
        validationSchema: Yup.object().shape({
            email: Yup.string()
                .required(ERRORS.AUTH.EMAIL_REQUIRED)
                .matches(EMAIL_REGEX, ERRORS.AUTH.INVALID_EMAIL),
            password: Yup.string()
                .required(ERRORS.AUTH.PASSWORD_REQUIRED)
                .min(8, ERRORS.AUTH.PASSWORD_TOO_SHORT)
        }),
        onSubmit: handleLogin
    })

    return (
        <div className="h-fit w-full max-w-sm mx-auto flex flex-col gap-4">
            <form
                onSubmit={formik.handleSubmit}
                className="flex h-full w-full items-center justify-center"
            >
                <div className="flex w-full flex-col gap-4">
                    <div className="flex flex-col items-center pb-6">
                        {/* <AcmeIcon size={60} /> */}
                        <LogoStyled className="mb-3" />
                        <p className="text-xl font-medium">{TEXT.STRINGS.WELCOME}</p>
                        <p className="text-small text-default-500">{TEXT.STRINGS.LOGIN_CONTINUE}</p>
                    </div>

                    <div className="flex flex-col gap-3">
                        <InputStyled
                            // className="my-3"
                            variant="bordered"
                            label={TEXT.TITLES.EMAIL}
                            value={formik.values.email}
                            onValueChange={(value) => formik.setFieldValue("email", value)}
                            isInvalid={!!(formik.touched.email && formik.errors.email)}
                            errorMessage={formik.errors.email}
                            onBlur={() => {
                                formik.setFieldTouched("email")
                            }}
                        />
                        <InputStyled
                            variant="bordered"
                            label={TEXT.TITLES.PASSWORD}
                            type={isVisible ? "text" : "password"}
                            value={formik.values.password}
                            onValueChange={(value) => formik.setFieldValue("password", value)}
                            isInvalid={!!(formik.touched.password && formik.errors.password)}
                            errorMessage={formik.errors.password}
                            onBlur={() => {
                                formik.setFieldTouched("password")
                            }}
                            endContent={
                                <ButtonToggleVisibility
                                    isVisible={isVisible}
                                    toggleVisibility={toggleVisibility}
                                />
                            }
                        />
                    </div>

                    <div className="flex w-full items-center justify-between px-1 py-2">
                        <div className="flex flex-col gap-2">
                            {/* <Checkbox
                            isSelected={formik.values.rememberMe}
                            onValueChange={(isSelected) =>
                                formik.setFieldValue("rememberMe", isSelected)
                            }
                        > */}
                            <Checkbox
                                isSelected={rememberMe}
                                onValueChange={(isSelected) => setRememberMe(isSelected)}
                            >
                                {TEXT.STRINGS.REMEMBER_ME}
                            </Checkbox>
                        </div>
                        <div>
                            {/* <Link
                                className="text-default-500 cursor-pointer"
                                size="sm"
                                onPress={handleOpenForgot}
                            >
                                {t("login.forgot")}
                            </Link> */}
                        </div>
                    </div>

                    <ButtonStyled
                        className="w-full"
                        type="submit"
                        isLoading={formik.isSubmitting}
                        color="primary"
                        isDisabled={!formik.isValid}
                        onPress={() => formik.submitForm()}
                    >
                        {TEXT.TITLES.LOGIN}
                    </ButtonStyled>
                </div>
            </form>
            <div className="flex items-center gap-4 py-2">
                <Divider className="flex-1" />
                <p className="text-tiny text-default-500 shrink-0">{TEXT.STRINGS.OR}</p>
                <Divider className="flex-1" />
            </div>
            {/* <div className="flex flex-col gap-2">
                <GoogleLoginButton rememberMe={rememberMe} onSuccess={onSuccess} />
            </div> */}
            <p className="text-small text-center">
                {TEXT.STRINGS.NOT_HAVE_ACCOUNT}&nbsp;
                {/* <Link isBlock onPress={handleOpenRegister} className="cursor-pointer">
                    {t("login.register")}
                </Link> */}
            </p>
        </div>
    )
}
