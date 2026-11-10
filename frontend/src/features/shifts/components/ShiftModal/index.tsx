"use client"

import React, { useEffect, useState } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Tabs, Tab } from "@heroui/react"
import { useFormik } from "formik"
import { shiftSchema, ShiftFormValues } from "./ShiftForm/schema"
import { useTokenStore } from "@/hooks/singleton/store"
import { ButtonStyled, ModalStyled } from "@/components"
import { ShiftAssignmentTab } from "./ShiftAssignmentTab"
import { ShiftStatus } from "@/features/shifts/constants"
import { ShiftForm } from "./ShiftForm"
import { ShiftApplicationTab } from "./ShiftApplicationTab"

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

    const isSubmitable =
        !initialData?.id ||
        initialData?.status === ShiftStatus.Draft ||
        initialData?.status === ShiftStatus.Published ||
        initialData?.status === ShiftStatus.Open

    // 2. Setup Formik
    const formik = useFormik<ShiftFormValues>({
        initialValues: {
            id: initialData?.id || undefined,
            storeId: storeId || "",
            shiftDate: new Date(),
            startTime: "08:00",
            endTime: "12:00",
            requiredStaffCount: 1,
            breakDurationMinutes: 0,
            note: "",
            status: ShiftStatus.Draft,
            shiftTemplateId: undefined
        },
        validationSchema: shiftSchema,
        onSubmit: (values) => {
            if (!isSubmitable) return
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
                // Create Mode
                formik.resetForm()
                formik.setFieldValue("storeId", storeId || "")
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, initialData?.id, storeId])

    const handleClose = () => {
        setActiveTab("info")
        formik.resetForm()
        onClose()
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={handleClose} isKeyboardDismissDisabled={false}>
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
                            <ShiftForm formik={formik} isSubmitable={isSubmitable} />
                        </Tab>
                        <Tab
                            key="staff"
                            title="Phân công nhân sự"
                            isDisabled={!initialData?.id} // Chưa có ca thì không cho gán
                            className="py-0"
                        >
                            {initialData?.id ? (
                                <ShiftAssignmentTab
                                    storeId={storeId || ""}
                                    shiftId={initialData.id}
                                    requiredStaffCount={formik.values.requiredStaffCount}
                                    isSubmitable={isSubmitable}
                                    onClose={onClose}
                                />
                            ) : (
                                <p className="text-center text-default-400 mt-3">
                                    Vui lòng lưu ca làm việc trước khi gán nhân sự.
                                </p>
                            )}
                        </Tab>
                        <Tab
                            key="applications"
                            title="Đơn ứng tuyển"
                            isDisabled={!initialData?.id}
                            className="py-0"
                        >
                            {initialData?.id ? (
                                <ShiftApplicationTab shiftId={initialData.id} />
                            ) : (
                                <p className="text-center text-default-400 mt-3">
                                    Vui lòng lưu ca làm việc trước khi xem đơn ứng tuyển.
                                </p>
                            )}
                        </Tab>
                    </Tabs>
                </ModalBody>
                {activeTab === "info" && (
                    <ModalFooter>
                        {/* Nút Xóa (Chỉ hiện khi Edit) */}
                        {onDelete && isSubmitable && (
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
                        {isSubmitable && (
                            <ButtonStyled
                                color="primary"
                                isLoading={isSubmitting}
                                onPress={() => formik.handleSubmit()}
                            >
                                Lưu thay đổi
                            </ButtonStyled>
                        )}
                    </ModalFooter>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
