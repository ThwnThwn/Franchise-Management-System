"use client"
import { ButtonStyled, ButtonToggleVisibility, DatePickerStyled, InputStyled } from "@/components"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import { Gender } from "@/constants/enums"
import { GenderLabels } from "@/constants/labels"
import { EMAIL_REGEX, NAME_REGEX, PHONE_REGEX } from "@/constants/regexs"
import { AUTH_ERRORS, AUTH_TEXT } from "@/features/auth/constants"
import { useRegister } from "@/features/auth/hooks"
import { UserRegisterReq } from "@/features/auth/models/request"
import { useDay } from "@/hooks"
import { EnumPicker } from "@/components/modules/EnumPicker"
import { useFormik } from "formik"
import { number } from "framer-motion"
import React, { useCallback, useState } from "react"
import * as Yup from "yup"

interface RegisterInfoProps {
    email: string
    setEmail: (email: string) => void
    onSuccess?: () => void
}
export function Register({ email, setEmail, onSuccess }: RegisterInfoProps) {
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })
    const registerMutation = useRegister({ onSuccess })

    const [isVisible, setIsVisible] = useState(false)
    const toggleVisibility = () => setIsVisible(!isVisible)
    const [isConfirmVisible, setIsConfirmVisible] = useState(false)
    const toggleConFirmVisibility = () => setIsConfirmVisible(!isConfirmVisible)
    const handleRegister = useCallback(
        async (values: UserRegisterReq) => {
            await registerMutation.mutateAsync(values)
            setEmail(values.email)
        },
        [registerMutation, setEmail]
    )

    const formik = useFormik({
        initialValues: {
            email: email,
            userName: "",
            lastName: "",
            firstName: "",
            password: "",
            confirmPassword: "",
            dateOfBirth: "",
            phone: "",
            address: "",
            gender: Gender.Male
        },
        validationSchema: Yup.object({
            email: Yup.string()
                .required(AUTH_ERRORS.AUTH.EMAIL_REQUIRED)
                .matches(EMAIL_REGEX, AUTH_ERRORS.AUTH.INVALID_EMAIL),
            userName: Yup.string().required(AUTH_ERRORS.AUTH.USER_NAME_REQUIRED),
            lastName: Yup.string()
                // .required(AUTH_ERRORS.AUTH.LAST_NAME_REQUIRED)
                .matches(NAME_REGEX, AUTH_ERRORS.AUTH.INVALID_LAST_NAME),
            firstName: Yup.string()
                .required(AUTH_ERRORS.AUTH.FIRST_NAME_REQUIRED)
                .matches(NAME_REGEX, AUTH_ERRORS.AUTH.INVALID_FIRST_NAME),
            password: Yup.string().required(AUTH_ERRORS.AUTH.PASSWORD_REQUIRED),
            // .min(8, AUTH_ERRORS.AUTH.PASSWORD_TOO_SHORT)
            // .matches(PASSWORD_REGEX, AUTH_ERRORS.AUTH.PASSWORD_MUST_STRONGER),
            confirmPassword: Yup.string()
                .oneOf([Yup.ref("password")], AUTH_ERRORS.AUTH.CONFIRM_PASSWORD_NOT_MATCHED)
                .required(AUTH_ERRORS.AUTH.PASSWORD_REQUIRED),
            address: Yup.string().max(255, AUTH_ERRORS.AUTH.ADDRESS_TO_LONG),
            dateOfBirth: Yup.string().required(AUTH_ERRORS.AUTH.DATE_OF_BIRTH_REQUIRED),
            phone: Yup.string()
                .required(AUTH_ERRORS.AUTH.PHONE_REQUIRED)
                .matches(PHONE_REGEX, AUTH_ERRORS.AUTH.INVALID_PHONE),
            gender: Yup.number().required(AUTH_ERRORS.AUTH.GENDER)
        }),
        onSubmit: handleRegister
    })

    return (
        <form onSubmit={formik.handleSubmit} className="flex flex-col gap-2 w-full max-w-110 ">
            {/* Title */}
            <div className="mx-auto mt-2">
                <div className="text-center">{AUTH_TEXT.STRINGS.REGISTER_INFO}</div>
            </div>

            {/* Input InFo */}
            <div className="w-full mx-auto space-y-2 ">
                <InputStyled
                    // className="my-3"
                    variant="bordered"
                    label={AUTH_TEXT.TITLES.EMAIL}
                    value={formik.values.email}
                    onValueChange={(value) => formik.setFieldValue("email", value)}
                    isInvalid={!!(formik.touched.email && formik.errors.email)}
                    errorMessage={formik.errors.email}
                    onBlur={() => {
                        formik.setFieldTouched("email")
                    }}
                />
            </div>

            <div className="w-full mx-auto space-y-2">
                <InputStyled
                    // className="my-3"
                    variant="bordered"
                    label={AUTH_TEXT.TITLES.USERNAME}
                    value={formik.values.userName}
                    onValueChange={(value) => formik.setFieldValue("userName", value)}
                    isInvalid={!!(formik.touched.userName && formik.errors.userName)}
                    errorMessage={formik.errors.userName}
                    onBlur={() => {
                        formik.setFieldTouched("userName")
                    }}
                />
            </div>

            <div className="flex mx-auto w-full gap-2">
                <InputStyled
                    label={AUTH_TEXT.TITLES.FIRSTNAME}
                    variant="bordered"
                    value={formik.values.firstName}
                    onValueChange={(value) => formik.setFieldValue("firstName", value)}
                    isInvalid={!!(formik.touched.firstName && formik.errors.firstName)}
                    errorMessage={formik.errors.firstName}
                    onBlur={() => {
                        formik.setFieldTouched("firstName")
                    }}
                />

                <InputStyled
                    label={AUTH_TEXT.TITLES.LASTNAME}
                    variant="bordered"
                    value={formik.values.lastName}
                    onValueChange={(value) => formik.setFieldValue("lastName", value)}
                    isInvalid={!!(formik.touched.lastName && formik.errors.lastName)}
                    errorMessage={formik.errors.lastName}
                    onBlur={() => {
                        formik.setFieldTouched("lastName")
                    }}
                />
            </div>

            <div className="w-full mx-auto space-y-2">
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
                />

                <InputStyled
                    variant="bordered"
                    label={AUTH_TEXT.TITLES.CONFIRM_PASSWORD}
                    type={isConfirmVisible ? "text" : "password"}
                    value={formik.values.confirmPassword}
                    onValueChange={(value) => formik.setFieldValue("confirmPassword", value)}
                    errorMessage={formik.errors.confirmPassword}
                    onBlur={() => {
                        formik.setFieldTouched("confirmPassword")
                    }}
                    isInvalid={!!(formik.touched.confirmPassword && formik.errors.confirmPassword)}
                    endContent={
                        <ButtonToggleVisibility
                            isVisible={isConfirmVisible}
                            toggleVisibility={toggleConFirmVisibility}
                        />
                    }
                />
                <div className="w-full mx-auto space-y-2">
                    <InputStyled
                        // className="my-3"
                        variant="bordered"
                        label={AUTH_TEXT.TITLES.ADDRESS}
                        value={formik.values.address}
                        onValueChange={(value) => formik.setFieldValue("address", value)}
                        isInvalid={!!(formik.touched.address && formik.errors.address)}
                        errorMessage={formik.errors.address}
                        onBlur={() => {
                            formik.setFieldTouched("address")
                        }}
                    />
                </div>
                <InputStyled
                    variant="bordered"
                    label={AUTH_TEXT.TITLES.PHONE_NUMBER}
                    maxLength={10}
                    pattern="[0-9]*"
                    onInput={(e) => {
                        e.currentTarget.value = e.currentTarget.value.replace(/[^0-9]/g, "")
                    }}
                    value={formik.values.phone}
                    onValueChange={(value) => formik.setFieldValue("phone", value)}
                    isInvalid={!!(formik.touched.phone && formik.errors.phone)}
                    errorMessage={formik.errors.phone}
                    onBlur={() => {
                        formik.setFieldTouched("phone")
                    }}
                />
            </div>

            <div className="flex mx-auto w-full gap-2">
                <EnumPicker
                    label={AUTH_TEXT.TITLES.GENDER}
                    labels={GenderLabels}
                    value={formik.values.gender}
                    onChange={(val) =>
                        formik.setFieldValue("gender", number.parse(val?.toString() ?? "0"))
                    }
                />
                <DatePickerStyled
                    label={AUTH_TEXT.TITLES.DATE_OF_BIRTH}
                    isInvalid={!!(formik.touched.dateOfBirth && formik.errors.dateOfBirth)}
                    errorMessage={formik.errors.dateOfBirth}
                    onChange={(value) => {
                        if (!value) {
                            formik.setFieldValue("dateOfBirth", null)
                            return
                        }

                        const dob = formatDateTime({ date: value })
                        formik.setFieldValue("dateOfBirth", dob)
                    }}
                />
            </div>

            <ButtonStyled
                type="submit"
                className="w-full h-10 mx-auto my-4"
                isLoading={formik.isSubmitting}
                color="primary"
                isDisabled={!formik.isValid}
            >
                {AUTH_TEXT.STRINGS.REGISTER}
            </ButtonStyled>
        </form>
    )
}
