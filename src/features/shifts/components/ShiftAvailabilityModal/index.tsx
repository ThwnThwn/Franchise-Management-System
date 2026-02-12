"use client"

import React, { useEffect, useMemo } from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Switch } from "@heroui/react"
import { useFormik } from "formik"
import { CreateShiftAvailabilityRequest } from "@/features/shifts/models/request"
import { ShiftAvailabilityResponse } from "@/features/shifts/models/response"
import { useCreateAvailability, useUpdateAvailability } from "@/features/shifts/hooks"
import {
    ButtonStyled,
    InputStyled,
    ModalStyled,
    TimeInputStyled,
    EnumPicker,
    DatePickerStyled
} from "@/components"
import { useDay, useFormikHelpers } from "@/hooks"
import { CalendarDays, Repeat } from "lucide-react"
import { DayOfWeek } from "@/constants/enums"
import { DayOfWeekLabel } from "@/constants/labels"
import { shiftAvailabilitySchema } from "./schema"

interface ShiftAvailabilityModalProps {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    selectedItem?: ShiftAvailabilityResponse
}

const INITIAL_VALUES: CreateShiftAvailabilityRequest = {
    isAvailable: false,
    isRecurring: false,
    startTime: "08:00",
    endTime: "12:00",
    note: ""
}

export const ShiftAvailabilityModal = ({
    isOpen,
    onOpenChange,
    onClose,
    selectedItem
}: ShiftAvailabilityModalProps) => {
    const { dateNow, formatTime, toTime, formatDate, toDate } = useDay()
    const createMutation = useCreateAvailability({ onSuccess: () => handleClose() })
    const updateMutation = useUpdateAvailability({ onSuccess: () => handleClose() })

    const isEdit = !!selectedItem
    const isLoading = createMutation.isPending || updateMutation.isPending

    const minDateStr = dateNow(2)
    const formik = useFormik<CreateShiftAvailabilityRequest>({
        initialValues: INITIAL_VALUES,
        validationSchema: shiftAvailabilitySchema,
        onSubmit: (values) => {
            if (isEdit && selectedItem) {
                updateMutation.mutate({ id: selectedItem.id, req: values })
            } else {
                createMutation.mutate(values)
            }
        }
    })
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    const isOvernight = useMemo(() => {
        if (!formik.values.startTime || !formik.values.endTime) return false
        return formik.values.startTime > formik.values.endTime
    }, [formik.values.startTime, formik.values.endTime])

    useEffect(() => {
        if (isOpen) {
            if (selectedItem) {
                formik.setValues({
                    isAvailable: selectedItem.isAvailable,
                    isRecurring: selectedItem.isRecurring,
                    specificDate: selectedItem.specificDate || undefined,
                    dayOfWeek: selectedItem.dayOfWeek ?? undefined,
                    startTime: selectedItem.startTime.slice(0, 5),
                    endTime: selectedItem.endTime.slice(0, 5),
                    note: selectedItem.note
                })
            } else {
                formik.resetForm()
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, selectedItem])

    const handleClose = () => {
        onClose()
        formik.resetForm()
    }

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
            <ModalContent>
                <form onSubmit={formik.handleSubmit}>
                    <ModalHeader className="flex flex-col gap-1 text-xl font-bold">
                        {isEdit ? "Cập nhật đơn" : "Đăng ký lịch Rảnh/Bận"}
                    </ModalHeader>

                    <ModalBody className="gap-6">
                        {/* Toggle Rảnh/Bận - Nhìn phát biết ngay */}
                        <div
                            className={`flex items-center justify-between p-4 rounded-xl border-2 border-dashed ${formik.values.isAvailable ? "border-success-200 bg-success-50" : "border-danger-200 bg-danger-50"}`}
                        >
                            <div className="flex flex-col">
                                <span className="font-bold text-default-700">
                                    Trạng thái đăng ký
                                </span>
                                <span className="text-tiny text-default-500">
                                    Bạn đang báo {formik.values.isAvailable ? "RẢNH" : "BẬN"} vào
                                    giờ này
                                </span>
                            </div>
                            <Switch
                                color={formik.values.isAvailable ? "success" : "danger"}
                                isSelected={formik.values.isAvailable}
                                onValueChange={(v) => formik.setFieldValue("isAvailable", v)}
                            />
                        </div>

                        {/* Hybrid Logic Switch */}
                        <div className="flex items-center gap-4 px-2">
                            <Switch
                                size="sm"
                                isSelected={formik.values.isRecurring}
                                onValueChange={(v) => {
                                    formik.setFieldValue("isRecurring", v)
                                    formik.setFieldValue("specificDate", undefined)
                                    formik.setFieldValue("dayOfWeek", undefined)
                                }}
                            />
                            <span className="text-small font-semibold flex items-center gap-2">
                                {formik.values.isRecurring ? (
                                    <>
                                        <Repeat size={16} className="text-primary" /> Lặp lại hàng
                                        tuần
                                    </>
                                ) : (
                                    <>
                                        <CalendarDays size={16} className="text-primary" /> Chỉ một
                                        ngày cụ thể
                                    </>
                                )}
                            </span>
                        </div>

                        {/* Field hiển thị theo Logic Hybrid */}
                        {formik.values.isRecurring ? (
                            <EnumPicker<DayOfWeek>
                                label="Chọn Thứ trong tuần"
                                placeholder="Gõ để tìm nhanh (VD: Thứ 2)..."
                                labels={DayOfWeekLabel}
                                value={formik.values.dayOfWeek}
                                onChange={(v) => formik.setFieldValue("dayOfWeek", v ?? undefined)}
                                isInvalid={isInvalid("dayOfWeek")}
                                errorMessage={getErrorMessage("dayOfWeek")}
                            />
                        ) : (
                            <DatePickerStyled
                                label="Chọn ngày chính xác"
                                name="specificDate"
                                value={toDate(formik.values.specificDate)}
                                onChange={(val) =>
                                    formik.setFieldValue("specificDate", formatDate(val))
                                }
                                minValue={toDate(minDateStr)}
                                isInvalid={isInvalid("specificDate")}
                                errorMessage={getErrorMessage("specificDate")}
                            />
                        )}

                        <div className="grid grid-cols-2 gap-4">
                            <TimeInputStyled
                                label="Bắt đầu từ"
                                value={toTime(formik.values.startTime)}
                                onChange={(t) =>
                                    formik.setFieldValue("startTime", formatTime(t, false))
                                }
                            />
                            <TimeInputStyled
                                label="Kết thúc lúc"
                                value={toTime(formik.values.endTime)}
                                onChange={(t) =>
                                    formik.setFieldValue("endTime", formatTime(t, false))
                                }
                            />
                        </div>
                        {isOvernight && (
                            <p className="text-tiny text-warning-500 italic -mt-3 px-2">
                                * Hệ thống ghi nhận đây là khung giờ làm việc xuyên đêm.
                            </p>
                        )}

                        <InputStyled
                            label="Ghi chú thêm"
                            placeholder="Lý do bận, hoặc lưu ý rảnh..."
                            variant="flat"
                            name="note"
                            value={formik.values.note}
                            onChange={formik.handleChange}
                        />
                    </ModalBody>

                    <ModalFooter className="bg-default-50 rounded-b-xl border-t">
                        <ButtonStyled variant="light" onPress={handleClose}>
                            Hủy bỏ
                        </ButtonStyled>
                        <ButtonStyled
                            color="primary"
                            type="submit"
                            isLoading={isLoading}
                            className="font-bold px-8 shadow-md"
                        >
                            {isEdit ? "Cập nhật đơn" : "Gửi yêu cầu"}
                        </ButtonStyled>
                    </ModalFooter>
                </form>
            </ModalContent>
        </ModalStyled>
    )
}
