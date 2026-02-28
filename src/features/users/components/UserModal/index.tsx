"use client"

import { useMemo } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter } from "@heroui/react"
import { useFormik } from "formik"
import { userSchema, UserFormValues } from "./UserForm/schema"
import { ButtonStyled, ModalStyled } from "@/components"
import { UserForm } from "./UserForm"
import { Gender, RoleSlug } from "@/constants/enums"
import { useCreateUser, useUpdateUser } from "@/features/users/hooks"
import { confirmDialog, useTokenStore } from "@/hooks"
import { User } from "@/models/user"

interface UserModalProps {
    isOpen: boolean
    onClose: () => void
    initialData?: User
    manageRoleLabels: Record<string, string>
    defaultManageRole?: RoleSlug
}

export const UserModal = ({
    isOpen,
    onClose,
    initialData,
    manageRoleLabels,
    defaultManageRole
}: UserModalProps) => {
    const currStoreId = useTokenStore((s) => s.storeId) || undefined

    const isEditMode = !!initialData?.id

    const createMutation = useCreateUser({ onSuccess: onClose })
    const updateMutation = useUpdateUser({ onSuccess: onClose })

    const initialValues: UserFormValues = useMemo(() => {
        if (isEditMode && initialData) {
            return {
                id: initialData.id,
                username: initialData.username || "",
                email: initialData.email || "",
                phone: initialData.phone || "",
                firstName: initialData.firstName || "",
                lastName: initialData.lastName || "",
                address: initialData.address || "",
                gender: initialData.gender ?? Gender.Male,
                dateOfBirth: initialData.dateOfBirth || "",
                roleSlug: initialData.role?.slug || RoleSlug.Customer,
                homeStoreId: initialData.homeStoreId || "",
                currentHourlyWage: initialData.currentHourlyWage || 1000
            }
        }
        return {
            id: undefined,
            username: "",
            email: "",
            phone: "",
            firstName: "",
            lastName: "",
            address: "",
            gender: Gender.Male,
            dateOfBirth: "",
            roleSlug: defaultManageRole || RoleSlug.Staff,
            homeStoreId: currStoreId || "",
            currentHourlyWage: 1000
        }
    }, [initialData, isEditMode, defaultManageRole, currStoreId])

    const formik = useFormik<UserFormValues>({
        initialValues,
        validationSchema: userSchema,
        enableReinitialize: true,
        onSubmit: async (values) => {
            if (isEditMode && initialData?.id) {
                if (values.roleSlug !== initialData.role?.slug) {
                    const isConfirm = await confirmDialog({
                        title: "Thông báo",
                        message: "Vai trò có thay đổi, bạn chắc chắn muốn đổi không?",
                        variant: "warning"
                    })
                    if (!isConfirm) return
                }

                await updateMutation.mutateAsync({
                    id: initialData.id,
                    req: {
                        roleSlug: values.roleSlug,
                        homeStoreId: [RoleSlug.Manager, RoleSlug.Staff].includes(values.roleSlug)
                            ? values.homeStoreId
                            : undefined,
                        currentHourlyWage:
                            values.roleSlug === RoleSlug.Staff
                                ? values.currentHourlyWage
                                : undefined
                    }
                })
            } else {
                await createMutation.mutateAsync({
                    username: values.username || "",
                    email: values.email || "",
                    phone: values.phone || "",
                    firstName: values.firstName || "",
                    lastName: values.lastName || "",
                    address: values.address || "",
                    gender: Number(values.gender),
                    dateOfBirth: values.dateOfBirth || "",
                    roleSlug: values.roleSlug,
                    // Logic bóc tách store và wage tương tự
                    homeStoreId: [RoleSlug.Manager, RoleSlug.Staff].includes(values.roleSlug)
                        ? values.homeStoreId
                        : undefined,
                    currentHourlyWage:
                        values.roleSlug === RoleSlug.Staff ? values.currentHourlyWage : undefined
                })
            }
        }
    })

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="2xl">
            <ModalContent>
                <ModalHeader>
                    {isEditMode ? `Cập nhật: ${initialData.username}` : "Tạo người dùng mới"}
                </ModalHeader>
                <ModalBody>
                    <UserForm
                        formik={formik}
                        isEditMode={isEditMode}
                        manageRoleLabels={manageRoleLabels}
                    />
                </ModalBody>
                <ModalFooter>
                    <ButtonStyled
                        variant="light"
                        onPress={onClose}
                        isDisabled={createMutation.isPending || updateMutation.isPending}
                    >
                        Hủy
                    </ButtonStyled>
                    <ButtonStyled
                        color="primary"
                        isLoading={createMutation.isPending || updateMutation.isPending}
                        isDisabled={!formik.dirty}
                        onPress={() => formik.handleSubmit()}
                    >
                        {isEditMode ? "Lưu cập nhật" : "Tạo người dùng"}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
