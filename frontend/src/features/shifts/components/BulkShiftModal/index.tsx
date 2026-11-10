"use client"

import React, { useCallback, useEffect, useMemo } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Select,
    SelectItem
} from "@heroui/react"
import { useFormik } from "formik"
import { CalendarRange, Users } from "lucide-react"

import {
    ModalStyled,
    ButtonStyled,
    EnumPicker,
    DatePickerStyled,
    TimeInputStyled,
    NumberInputStyled,
    TextareaStyled
} from "@/components"

import { BulkShiftTypeLabels } from "@/features/shifts/constants/labels"
import { bulkShiftSchema, BulkShiftFormValues } from "./schema"

import {
    useGetShiftTemplates,
    useBulkCreateManualShift,
    useBulkCreateFromTemplateShift
} from "@/features/shifts/hooks"
import { DayOfWeekLabel } from "@/constants/labels"
import { DayOfWeek } from "@/constants/enums"
import { useDay } from "@/hooks"
import { enumToValues } from "@/utils/helpers/arrayHelper"
import dayjs from "dayjs"

interface BulkShiftModalProps {
    isOpen: boolean
    onClose: () => void
    storeId: string

    currentDate: dayjs.Dayjs
    dateRange: {
        fromDate: string
        toDate: string
    }
}

export const BulkShiftModal = ({
    isOpen,
    onClose,
    storeId,
    currentDate,
    dateRange
}: BulkShiftModalProps) => {
    const { dateNow, formatDate, toDate, formatTime, toTime } = useDay({ defaultFormat: "HH:mm" })

    const minDateStr = dateNow()
    // 1. API Hooks
    const { data: templates } = useGetShiftTemplates({ params: { storeId } })
    const createManual = useBulkCreateManualShift({ onSuccess: onClose })
    const createTemplate = useBulkCreateFromTemplateShift({ onSuccess: onClose })

    // 2. Formik Setup
    const formik = useFormik<BulkShiftFormValues>({
        initialValues: {
            type: "manual",
            startDate: "",
            endDate: "",
            startTime: "08:00",
            endTime: "16:00",
            daysOfWeek: enumToValues(DayOfWeek),
            templateId: "",
            requiredStaffCount: 1,
            breakDurationMinutes: 0,
            note: ""
        },
        validationSchema: bulkShiftSchema,
        onSubmit: (values) => {
            if (values.type === "manual") {
                createManual.mutate({
                    storeId,

                    startTime: values.startTime!,
                    endTime: values.endTime!,
                    daysOfWeek: values.daysOfWeek as number[],
                    startDate: values.startDate,
                    endDate: values.endDate,

                    requiredStaffCount: values.requiredStaffCount,
                    breakDurationMinutes: values.breakDurationMinutes,
                    note: values.note || undefined
                })
            } else {
                createTemplate.mutate({
                    storeId,
                    templateId: values.templateId!,
                    startDate: values.startDate,
                    endDate: values.endDate,
                    requiredStaffCount: values.requiredStaffCount
                })
            }
        }
    })

    useEffect(() => {
        if (isOpen) {
            const forOpenStartDate = currentDate.isAfter(dayjs(dateRange.fromDate))
                ? currentDate.format("YYYY-MM-DD")
                : dateRange.fromDate
            formik.setFieldValue("startDate", forOpenStartDate)
            formik.setFieldValue("endDate", dateRange.toDate)
            formik.validateForm()
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen])

    // 3. Chuyển đổi list Template thành Labels cho EnumPicker
    const templateLabels = useMemo(() => {
        const obj: Record<string, string> = {}
        templates?.forEach((t) => {
            obj[t.id] = `${t.name} (${t.startTime.substring(0, 5)} - ${t.endTime.substring(0, 5)})`
        })
        return obj
    }, [templates])

    const handleTemplateChange = useCallback(
        (id: string | undefined) => {
            // 1. Cập nhật ID vào Formik trước
            formik.setFieldValue("templateId", id)

            // 2. Nếu không có ID (user clear chọn), có thể reset field hoặc dừng lại
            if (!id || !templates) return

            // 3. Tìm template và set data vào các field liên quan
            const template = templates.find((t) => t.id === id)
            if (template) {
                // formik.setFieldValue("startTime", template.startTime.slice(0, 5))
                // formik.setFieldValue("endTime", template.endTime.slice(0, 5))
                formik.setFieldValue("breakDurationMinutes", template.breakDurationMinutes)
            }
        },
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [templates, formik.setFieldValue]
    )

    const isSubmitting = createManual.isPending || createTemplate.isPending

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="2xl">
            <ModalContent>
                <ModalHeader className="flex gap-2 items-center border-b pb-4">
                    <CalendarRange className="text-primary" size={24} />
                    <div className="flex flex-col">
                        <span className="text-lg">Xếp Lịch Hàng Loạt</span>
                        <span className="text-tiny font-normal text-default-400">
                            Tự động tạo ca làm việc theo chu kỳ
                        </span>
                    </div>
                </ModalHeader>

                <ModalBody className="py-6">
                    <div className="flex flex-col gap-6">
                        {/* 1. Chọn phương thức tạo */}
                        <EnumPicker
                            label="Phương thức tạo ca"
                            placeholder="Chọn cách bro muốn tạo"
                            labels={BulkShiftTypeLabels}
                            value={formik.values.type}
                            onChange={(val) => formik.setFieldValue("type", val)}
                            isClearable={false}
                        />

                        {/* 2. Khoảng ngày áp dụng (Luôn hiện) */}
                        <div className="grid grid-cols-2 gap-4 p-4 bg-default-50 rounded-xl border border-dashed border-default-300">
                            <DatePickerStyled
                                label="Từ ngày"
                                value={toDate(formik.values.startDate)}
                                onChange={(date) => {
                                    formik.setFieldValue("startDate", formatDate(date))
                                }}
                                isInvalid={!!formik.errors.startDate}
                                errorMessage={formik.errors.startDate}
                                minValue={toDate(minDateStr)}
                            />
                            <DatePickerStyled
                                label="Đến ngày"
                                value={toDate(formik.values.endDate)}
                                onChange={(date) => {
                                    formik.setFieldValue("endDate", formatDate(date))
                                }}
                                isInvalid={!!formik.errors.endDate}
                                errorMessage={formik.errors.endDate}
                                minValue={toDate(minDateStr)}
                            />
                        </div>

                        {/* 3. Logic hiển thị theo Type */}
                        {formik.values.type === "manual" ? (
                            <div className="flex flex-col gap-5 animate-in fade-in slide-in-from-top-2">
                                <Select
                                    label="Chọn các ngày trong tuần"
                                    selectionMode="multiple"
                                    placeholder="Ví dụ: Thứ 2, Thứ 4..."
                                    variant="bordered"
                                    selectedKeys={
                                        new Set(formik.values.daysOfWeek?.map(String).sort())
                                    }
                                    onSelectionChange={(keys) => {
                                        const arr = Array.from(keys).map(Number)
                                        formik.setFieldValue("daysOfWeek", arr)
                                    }}
                                >
                                    {Object.entries(DayOfWeekLabel).map(([key, label]) => (
                                        <SelectItem key={key} textValue={label}>
                                            {label}
                                        </SelectItem>
                                    ))}
                                </Select>

                                <div className="grid grid-cols-2 gap-4">
                                    <TimeInputStyled
                                        label="Giờ bắt đầu"
                                        value={toTime(formik.values.startTime)}
                                        onChange={(time) => {
                                            if (!time) return
                                            formik.setFieldValue(
                                                "startTime",
                                                formatTime(time, false)
                                            )
                                        }}
                                        isInvalid={!!formik.errors.startTime}
                                        errorMessage={formik.errors.startTime}
                                    />
                                    <TimeInputStyled
                                        label="Giờ kết thúc"
                                        value={toTime(formik.values.endTime)}
                                        onChange={(time) => {
                                            if (!time) return
                                            formik.setFieldValue("endTime", formatTime(time, false))
                                        }}
                                        isInvalid={!!formik.errors.endTime}
                                        errorMessage={formik.errors.endTime}
                                    />
                                </div>
                            </div>
                        ) : (
                            <div className="flex flex-col gap-5 animate-in fade-in slide-in-from-top-2">
                                <EnumPicker
                                    label="Chọn mẫu ca làm việc"
                                    placeholder="Tìm mẫu ca đã lưu..."
                                    labels={templateLabels}
                                    value={formik.values.templateId}
                                    onChange={handleTemplateChange}
                                    isInvalid={!!formik.errors.templateId}
                                    errorMessage={formik.errors.templateId}
                                />
                            </div>
                        )}

                        {/* 4. Thông số chung */}
                        <div className="grid grid-cols-2 gap-4">
                            <NumberInputStyled
                                label="Nhân viên/ca"
                                variant="bordered"
                                startContent={<Users size={18} className="text-default-400" />}
                                value={formik.values.requiredStaffCount}
                                onValueChange={(n) => formik.setFieldValue("requiredStaffCount", n)}
                                isInvalid={!!formik.errors.requiredStaffCount}
                                errorMessage={formik.errors.requiredStaffCount}
                            />
                            <NumberInputStyled
                                label="Phút nghỉ"
                                variant="bordered"
                                value={formik.values.breakDurationMinutes}
                                onValueChange={(n) =>
                                    formik.setFieldValue("breakDurationMinutes", n)
                                }
                                isInvalid={!!formik.errors.breakDurationMinutes}
                                errorMessage={formik.errors.breakDurationMinutes}
                                isDisabled={formik.values.type === "template"}
                            />
                        </div>

                        <TextareaStyled
                            label="Ghi chú"
                            placeholder="VD: Ca này cần kiểm kê kho"
                            name="note"
                            value={formik.values.note || ""}
                            onChange={formik.handleChange}
                        />
                    </div>
                </ModalBody>

                <ModalFooter className="border-t">
                    <Button variant="light" onPress={onClose} isDisabled={isSubmitting}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="primary"
                        onPress={() => formik.handleSubmit()}
                        isLoading={isSubmitting}
                        className="px-8"
                    >
                        Tạo lịch ngay
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
