"use client"

import { ButtonStyled, ButtonToggleVisibility, InputStyled, LogoStyled } from "@/components"
import { useLogin } from "@/features/"
import { AUTH_ERRORS, AUTH_TEXT } from "@/features/auth/constants"
import { useForgotPasswordDiscloresureSingleton, useLoginDiscloresureSingleton } from "@/hooks"
import { Checkbox, Divider, Link } from "@heroui/react"
import { useFormik } from "formik"
import { useRouter } from "next/navigation"
import { useCallback, useState } from "react"
import * as Yup from "yup"

export function LoginForm({ onSuccess }: { onSuccess?: () => void }) {
    const [isVisible, setIsvisible] = useState(false)
    const [rememberMe, setRememberMe] = useState(true)
    const loginMutation = useLogin({ rememberMe, onSuccess })
    const { onClose: onCloseLogin } = useLoginDiscloresureSingleton()
    const { onOpen: onOpenForgotPassword } = useForgotPasswordDiscloresureSingleton()
    // const { onOpen: onOpenRegister } = useRegisterDiscloresureSingleton()
    const router = useRouter()

    const goToRegister = () => {
        // Lưu ý: (public) là Route Group nên nó bị lược bỏ khỏi URL
        onCloseLogin()
        router.push("/auth/register")
    }
    // const { onClose: onCloseLogin } = useLoginDiscloresureSingleton()

    const toggleVisibility = () => setIsvisible(!isVisible)

    const handleLogin = useCallback(
        async (values: { emailOrUsername: string; password: string }) => {
            await loginMutation.mutateAsync(values)
        },
        [loginMutation]
    )

    // const handleOpenRegister = useCallback(() => {
    //     onCloseLogin()
    //     onOpenRegister()
    // }, [onCloseLogin, onOpenRegister])

    const formik = useFormik({
        initialValues: {
            emailOrUsername: "",
            password: ""
            // rememberMe: false
        },
        validationSchema: Yup.object().shape({
            emailOrUsername: Yup.string().required(AUTH_ERRORS.AUTH.EMAIL_REQUIRED),
            password: Yup.string().required(AUTH_ERRORS.AUTH.PASSWORD_REQUIRED)
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
                        <p className="text-xl font-medium">{AUTH_TEXT.STRINGS.WELCOME}</p>
                        {/* <p className="text-small text-default-500">{TEXT.STRINGS.LOGIN_CONTINUE}</p> */}
                    </div>

                    <div className="flex flex-col gap-3">
                        <InputStyled
                            // className="my-3"
                            variant="bordered"
                            label={AUTH_TEXT.TITLES.EMAIL_OR_USERNAME}
                            value={formik.values.emailOrUsername}
                            onValueChange={(value) =>
                                formik.setFieldValue("emailOrUsername", value)
                            }
                            isInvalid={
                                !!(formik.touched.emailOrUsername && formik.errors.emailOrUsername)
                            }
                            errorMessage={formik.errors.emailOrUsername}
                            onBlur={() => {
                                formik.setFieldTouched("emailOrUsername")
                            }}
                            isRequired
                        />
                        <InputStyled
                            variant="bordered"
                            label={AUTH_TEXT.TITLES.PASSWORD}
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
                            isRequired
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
                                {AUTH_TEXT.STRINGS.REMEMBER_ME}
                            </Checkbox>
                        </div>
                        <div>
                            <Link
                                className="text-default-500 cursor-pointer"
                                size="sm"
                                onPress={() => {
                                    onCloseLogin()
                                    // router.push("/auth/forgot-password")
                                    onOpenForgotPassword()
                                }}
                            >
                                Quên mật khẩu?
                            </Link>
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
                        {AUTH_TEXT.TITLES.LOGIN}
                    </ButtonStyled>
                </div>
            </form>
            <div className="flex items-center gap-4 py-2">
                <Divider className="flex-1" />
                <p className="text-tiny text-default-500 shrink-0">{AUTH_TEXT.STRINGS.OR}</p>
                <Divider className="flex-1" />
            </div>
            {/* <div className="flex flex-col gap-2">
                <GoogleLoginButton rememberMe={rememberMe} onSuccess={onSuccess} />
            </div> */}
            <p className="text-small text-center">
                {AUTH_TEXT.STRINGS.NOT_HAVE_ACCOUNT}&nbsp;
                <Link isBlock onPress={goToRegister} className="cursor-pointer">
                    {AUTH_TEXT.STRINGS.REGISTER}
                </Link>
            </p>
        </div>
    )
}
