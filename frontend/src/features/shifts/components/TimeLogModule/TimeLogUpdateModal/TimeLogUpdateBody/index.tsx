// features/shifts/components/TimeLogUpdateBody.tsx
"use client"

import { Divider } from "@heroui/react"
import { useFormikContext } from "formik"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatus } from "@/features/shifts/constants"
import { TimeLogStatusLabels } from "@/features/shifts/constants/labels"
import { EnumPicker, TextareaStyled } from "@/components"
import { UpdateTimeLogFormValues } from "../schema"
import { AlertTriangle } from "lucide-react"
import { TimeLogSharedInfo } from "../TimeLogSharedInfo"

interface TimeLogUpdateBodyProps {
    data: TimeLogResponse
}

export const TimeLogUpdateBody = ({ data }: TimeLogUpdateBodyProps) => {
    const { values, errors, handleChange, setFieldValue } =
        useFormikContext<UpdateTimeLogFormValues>()

    return (
        <div className="flex flex-col gap-6">
            <TimeLogSharedInfo data={data} />

            <Divider className="my-2" />

            <div className="flex flex-col gap-5">
                <EnumPicker
                    label="Trạng thái phê duyệt"
                    placeholder="Chọn trạng thái công"
                    labels={TimeLogStatusLabels}
                    value={values.status}
                    onChange={(val) => setFieldValue("status", val)}
                    isClearable={false}
                    isInvalid={!!errors.status}
                    disabledKeys={[TimeLogStatus.Processing]}
                />

                <div className="relative">
                    <TextareaStyled
                        label="Ghi chú của quản lý"
                        placeholder={
                            values.status === TimeLogStatus.Invalid
                                ? "Bắt buộc: Tại sao ca này không hợp lệ?"
                                : "Nhập ghi chú (không bắt buộc)..."
                        }
                        name="managerNote"
                        value={values.managerNote || ""}
                        onChange={handleChange}
                        isInvalid={!!errors.managerNote}
                        errorMessage={errors.managerNote}
                        variant="bordered"
                    />
                    {values.status === TimeLogStatus.Invalid && !values.managerNote && (
                        <div className="absolute top-2 right-2 flex items-center gap-1 text-danger animate-pulse">
                            <AlertTriangle size={14} />
                            <span className="text-[10px] font-bold">BẮT BUỘC</span>
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}
