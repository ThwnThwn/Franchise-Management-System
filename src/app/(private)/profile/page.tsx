"use client"

import {
    ButtonIconStyled,
    ButtonStyled,
    DatePickerStyled,
    InputStyled
    // SpinnerStyled
} from "@/components"
import { Gender } from "@/constants/enums"
import { GenderLabels } from "@/constants/labels"
import { NAME_REGEX, PHONE_REGEX } from "@/constants/regexs"
import { AUTH_ERRORS } from "@/features"
import { useGetMe, useUpdateMe } from "@/features/profile/hooks/useProfile"
import { UserUpdateReq } from "@/features/profile/models/request"
import { useDay } from "@/hooks"
import { useUserHelper } from "@/hooks/reuseable/useUserHelper"
import { EnumPicker } from "@/components/modules/EnumPicker"
import { useFormik } from "formik"
import { Pencil } from "lucide-react"
import React, { useCallback, useState } from "react"
import * as Yup from "yup"

export default function ProfilePage() {
    const [editable, setEditable] = useState(false)
    const updateMeMutation = useUpdateMe({ onSuccess: undefined })
    const { toFullName } = useUserHelper()
    const { data: user } = useGetMe()
    const { toDate, formatDateTime } = useDay({ defaultFormat: "YYYY-MM-DD" })
    // ===== Update Me =====
    const handleUpdateMe = useCallback(
        async (values: UserUpdateReq) => {
            await updateMeMutation.mutateAsync(values)
            setEditable(!editable)
        },
        [updateMeMutation, editable, setEditable]
    )

    const updateMeFormik = useFormik({
        enableReinitialize: true,
        initialValues: {
            firstName: user?.firstName || "",
            lastName: user?.lastName || "",
            phone: user?.phone || undefined,
            gender: user?.gender || Gender.Male,
            dateOfBirth: user?.dateOfBirth || "",
            address: user?.address || ""
        },
        validationSchema: Yup.object({
            firstName: Yup.string()
                .required(AUTH_ERRORS.AUTH.FIRST_NAME_REQUIRED)
                .matches(NAME_REGEX, AUTH_ERRORS.AUTH.INVALID_EMAIL),
            phone: Yup.string()
                .required("user.phone_require")
                .matches(PHONE_REGEX, "user.invalid_phone"),
            gender: Yup.number().required("user.sex_require"),
            dateOfBirth: Yup.string().required("user.date_of_birth_require"),
            address: Yup.string().max(100, "hahahaha")
        }),
        onSubmit: handleUpdateMe
    })

    // if (!user) return <SpinnerStyled />

    return (
        <>
            <div className="bg-white w-full lg:w-5xl max-w-screen overflow-y-auto px-4 sm:px-14 py-6 shadow-2xs rounded-2xl">
                {/* Title */}
                <div className="text-2xl text-center sm:text-left sm:text-3xl mb-8 font-bold">
                    {"user.account_information"}
                </div>

                <div className="flex flex-col md:flex-row gap-6 w-full">
                    <div
                        className="flex flex-col items-center w-full md:w-sm"
                        id="upload-profile-info"
                    >
                        {/* Preview info */}
                        <div className="mt-3 space-y-2 sm:mt-1">
                            {/* Top container */}
                            <div className="flex gap-3 justify-between items-center">
                                {/* user full name */}
                                <div>
                                    <div
                                        className="text-3xl min-h-10" //
                                    >
                                        {toFullName({
                                            firstName: user == null ? "" : user.firstName,
                                            lastName: user == null ? "" : user.lastName
                                        })}
                                    </div>
                                </div>

                                {/* Button enable show change */}
                                {!editable && (
                                    <ButtonIconStyled
                                        color="primary"
                                        variant="ghost"
                                        onPress={() => setEditable(!editable)}
                                    >
                                        <Pencil />
                                    </ButtonIconStyled>
                                )}
                            </div>

                            {/* Form for update */}
                            <form
                                onSubmit={updateMeFormik.submitForm}
                                className="flex flex-col gap-2 w-full"
                            >
                                <InputStyled
                                    isReadOnly={true}
                                    label={"auth.email"}
                                    variant="bordered"
                                    value={user == null ? "" : user.email || "-"}
                                />
                                <InputStyled
                                    isReadOnly={true}
                                    label={"auth.username"}
                                    variant="bordered"
                                    value={user == null ? "" : user.userName || "-"}
                                />
                                <InputStyled
                                    isReadOnly={!editable}
                                    label={"user.last_name"}
                                    variant="bordered"
                                    value={updateMeFormik.values.lastName}
                                    onValueChange={(value) =>
                                        updateMeFormik.setFieldValue("lastName", value)
                                    }
                                    isInvalid={
                                        editable &&
                                        !!(
                                            updateMeFormik.touched.lastName &&
                                            updateMeFormik.errors.lastName
                                        )
                                    }
                                    errorMessage={updateMeFormik.errors.lastName}
                                    onBlur={() => {
                                        updateMeFormik.setFieldTouched("lastName")
                                    }}
                                />

                                <InputStyled
                                    isReadOnly={!editable}
                                    label={"user.first_name"}
                                    variant="bordered"
                                    value={updateMeFormik.values.firstName}
                                    onValueChange={(value) =>
                                        updateMeFormik.setFieldValue("firstName", value)
                                    }
                                    isInvalid={
                                        editable &&
                                        !!(
                                            updateMeFormik.touched.firstName &&
                                            updateMeFormik.errors.firstName
                                        )
                                    }
                                    errorMessage={updateMeFormik.errors.firstName}
                                    onBlur={() => {
                                        updateMeFormik.setFieldTouched("firstName")
                                    }}
                                />

                                <div className="flex gap-2">
                                    {/* Phone */}
                                    <InputStyled
                                        isRequired
                                        isReadOnly={!editable}
                                        variant="bordered"
                                        label={"user.phone"}
                                        maxLength={10}
                                        value={updateMeFormik.values.phone}
                                        onValueChange={(value) =>
                                            updateMeFormik.setFieldValue("phone", value)
                                        }
                                        isInvalid={
                                            editable &&
                                            !!(
                                                updateMeFormik.touched.phone &&
                                                updateMeFormik.errors.phone
                                            )
                                        }
                                        errorMessage={updateMeFormik.errors.phone}
                                        onBlur={() => {
                                            updateMeFormik.setFieldTouched("phone")
                                        }}
                                    />

                                    <EnumPicker
                                        isReadOnly={!editable}
                                        label={"user.gender"}
                                        labels={GenderLabels}
                                        value={updateMeFormik.values.gender}
                                        onChange={(val) =>
                                            updateMeFormik.setFieldValue("gender", val)
                                        }
                                        isClearable={false}
                                    />
                                </div>
                                <InputStyled
                                    isReadOnly={!editable}
                                    label={"user.address"}
                                    variant="bordered"
                                    value={updateMeFormik.values.address}
                                    onValueChange={(value) =>
                                        updateMeFormik.setFieldValue("address", value)
                                    }
                                    isInvalid={
                                        editable &&
                                        !!(
                                            updateMeFormik.touched.address &&
                                            updateMeFormik.errors.address
                                        )
                                    }
                                    errorMessage={updateMeFormik.errors.address}
                                    onBlur={() => {
                                        updateMeFormik.setFieldTouched("address")
                                    }}
                                />
                                <DatePickerStyled
                                    isRequired
                                    isReadOnly={!editable}
                                    label={"user.date_of_birth"}
                                    isInvalid={
                                        editable &&
                                        !!(
                                            updateMeFormik.touched.dateOfBirth &&
                                            updateMeFormik.errors.dateOfBirth
                                        )
                                    }
                                    errorMessage={updateMeFormik.errors.dateOfBirth}
                                    value={
                                        updateMeFormik.values.dateOfBirth
                                            ? toDate(updateMeFormik.values.dateOfBirth)
                                            : null
                                    }
                                    onChange={(value) => {
                                        if (!value) {
                                            updateMeFormik.setFieldValue("dateOfBirth", null)
                                            return
                                        }

                                        const dob = formatDateTime({ date: value })

                                        updateMeFormik.setFieldValue("dateOfBirth", dob)
                                    }}
                                />

                                {editable && (
                                    <div className="flex justify-end gap-2">
                                        <ButtonStyled
                                            color="primary"
                                            variant="ghost"
                                            isLoading={updateMeFormik.isSubmitting}
                                            isDisabled={
                                                !updateMeFormik.isValid || !updateMeFormik.dirty
                                            }
                                            onPress={updateMeFormik.submitForm}
                                        >
                                            {"common.save"}
                                        </ButtonStyled>
                                        <ButtonStyled
                                            isDisabled={updateMeFormik.isSubmitting}
                                            onPress={() => {
                                                setEditable(!editable)
                                                updateMeFormik.resetForm()
                                            }}
                                        >
                                            {"common.cancel"}
                                        </ButtonStyled>
                                    </div>
                                )}
                            </form>
                        </div>
                    </div>

                    <div className="hidden md:block w-1.25 bg-default self-stretch"></div>
                </div>
                {/* <BankInfoProfile user={user} /> */}
            </div>
        </>
    )
}
