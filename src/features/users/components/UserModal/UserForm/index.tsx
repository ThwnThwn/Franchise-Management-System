"use client"

import React, { useEffect, useMemo } from "react"
import { useFormik } from "formik"
import { UserFormValues } from "./schema"
import { InputStyled, EnumPicker, AutocompleteStyled } from "@/components"
import { useFormikHelpers, useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { useGetInfiniteStores } from "@/features/stores"
import { useInView } from "react-intersection-observer"
import { AutocompleteItem } from "@heroui/react"

interface UserFormProps {
    formik: ReturnType<typeof useFormik<UserFormValues>>
    isEditMode: boolean
    manageRoleLabels: Record<string, string>
}

export const UserForm = ({ formik, isEditMode, manageRoleLabels }: UserFormProps) => {
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    // Infinite scroll cho Stores
    const { ref, inView } = useInView()
    const {
        data: storesData,
        fetchNextPage,
        hasNextPage,
        isFetchingNextPage
    } = useGetInfiniteStores({
        params: {},
        pageSize: 10,
        enabled: isAdmin
    })

    useEffect(() => {
        if (inView && hasNextPage && !isFetchingNextPage) fetchNextPage()
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [inView, hasNextPage, isFetchingNextPage])

    const stores = useMemo(() => storesData?.pages.flatMap((p) => p.items) ?? [], [storesData])

    const showWageInput = formik.values.roleSlug === RoleSlug.Staff
    const showStoreInput = [RoleSlug.Manager, RoleSlug.Staff].includes(formik.values.roleSlug)

    return (
        <div className="space-y-4">
            {/* 1. Thông tin cá nhân - ReadOnly khi Edit */}
            <div className="grid grid-cols-2 gap-3">
                <InputStyled
                    label="Họ"
                    {...formik.getFieldProps("lastName")}
                    isReadOnly={isEditMode}
                    // isInvalid={isInvalid("lastName")}
                    // errorMessage={getErrorMessage("lastName")}
                />
                <InputStyled
                    label="Tên"
                    {...formik.getFieldProps("firstName")}
                    isReadOnly={isEditMode}
                    isRequired
                    isInvalid={isInvalid("firstName")}
                    errorMessage={getErrorMessage("firstName")}
                />
            </div>

            <InputStyled
                label="Tên đăng nhập"
                {...formik.getFieldProps("username")}
                isReadOnly={isEditMode}
                isRequired
                isInvalid={isInvalid("username")}
                errorMessage={getErrorMessage("username")}
            />

            <div className="grid grid-cols-2 gap-3">
                <InputStyled
                    label="Email"
                    {...formik.getFieldProps("email")}
                    isReadOnly={isEditMode}
                    isRequired
                    isInvalid={isInvalid("email")}
                    errorMessage={getErrorMessage("email")}
                />
                <InputStyled
                    label="Số điện thoại"
                    {...formik.getFieldProps("phone")}
                    isReadOnly={isEditMode}
                    isRequired
                    isInvalid={isInvalid("phone")}
                    errorMessage={getErrorMessage("phone")}
                />
            </div>

            <InputStyled
                label="Địa chỉ"
                {...formik.getFieldProps("address")}
                isReadOnly={isEditMode}
            />

            {/* 2. Thông tin công việc - Luôn được sửa (trừ khi không có quyền) */}
            <div className="bg-primary-50/30 p-4 rounded-2xl space-y-4 border border-primary-100">
                <p className="text-xs font-bold text-primary uppercase tracking-wider">
                    Thông tin nhân sự
                </p>

                <EnumPicker
                    label="Vai trò"
                    labels={manageRoleLabels}
                    value={formik.values.roleSlug}
                    onChange={(val) => formik.setFieldValue("roleSlug", val)}
                    isClearable={false}
                    isRequired
                />

                {isAdmin && showStoreInput && (
                    <div>
                        <AutocompleteStyled
                            label="Cửa hàng trực thuộc"
                            items={stores}
                            selectedKey={formik.values.homeStoreId}
                            onSelectionChange={(key) => formik.setFieldValue("homeStoreId", key)}
                            isInvalid={isInvalid("homeStoreId")}
                            errorMessage={getErrorMessage("homeStoreId")}
                            isRequired
                        >
                            {(stores ?? []).map((item) => (
                                <AutocompleteItem key={item.id}>
                                    {`${item.name} - ${item.address}`}
                                </AutocompleteItem>
                            ))}
                        </AutocompleteStyled>
                        <div ref={ref} className="h-1" />
                    </div>
                )}

                {showWageInput && (
                    <InputStyled
                        label="Lương mỗi giờ (VNĐ)"
                        type="number"
                        {...formik.getFieldProps("currentHourlyWage")}
                        isInvalid={isInvalid("currentHourlyWage")}
                        errorMessage={getErrorMessage("currentHourlyWage")}
                        isRequired
                    />
                )}
            </div>
        </div>
    )
}
