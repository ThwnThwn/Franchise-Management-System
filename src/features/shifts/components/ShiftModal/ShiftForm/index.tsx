"use client"

import React, { useEffect } from "react"
import { Select, SelectItem, DatePicker } from "@heroui/react"
import { useFormik } from "formik"
import { ShiftFormValues } from "./schema"
import { getLocalTimeZone } from "@internationalized/date"
import { useGetShiftTemplates } from "@/features/shifts/hooks/useShiftTemplates"
import { useTokenStore } from "@/hooks/singleton/store"
import { NumberInputStyled, TextareaStyled, TimeInputStyled } from "@/components"
import { useDay, useFormikHelpers } from "@/hooks"

interface ShiftFormProps {
    formik: ReturnType<typeof useFormik<ShiftFormValues>>
}

export const ShiftForm = ({ formik }: ShiftFormProps) => {
    const storeId = useTokenStore((s) => s.storeId)
    const { toDate, formatTime, toTime } = useDay({ defaultFormat: "HH:mm" })

    // 1. API lấy Template
    const { data: templates } = useGetShiftTemplates({
        params: {
            storeId: storeId || ""
        }
    })

    // 2. Setup Formik
    // const formik = useFormik<ShiftFormValues>({
    //     initialValues: {
    //         storeId: storeId || "",
    //         shiftDate: new Date(),
    //         startTime: "08:00",
    //         endTime: "12:00",
    //         requiredStaffCount: 1,
    //         breakDurationMinutes: 0,
    //         note: "",
    //         status: ShiftStatus.Draft,
    //         shiftTemplateId: null
    //     },
    //     validationSchema: shiftSchema,
    //     onSubmit: (values) => {
    //         onSubmit(values)
    //         onClose()
    //     }
    // })

    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    // 4. Logic: Auto-fill giờ khi chọn Template
    // Lắng nghe templateId thay đổi
    useEffect(() => {
        const templateId = formik.values.shiftTemplateId
        if (templateId && templates) {
            const template = templates.find((t) => t.id === templateId)
            if (template) {
                // Cắt chuỗi "08:00:00" -> "08:00" cho Input Time
                formik.setFieldValue("startTime", template.startTime.slice(0, 5))
                formik.setFieldValue("endTime", template.endTime.slice(0, 5))
                formik.setFieldValue("breakDurationMinutes", template.breakDurationMinutes)
                // Nếu muốn auto-fill cả số lượng nhân viên thì thêm ở đây
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [formik.values.shiftTemplateId, templates])

    return (
        <form onSubmit={formik.handleSubmit} className="space-y-2">
            {/* 1. Chọn Mẫu */}
            <Select
                label="Chọn mẫu (Tùy chọn)"
                placeholder="Chọn mẫu để điền nhanh"
                items={templates || []}
                selectedKeys={formik.values.shiftTemplateId ? [formik.values.shiftTemplateId] : []}
                onChange={(e) => {
                    // e.target.value có thể rỗng nếu user bỏ chọn, cần handle
                    formik.setFieldValue("shiftTemplateId", e.target.value || null)
                }}
            >
                {/* 4. Dùng render function thay vì map trực tiếp */}
                {(item) => (
                    <SelectItem key={item.id} textValue={item.name}>
                        {item.name} ({item.startTime.slice(0, 5)} - {item.endTime.slice(0, 5)})
                    </SelectItem>
                )}
            </Select>

            {/* 2. Ngày làm việc */}
            <DatePicker
                label="Ngày làm việc"
                value={toDate(formik.values.shiftDate)}
                onChange={(date) => {
                    // Convert ngược về JS Date để lưu vào Formik (dùng timezone local của user)
                    formik.setFieldValue(
                        "shiftDate",
                        date ? date.toDate(getLocalTimeZone()) : undefined
                    )
                }}
                isInvalid={isInvalid("shiftDate")}
                errorMessage={getErrorMessage("shiftDate")}
            />

            {/* 3. Giờ Bắt đầu - Kết thúc */}
            <div className="flex gap-2">
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
            </div>

            {/* 4. Số lượng nhân viên và số phút nghỉ */}
            <div className="flex gap-2">
                <NumberInputStyled
                    label="Số nhân viên cần"
                    name="requiredStaffCount"
                    value={formik.values.requiredStaffCount}
                    onValueChange={(n) => formik.setFieldValue("requiredStaffCount", n)}
                    isInvalid={isInvalid("requiredStaffCount")}
                    errorMessage={getErrorMessage("requiredStaffCount")}
                />
                <NumberInputStyled
                    label="Nghỉ (phút)"
                    name="breakDurationMinutes"
                    value={formik.values.breakDurationMinutes}
                    onValueChange={(n) => formik.setFieldValue("breakDurationMinutes", n)}
                    isInvalid={isInvalid("breakDurationMinutes")}
                    errorMessage={getErrorMessage("breakDurationMinutes")}
                />
            </div>

            {/* 5. Ghi chú */}
            <TextareaStyled
                label="Ghi chú"
                placeholder="VD: Ca này cần kiểm kê kho"
                name="note"
                value={formik.values.note || ""}
                onChange={formik.handleChange}
            />
        </form>
    )
}
