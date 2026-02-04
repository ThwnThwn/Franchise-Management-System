"use client"

import React, { useCallback } from "react"
import { Gender, RoleSlug } from "@/constants/enums"
import { useCreateUser } from "@/features/dashboard/hooks"
import { CreateUserRequest } from "@/features/dashboard/models/user/request"
import { useDay } from "@/hooks"
import * as Yup from "yup"
import { useFormik } from "formik"
import { EMAIL_REGEX, NAME_REGEX, PHONE_REGEX } from "@/constants/regexs"
import { ButtonStyled, DatePickerStyled, EnumPicker, InputStyled } from "@/components"
import { Spinner } from "@heroui/react"
import { GenderLabels } from "@/constants/labels"

export interface CreateUserFormProps {
    isCreateCustomer?: boolean
    createRoleSlug?: RoleSlug
    createMutation: ReturnType<typeof useCreateUser>
}

export function CreateUserForm({
    isCreateCustomer = true,
    createRoleSlug,
    createMutation
}: CreateUserFormProps) {
    const { formatDateTime } = useDay()

    const handleSubmit = useCallback(
        async (value: CreateUserRequest) => {
            console.log(formatDateTime({ date: value.dateOfBirth }))

            await createMutation.mutateAsync({
                ...value,
                gender: Number(value.gender),
                dateOfBirth: formatDateTime({ date: value.dateOfBirth })
            })
        },
        [createMutation, formatDateTime]
    )

    // Schema Validation - Đã sửa message thành tiếng Việt
    const validationSchema = Yup.object({
        username: Yup.string()
            .required("Vui lòng nhập tên đăng nhập")
            .min(3, "Tên đăng nhập phải có ít nhất 3 ký tự"),
        phone: Yup.string()
            .required("Vui lòng nhập số điện thoại")
            .matches(PHONE_REGEX, "Số điện thoại không hợp lệ"),
        firstName: Yup.string()
            .required("Vui lòng nhập tên")
            .matches(NAME_REGEX, "Tên không được chứa ký tự đặc biệt hoặc số"),
        lastName: Yup.string()
            // .required("Vui lòng nhập họ")
            .matches(NAME_REGEX, "Họ không được chứa ký tự đặc biệt hoặc số"),
        gender: Yup.number().required("Vui lòng chọn giới tính"),
        dateOfBirth: Yup.string().required("Vui lòng chọn ngày sinh"),
        address: Yup.string().required("Vui lòng nhập địa chỉ"),

        email: isCreateCustomer
            ? Yup.string().notRequired()
            : Yup.string()
                  .required("Vui lòng nhập email")
                  .matches(EMAIL_REGEX, "Email không đúng định dạng")
    })

    const formik = useFormik<CreateUserRequest>({
        initialValues: {
            username: "",
            email: "",
            phone: "",
            address: "",
            firstName: "",
            lastName: "",
            gender: Gender.Male,
            dateOfBirth: "",
            roleSlug: (createRoleSlug as unknown as RoleSlug) || RoleSlug.Customer
        },
        enableReinitialize: true,
        validationSchema: validationSchema,
        onSubmit: handleSubmit
    })

    return (
        <form onSubmit={formik.handleSubmit} className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {/* --- USERNAME --- */}
            <InputStyled
                label="Tên đăng nhập"
                variant="bordered"
                value={formik.values.username}
                onValueChange={(value) => formik.setFieldValue("username", value)}
                isInvalid={!!(formik.touched.username && formik.errors.username)}
                errorMessage={formik.errors.username}
                onBlur={() => formik.setFieldTouched("username")}
                isRequired
            />

            {/* --- EMAIL --- */}
            {!isCreateCustomer ? (
                <InputStyled
                    label="Email"
                    variant="bordered"
                    value={formik.values.email}
                    onValueChange={(value) => formik.setFieldValue("email", value)}
                    isInvalid={!!(formik.touched.email && formik.errors.email)}
                    errorMessage={formik.errors.email}
                    onBlur={() => formik.setFieldTouched("email")}
                    isRequired
                />
            ) : (
                <InputStyled
                    label="Email"
                    variant="bordered"
                    value={formik.values.email}
                    onValueChange={(value) => formik.setFieldValue("email", value)}
                    isInvalid={!!(formik.touched.email && formik.errors.email)}
                    errorMessage={formik.errors.email}
                    placeholder="Email (Tùy chọn)"
                />
            )}

            {/* --- FIRST NAME (TÊN) --- */}
            <InputStyled
                label="Tên"
                variant="bordered"
                value={formik.values.firstName}
                onValueChange={(value) => formik.setFieldValue("firstName", value)}
                isInvalid={!!(formik.touched.firstName && formik.errors.firstName)}
                errorMessage={formik.errors.firstName}
                onBlur={() => formik.setFieldTouched("firstName")}
                isRequired
            />

            {/* --- LAST NAME (HỌ) --- */}
            <InputStyled
                label="Họ"
                variant="bordered"
                value={formik.values.lastName}
                onValueChange={(value) => formik.setFieldValue("lastName", value)}
                isInvalid={!!(formik.touched.lastName && formik.errors.lastName)}
                errorMessage={formik.errors.lastName}
                onBlur={() => formik.setFieldTouched("lastName")}
                // isRequired
            />

            {/* --- PHONE --- */}
            <InputStyled
                label="Số điện thoại"
                variant="bordered"
                maxLength={10}
                pattern="[0-9]*"
                onInput={(e) => {
                    e.currentTarget.value = e.currentTarget.value.replace(/[^0-9]/g, "")
                }}
                value={formik.values.phone}
                onValueChange={(value) => formik.setFieldValue("phone", value)}
                isInvalid={!!(formik.touched.phone && formik.errors.phone)}
                errorMessage={formik.errors.phone}
                onBlur={() => formik.setFieldTouched("phone")}
                isRequired
            />

            {/* --- ADDRESS --- */}
            <InputStyled
                label="Địa chỉ"
                variant="bordered"
                value={formik.values.address}
                onValueChange={(value) => formik.setFieldValue("address", value)}
                isInvalid={!!(formik.touched.address && formik.errors.address)}
                errorMessage={formik.errors.address}
                onBlur={() => formik.setFieldTouched("address")}
                isRequired
            />

            {/* --- GENDER --- */}
            <EnumPicker
                label="Giới tính"
                labels={GenderLabels}
                value={formik.values.gender}
                onChange={(val) => formik.setFieldValue("gender", val)}
                isClearable={false}
                isRequired
            />

            {/* --- DATE OF BIRTH --- */}
            <DatePickerStyled
                label="Ngày sinh"
                isInvalid={!!(formik.touched.dateOfBirth && formik.errors.dateOfBirth)}
                isRequired
                errorMessage={formik.errors.dateOfBirth}
                onChange={(value) => {
                    if (!value) {
                        formik.setFieldValue("dateOfBirth", "")
                        return
                    }
                    const dob = formatDateTime({ date: value })
                    formik.setFieldValue("dateOfBirth", dob)
                }}
            />

            {/* --- SUBMIT BUTTON --- */}
            <div className="text-center col-span-1 sm:col-span-2 mt-4">
                <ButtonStyled
                    type="submit"
                    isLoading={formik.isSubmitting}
                    color="primary"
                    isDisabled={!formik.isValid || !formik.dirty}
                    className="w-full sm:w-1/2"
                >
                    {formik.isSubmitting ? <Spinner color="white" size="sm" /> : "Tạo mới"}
                </ButtonStyled>
            </div>
        </form>
    )
}
