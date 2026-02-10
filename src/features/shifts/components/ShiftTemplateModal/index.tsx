"use client"

import React, { useEffect } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter } from "@heroui/react"
import { useFormik } from "formik"
import * as Yup from "yup"
import { CreateShiftTemplateRequest } from "../../models/request"
import { ShiftTemplateResponse } from "../../models/response"
import { useCreateShiftTemplate, useUpdateShiftTemplate } from "@/features/shifts/hooks"
import { ButtonStyled, InputStyled, ModalStyled } from "@/components"
import { HOUR_MINUTE_REGEX } from "@/constants/regexs"

// 1. Validation Schema
const validationSchema = Yup.object({
    code: Yup.string().required("Mã ca không được để trống"),
    name: Yup.string().required("Tên ca không được để trống"),
    startTime: Yup.string()
        .required("Vui lòng chọn giờ")
        .matches(HOUR_MINUTE_REGEX, "Giờ không hợp lệ"),
    endTime: Yup.string()
        .required("Vui lòng chọn giờ")
        .matches(HOUR_MINUTE_REGEX, "Giờ không hợp lệ"),
    breakDurationMinutes: Yup.number()
        .min(0, "Thời gian nghỉ phải >= 0")
        .required("Nhập 0 nếu không nghỉ")
})

interface ShiftTemplateModalProps {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    selectedTemplate?: ShiftTemplateResponse
    storeId: string
}

// Giá trị mặc định ban đầu
const INITIAL_VALUES: CreateShiftTemplateRequest = {
    storeId: "",
    code: "",
    name: "",
    startTime: "08:00",
    endTime: "16:00",
    breakDurationMinutes: 0
}

export const ShiftTemplateModal = ({
    isOpen,
    onOpenChange,
    onClose,
    selectedTemplate,
    storeId
}: ShiftTemplateModalProps) => {
    // 2. Setup Hooks
    const createMutation = useCreateShiftTemplate({ onSuccess: () => handleClose() })
    const updateMutation = useUpdateShiftTemplate({ onSuccess: () => handleClose() })

    const isEdit = !!selectedTemplate
    const isLoading = createMutation.isPending || updateMutation.isPending

    // 3. Setup Formik (Tắt enableReinitialize để tối ưu performance)
    const formik = useFormik<CreateShiftTemplateRequest>({
        initialValues: INITIAL_VALUES,
        validationSchema: validationSchema,
        onSubmit: (values) => {
            // Format data
            const submitData = {
                ...values,
                storeId: storeId, // Đảm bảo storeId luôn đúng
                startTime:
                    values.startTime.length === 5 ? `${values.startTime}:00` : values.startTime,
                endTime: values.endTime.length === 5 ? `${values.endTime}:00` : values.endTime
            }

            if (isEdit && selectedTemplate) {
                updateMutation.mutate({
                    id: selectedTemplate.id,
                    req: submitData
                })
            } else {
                createMutation.mutate(submitData)
            }
        }
    })

    // 4. Effect: Chỉ cập nhật form khi Modal thực sự MỞ
    useEffect(() => {
        if (isOpen) {
            if (selectedTemplate) {
                // Mode EDIT: Fill data vào form
                formik.setValues({
                    storeId: storeId,
                    code: selectedTemplate.code,
                    name: selectedTemplate.name,
                    startTime: selectedTemplate.startTime.slice(0, 5),
                    endTime: selectedTemplate.endTime.slice(0, 5),
                    breakDurationMinutes: selectedTemplate.breakDurationMinutes
                })
            } else {
                formik.resetForm({ values: { ...INITIAL_VALUES, storeId } })
            }
        } // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, selectedTemplate, storeId])

    // 5. Custom Close Handler
    const handleClose = () => {
        // if (isLoading) return

        onClose()
        formik.resetForm()
    }

    // Helper UI check lỗi
    const isInvalid = (field: keyof CreateShiftTemplateRequest) =>
        !!formik.errors[field] && !!formik.touched[field]

    const getErrorMessage = (field: keyof CreateShiftTemplateRequest) =>
        isInvalid(field) ? formik.errors[field] : undefined

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="lg"
            placement="center"
            backdrop="blur"
        >
            <ModalContent>
                <form onSubmit={formik.handleSubmit}>
                    <ModalHeader className="flex flex-col gap-1 text-xl font-bold">
                        {isEdit ? "Cập nhật Mẫu Ca" : "Tạo Mẫu Ca Mới"}
                    </ModalHeader>

                    <ModalBody className="gap-4">
                        <div className="grid grid-cols-2 gap-4">
                            <InputStyled
                                label="Mã Ca"
                                placeholder="VD: CA-SANG"
                                variant="bordered"
                                name="code"
                                value={formik.values.code}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                                isInvalid={isInvalid("code")}
                                errorMessage={getErrorMessage("code")}
                                autoFocus
                            />
                            <InputStyled
                                label="Tên hiển thị"
                                placeholder="VD: Ca Sáng"
                                variant="bordered"
                                name="name"
                                value={formik.values.name}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                                isInvalid={isInvalid("name")}
                                errorMessage={getErrorMessage("name")}
                            />
                        </div>

                        <div className="grid grid-cols-3 gap-4">
                            <InputStyled
                                type="time"
                                label="Bắt đầu"
                                variant="bordered"
                                name="startTime"
                                value={formik.values.startTime}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                                isInvalid={isInvalid("startTime")}
                                errorMessage={getErrorMessage("startTime")}
                            />
                            <InputStyled
                                type="time"
                                label="Kết thúc"
                                variant="bordered"
                                name="endTime"
                                value={formik.values.endTime}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                                isInvalid={isInvalid("endTime")}
                                errorMessage={getErrorMessage("endTime")}
                            />
                            <InputStyled
                                type="number"
                                label="Nghỉ (phút)"
                                variant="bordered"
                                name="breakDurationMinutes"
                                value={String(formik.values.breakDurationMinutes)}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                                isInvalid={isInvalid("breakDurationMinutes")}
                                errorMessage={getErrorMessage("breakDurationMinutes")}
                            />
                        </div>

                        <p className="text-tiny text-gray-400 italic">
                            *Nếu giờ kết thúc nhỏ hơn giờ bắt đầu, hệ thống sẽ tự hiểu là ca qua
                            đêm.
                        </p>
                    </ModalBody>

                    <ModalFooter>
                        <ButtonStyled variant="light" onPress={handleClose} isDisabled={isLoading}>
                            Hủy bỏ
                        </ButtonStyled>
                        <ButtonStyled
                            color="primary"
                            type="submit"
                            isLoading={isLoading}
                            className="font-semibold shadow-lg"
                        >
                            {isEdit ? "Lưu thay đổi" : "Tạo mới"}
                        </ButtonStyled>
                    </ModalFooter>
                </form>
            </ModalContent>
        </ModalStyled>
    )
}
