"use client"

import React, { useEffect } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter } from "@heroui/react"
import { useFormik } from "formik"
import { CreateShiftTemplateRequest } from "@/features/shifts/models/request"
import { ShiftTemplateResponse } from "@/features/shifts/models/response"
import { useCreateShiftTemplate, useUpdateShiftTemplate } from "@/features/shifts/hooks"
import {
    ButtonStyled,
    InputStyled,
    ModalStyled,
    NumberInputStyled,
    TimeInputStyled
} from "@/components"
import { validationSchema } from "./schema"
import { useDay, useFormikHelpers } from "@/hooks"

interface ShiftTemplateModalProps {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    selectedTemplate?: ShiftTemplateResponse
    templateStoreName?: string
    storeId?: string
}

// Giá trị mặc định ban đầu
const INITIAL_VALUES: CreateShiftTemplateRequest = {
    storeId: undefined,
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
    templateStoreName = "",
    storeId
}: ShiftTemplateModalProps) => {
    const { formatTime, toTime } = useDay()

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
            // const formatTimeStr = (t: string) => (t.length === 5 ? `${t}:00` : t)

            const submitData = {
                ...values,
                storeId: storeId,
                startTime: values.startTime,
                endTime: values.endTime
                // startTime: formatTimeStr(values.startTime),
                // endTime: formatTimeStr(values.endTime)
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
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

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

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
            <ModalContent>
                <form onSubmit={formik.handleSubmit}>
                    <ModalHeader className="flex flex-col gap-1 text-xl font-bold">
                        {isEdit ? "Cập nhật Mẫu Ca" : "Tạo Mẫu Ca Mới"}
                        {isEdit && templateStoreName && (
                            <span className="text-tiny text-default-400">
                                Cửa hàng: {templateStoreName}
                            </span>
                        )}
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
                            <TimeInputStyled
                                label="Bắt đầu"
                                name="startTime"
                                value={toTime(formik.values.startTime)}
                                onChange={(time) => {
                                    if (!time) return
                                    formik.setFieldValue("startTime", formatTime(time, false))
                                }}
                                isInvalid={isInvalid("startTime")}
                                errorMessage={getErrorMessage("startTime")}
                            />
                            <TimeInputStyled
                                label="Kết thúc"
                                name="endTime"
                                value={toTime(formik.values.endTime)}
                                onChange={(time) => {
                                    if (!time) return
                                    formik.setFieldValue("endTime", formatTime(time, false))
                                }}
                                isInvalid={isInvalid("endTime")}
                                errorMessage={getErrorMessage("endTime")}
                            />
                            <NumberInputStyled
                                label="Nghỉ (phút)"
                                name="breakDurationMinutes"
                                value={formik.values.breakDurationMinutes}
                                onValueChange={(n) =>
                                    formik.setFieldValue("breakDurationMinutes", n)
                                }
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
