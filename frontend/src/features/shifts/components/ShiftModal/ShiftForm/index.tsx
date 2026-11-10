"use client"

import React, { useCallback } from "react"
import { Select, SelectItem } from "@heroui/react"
import { useFormik } from "formik"
import { ShiftFormValues } from "./schema"
import { getLocalTimeZone } from "@internationalized/date"
import { useGetShiftTemplates } from "@/features/shifts/hooks"
import { useTokenStore } from "@/hooks/singleton/store"
import {
    DatePickerStyled,
    EnumPicker,
    NumberInputStyled,
    TextareaStyled,
    TimeInputStyled
} from "@/components"
import { useDay, useFormikHelpers } from "@/hooks"
import { ShiftStatusLabels } from "@/features/shifts/constants/labels"
import dayjs from "dayjs"

interface ShiftFormProps {
    formik: ReturnType<typeof useFormik<ShiftFormValues>>
    isSubmitable: boolean
}

export const ShiftForm = ({ formik, isSubmitable }: ShiftFormProps) => {
    const storeId = useTokenStore((s) => s.storeId)
    const { toDate, formatTime, toTime } = useDay({ defaultFormat: "HH:mm" })

    // 1. API lấy Template
    const { data: templates } = useGetShiftTemplates({
        params: {
            storeId: storeId || ""
        }
    })

    const minDateStr = dayjs().format("YYYY-MM-DD")
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    const handleTemplateChange = useCallback(
        (id: string | null) => {
            // 1. Cập nhật ID vào Formik trước
            formik.setFieldValue("shiftTemplateId", id)

            // 2. Nếu không có ID (user clear chọn), có thể reset field hoặc dừng lại
            if (!id || !templates) return

            // 3. Tìm template và set data vào các field liên quan
            const template = templates.find((t) => t.id === id)
            if (template) {
                formik.setFieldValue("startTime", template.startTime.slice(0, 5))
                formik.setFieldValue("endTime", template.endTime.slice(0, 5))
                formik.setFieldValue("breakDurationMinutes", template.breakDurationMinutes)
            }
        },
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [templates, formik.setFieldValue]
    )

    const handleFieldChange = (field: string, value: any) => {
        formik.setFieldValue(field, value)

        // Nếu đang có link với Template thì xoá ID đi vì dữ liệu đã khác mẫu
        if (formik.values.shiftTemplateId) {
            formik.setFieldValue("shiftTemplateId", undefined)
        }
    }

    return (
        <form onSubmit={formik.handleSubmit} className="space-y-2">
            {/* 1. Chọn Mẫu */}
            <Select
                label="Chọn mẫu (Tùy chọn)"
                placeholder="Chọn mẫu để điền nhanh"
                variant="bordered"
                items={templates || []}
                selectedKeys={formik.values.shiftTemplateId ? [formik.values.shiftTemplateId] : []}
                onChange={(e) => {
                    // e.target.value có thể rỗng nếu user bỏ chọn, cần handle
                    handleTemplateChange(e.target.value || null)
                }}
                isDisabled={!isSubmitable}
                isClearable
            >
                {(item) => (
                    <SelectItem key={item.id} textValue={item.name}>
                        {item.name} ({item.startTime.slice(0, 5)} - {item.endTime.slice(0, 5)})
                    </SelectItem>
                )}
            </Select>

            {/* 2. Ngày làm việc */}
            <DatePickerStyled
                label="Ngày làm việc"
                value={toDate(formik.values.shiftDate)}
                onChange={(date) => {
                    // Convert ngược về JS Date để lưu vào Formik (dùng timezone local của user)
                    formik.setFieldValue(
                        "shiftDate",
                        date ? date.toDate(getLocalTimeZone()) : undefined
                    )
                }}
                minValue={toDate(minDateStr)}
                isInvalid={isInvalid("shiftDate")}
                errorMessage={getErrorMessage("shiftDate")}
                isReadOnly={!isSubmitable}
            />

            {/* 3. Giờ Bắt đầu - Kết thúc */}
            <div className="flex gap-2">
                <TimeInputStyled
                    label="Bắt đầu"
                    name="startTime"
                    value={toTime(formik.values.startTime)}
                    onChange={(time) => {
                        if (!time) return
                        handleFieldChange("startTime", formatTime(time, false))
                    }}
                    isInvalid={isInvalid("startTime")}
                    errorMessage={getErrorMessage("startTime")}
                    isReadOnly={!isSubmitable}
                />
                <TimeInputStyled
                    label="Kết thúc"
                    name="endTime"
                    value={toTime(formik.values.endTime)}
                    onChange={(time) => {
                        if (!time) return
                        handleFieldChange("endTime", formatTime(time, false))
                    }}
                    isInvalid={isInvalid("endTime")}
                    errorMessage={getErrorMessage("endTime")}
                    isReadOnly={!isSubmitable}
                />
            </div>

            {/* 4. Số lượng nhân viên và số phút nghỉ */}
            <div className="flex flex-col md:flex-row gap-2">
                <EnumPicker
                    label="Trạng thái"
                    labels={ShiftStatusLabels}
                    value={formik.values.status}
                    onChange={(val) => formik.setFieldValue("status", val !== null ? val : 0)}
                    className="w-xl"
                    isClearable={false}
                    isReadOnly={!isSubmitable}
                    isDisabled={formik.values.id === undefined}
                />
                <NumberInputStyled
                    label="Số nhân viên cần"
                    name="requiredStaffCount"
                    value={formik.values.requiredStaffCount}
                    onValueChange={(n) => formik.setFieldValue("requiredStaffCount", n)}
                    isInvalid={isInvalid("requiredStaffCount")}
                    errorMessage={getErrorMessage("requiredStaffCount")}
                    isReadOnly={!isSubmitable}
                />
                <NumberInputStyled
                    label="Nghỉ (phút)"
                    name="breakDurationMinutes"
                    value={formik.values.breakDurationMinutes}
                    onValueChange={(n) => handleFieldChange("breakDurationMinutes", n)}
                    isInvalid={isInvalid("breakDurationMinutes")}
                    errorMessage={getErrorMessage("breakDurationMinutes")}
                    isReadOnly={!isSubmitable}
                />
            </div>

            {/* 5. Ghi chú */}
            <TextareaStyled
                label="Ghi chú"
                placeholder="VD: Ca này cần kiểm kê kho"
                name="note"
                value={formik.values.note || ""}
                onChange={formik.handleChange}
                isReadOnly={!isSubmitable}
            />
        </form>
    )
}
