"use client"

import React, { useEffect, useState } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Tabs, Tab } from "@heroui/react"
import { useFormik } from "formik"
import { shiftSchema, ShiftFormValues } from "./ShiftForm/schema"
import { useTokenStore } from "@/hooks/singleton/store"
import { ButtonStyled, ModalStyled } from "@/components"
import { ShiftAssignmentTab } from "../ShiftAssignmentTab"
import { ShiftStatus } from "../../constants"
import { ShiftForm } from "./ShiftForm"

interface ShiftModalProps {
    isOpen: boolean
    onClose: () => void
    onSubmit: (data: ShiftFormValues) => void
    initialData?: Partial<ShiftFormValues> | null
    isSubmitting?: boolean
    onDelete?: () => void
}

export const ShiftModal = ({
    isOpen,
    onClose,
    onSubmit,
    initialData,
    isSubmitting,
    onDelete
}: ShiftModalProps) => {
    const storeId = useTokenStore((s) => s.storeId)

    const [activeTab, setActiveTab] = useState<string>("info")

    // 2. Setup Formik
    const formik = useFormik<ShiftFormValues>({
        initialValues: {
            storeId: storeId || "",
            shiftDate: new Date(),
            startTime: "08:00",
            endTime: "12:00",
            requiredStaffCount: 1,
            breakDurationMinutes: 0,
            note: "",
            status: ShiftStatus.Draft,
            shiftTemplateId: null
        },
        validationSchema: shiftSchema,
        onSubmit: (values) => {
            onSubmit(values)
            onClose()
        }
    })

    // 3. Reset form khi mở Modal hoặc đổi Data (Edit Mode)
    useEffect(() => {
        if (isOpen) {
            if (initialData) {
                // Edit Mode
                formik.setValues({
                    ...formik.initialValues, // Giữ các field mặc định nếu thiếu
                    ...initialData,
                    shiftDate: initialData.shiftDate ? new Date(initialData.shiftDate) : new Date(),
                    storeId: storeId || ""
                })
            } else {
                // Create Mode -> Reset về default
                formik.resetForm()
                formik.setFieldValue("storeId", storeId || "")
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, initialData, storeId])

    // 4. Logic: Auto-fill giờ khi chọn Template
    // Lắng nghe templateId thay đổi
    // useEffect(() => {
    //     const templateId = formik.values.shiftTemplateId
    //     if (templateId && templates) {
    //         const template = templates.find((t) => t.id === templateId)
    //         if (template) {
    //             // Cắt chuỗi "08:00:00" -> "08:00" cho Input Time
    //             formik.setFieldValue("startTime", template.startTime.slice(0, 5))
    //             formik.setFieldValue("endTime", template.endTime.slice(0, 5))
    //             formik.setFieldValue("breakDurationMinutes", template.breakDurationMinutes)
    //             // Nếu muốn auto-fill cả số lượng nhân viên thì thêm ở đây
    //         }
    //     }
    //     // eslint-disable-next-line react-hooks/exhaustive-deps
    // }, [formik.values.shiftTemplateId, templates])

    const handleClose = () => {
        setActiveTab("info")
        onClose()
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={handleClose}>
            <form onSubmit={formik.handleSubmit}>
                <ModalContent>
                    <ModalHeader className="flex flex-col gap-1">
                        {initialData?.id ? "Quản lý Ca làm việc" : "Tạo Ca Mới"}
                    </ModalHeader>
                    <ModalBody>
                        <Tabs
                            aria-label="Shift Options"
                            selectedKey={activeTab}
                            onSelectionChange={(key) => setActiveTab(key as string)}
                            fullWidth
                            color="primary"
                            variant="underlined"
                        >
                            <Tab key="info" title="Thông tin cơ bản">
                                <ShiftForm formik={formik} />
                            </Tab>
                            <Tab
                                key="staff"
                                title="Phân công nhân sự"
                                isDisabled={!initialData?.id} // Chưa có ca thì không cho gán
                            >
                                <div className="mt-4">
                                    {initialData?.id ? (
                                        <ShiftAssignmentTab
                                            storeId={storeId || ""}
                                            shiftId={initialData.id}
                                            requiredStaffCount={formik.values.requiredStaffCount}
                                            onClose={onClose}
                                        />
                                    ) : (
                                        <p className="text-center text-default-400">
                                            Vui lòng lưu ca làm việc trước khi gán nhân sự.
                                        </p>
                                    )}
                                </div>
                            </Tab>
                        </Tabs>
                    </ModalBody>
                    {activeTab === "info" && (
                        <ModalFooter>
                            {/* Nút Xóa (Chỉ hiện khi Edit) */}
                            {onDelete && (
                                <ButtonStyled color="danger" variant="flat" onPress={onDelete}>
                                    Xóa ca
                                </ButtonStyled>
                            )}

                            <ButtonStyled
                                variant="light"
                                onPress={handleClose}
                                isDisabled={isSubmitting}
                            >
                                Hủy
                            </ButtonStyled>
                            <ButtonStyled color="primary" type="submit" isLoading={isSubmitting}>
                                Lưu thay đổi
                            </ButtonStyled>
                        </ModalFooter>
                    )}
                </ModalContent>
            </form>
        </ModalStyled>
    )
}
