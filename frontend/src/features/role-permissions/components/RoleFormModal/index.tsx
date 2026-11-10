"use client"

import React, { useMemo } from "react"
import { useFormik } from "formik"
import * as Yup from "yup"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Button } from "@heroui/react"
import { ModalStyled, InputStyled, ButtonStyled } from "@/components"
import { useCreateRole, useUpdateRole } from "../../hooks/useRole"
import { RoleResponse } from "../../models/respone"
import { CreateRoleRequest, UpdateRoleRequest } from "../../models/request"
import { useRouter } from "next/navigation"
import { Shield } from "lucide-react"
import { RoleSlug } from "@/constants/enums"

interface RoleFormModalProps {
    isOpen: boolean
    onOpenChange: () => void
    role?: RoleResponse | null
}

export function RoleFormModal({ isOpen, onOpenChange, role }: RoleFormModalProps) {
    const isEditMode = !!role
    const router = useRouter()

    const handleGoToPermissions = () => {
        if (role?.id) {
            router.push(`/dashboard/roles/${role.id}/permissions`)
            onOpenChange()
        }
    }

    // Hook mutation cho Create
    const createMutation = useCreateRole({
        onSuccess: () => onOpenChange()
    })

    // Hook mutation cho Update (truyền ID nếu ở mode edit)
    const updateMutation = useUpdateRole({
        id: role?.id ?? "",
        onSuccess: () => onOpenChange()
    })

    const isLoading = createMutation.isPending || updateMutation.isPending

    // 1. Khởi tạo giá trị ban đầu cho Formik
    const initialValues = useMemo(() => {
        return {
            name: role?.name ?? "",
            slug: role?.slug ?? "",
            description: role?.description ?? ""
        }
    }, [role])

    // 2. Định nghĩa Schema Validation
    const validationSchema = Yup.object({
        name: Yup.string().required("Vui lòng nhập tên vai trò"),
        slug: Yup.string().required("Vui lòng nhập slug (định danh)"),
        description: Yup.string().max(255, "Mô tả không được quá 255 ký tự")
    })

    // 3. Cấu hình Formik
    const formik = useFormik({
        initialValues,
        validationSchema,
        enableReinitialize: true,
        onSubmit: async (values) => {
            if (isEditMode && role) {
                const req: UpdateRoleRequest = {
                    name: values.name,
                    description: values.description
                }
                await updateMutation.mutateAsync(req)
            } else {
                const req: CreateRoleRequest = {
                    name: values.name,
                    slug: values.slug,
                    description: values.description
                }
                await createMutation.mutateAsync(req)
            }
        }
    })

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="xl"
            scrollBehavior="inside"
            onClose={() => formik.resetForm()}
        >
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1 text-xl font-bold">
                            {isEditMode ? "Chỉnh sửa vai trò" : "Thêm vai trò mới"}
                        </ModalHeader>

                        <ModalBody>
                            <form className="flex flex-col gap-4 py-2">
                                <div className="flex flex-col gap-4">
                                    <InputStyled
                                        label="Tên vai trò"
                                        placeholder="VD: Quản trị viên"
                                        isRequired
                                        {...formik.getFieldProps("name")}
                                        isInvalid={!!(formik.touched.name && formik.errors.name)}
                                        errorMessage={formik.errors.name}
                                    />

                                    <InputStyled
                                        label="Mã định danh (Slug)"
                                        placeholder="VD: admin-system"
                                        isRequired
                                        isDisabled={isEditMode}
                                        {...formik.getFieldProps("slug")}
                                        isInvalid={!!(formik.touched.slug && formik.errors.slug)}
                                        errorMessage={formik.errors.slug}
                                    />

                                    <InputStyled
                                        label="Mô tả"
                                        placeholder="Mô tả ngắn gọn về quyền hạn của vai trò này"
                                        {...formik.getFieldProps("description")}
                                        isInvalid={
                                            !!(
                                                formik.touched.description &&
                                                formik.errors.description
                                            )
                                        }
                                        errorMessage={formik.errors.description}
                                    />
                                </div>
                            </form>
                        </ModalBody>

                        <ModalFooter>
                            {isEditMode && role.slug !== RoleSlug.Admin ? (
                                <ButtonStyled
                                    variant="bordered"
                                    startContent={<Shield size={18} />}
                                    onPress={handleGoToPermissions}
                                >
                                    Thiết lập quyền chi tiết
                                </ButtonStyled>
                            ) : (
                                <div />
                            )}
                            <Button
                                color="danger"
                                variant="light"
                                onPress={onClose}
                                isDisabled={isLoading}
                            >
                                Hủy bỏ
                            </Button>
                            <ButtonStyled
                                color="primary"
                                isLoading={isLoading}
                                onPress={() => formik.handleSubmit()}
                            >
                                {isEditMode ? "Cập nhật" : "Tạo vai trò"}
                            </ButtonStyled>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
