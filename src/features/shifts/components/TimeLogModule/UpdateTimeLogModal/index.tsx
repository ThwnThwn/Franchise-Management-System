"use client"

import React from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Image,
    Divider
} from "@heroui/react"
import { useFormik } from "formik"
import { Pencil, Clock, Info, AlertTriangle } from "lucide-react"

import { ModalStyled, ButtonStyled, EnumPicker, TextareaStyled } from "@/components"

import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatus } from "@/features/shifts/constants"
import { updateTimeLogSchema, UpdateTimeLogFormValues } from "./schema"
import { useUpdateTimeLog } from "@/features/shifts/hooks"
import { StaffInfoCell } from "@/features/shifts/components"
import { useDay } from "@/hooks"
import { TimeLogStatusLabels } from "@/features/shifts/constants/labels"

interface Props {
    isOpen: boolean
    onClose: () => void
    data: TimeLogResponse | null // Record đang được chọn để edit
    isReadOnly: boolean
}

export const UpdateTimeLogModal = ({ isOpen, onClose, data, isReadOnly }: Props) => {
    const { formatDateTime, formatTime } = useDay()
    const updateMutation = useUpdateTimeLog({ onSuccess: onClose })

    // 1. Formik Setup
    const formik = useFormik<UpdateTimeLogFormValues>({
        initialValues: {
            status: data?.status ?? TimeLogStatus.Processing,
            managerNote: data?.managerNote ?? ""
        },
        enableReinitialize: true, // Quan trọng: Để form cập nhật khi data thay đổi
        validationSchema: updateTimeLogSchema,
        onSubmit: (values) => {
            if (!data || isReadOnly) return
            updateMutation.mutate({
                id: data.id,
                req: {
                    status: values.status,
                    managerNote: values.managerNote || undefined
                }
            })
        }
    })

    const isSubmitting = updateMutation.isPending

    if (!data) return null

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="2xl">
            <ModalContent>
                <ModalHeader className="flex gap-2 items-center border-b pb-4">
                    <Pencil className="text-primary" size={24} />
                    <div className="flex flex-col">
                        <span className="text-lg">Cập Nhật Chấm Công</span>
                        <span className="text-tiny font-normal text-default-400">
                            Điều chỉnh trạng thái và ghi chú cho bản ghi #{data.id.substring(0, 8)}
                        </span>
                    </div>
                </ModalHeader>

                <ModalBody className="py-6 overflow-y-auto max-h-[70vh]">
                    <div className="flex flex-col gap-6">
                        {/* 1. Thông tin nhân viên */}
                        <div className="flex justify-between items-start bg-default-50 p-4 rounded-2xl border border-divider">
                            <StaffInfoCell staff={data.staff} />

                            {/* HIỂN THỊ CA LÀM GỐC */}
                            <div className="text-right flex flex-col items-end gap-1">
                                {/* <span className="text-[10px] font-bold text-default-400 uppercase tracking-widest block">
                                    Ca làm việc
                                </span> */}
                                <div className="flex items-center gap-2 bg-primary-50 px-3 py-1 rounded-full border border-primary-100">
                                    <Clock size={14} className="text-primary" />
                                    <span className="text-sm font-black text-primary tabular-nums">
                                        {formatTime(data.shift.startTime, false)} -{" "}
                                        {formatTime(data.shift.endTime, false)}
                                    </span>
                                </div>
                                <span className="px-3 text-[10px] font-medium text-default-500 italic max-w-50 truncate">
                                    {data.shift.note || "Không có ghi chú ca"}
                                </span>
                            </div>
                        </div>

                        {/* 2. Đối chiếu thời gian thực tế */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Check-in thực tế */}
                            <div className="p-4 rounded-2xl bg-success-50 border border-success-100 relative overflow-hidden">
                                <div className="flex items-center justify-between mb-2">
                                    <div className="flex items-center gap-2 text-success-600 font-bold text-xs uppercase">
                                        <Clock size={14} /> Check-in Thực Tế
                                    </div>
                                    <span className="text-[9px] font-bold bg-success-200 text-success-700 px-1.5 py-0.5 rounded uppercase">
                                        Dự kiến: {formatTime(data.shift.startTime, false)}
                                    </span>
                                </div>
                                <p className="text-2xl font-black tabular-nums text-success-700">
                                    {formatDateTime({ date: data.checkInAt, format: "HH:mm" })}
                                </p>
                                <span className="text-tiny text-success-500 font-medium">
                                    {formatDateTime({ date: data.checkInAt, format: "DD/MM/YYYY" })}
                                </span>
                            </div>

                            {/* Check-out thực tế */}
                            <div className="p-4 rounded-2xl bg-secondary-50 border border-secondary-100 relative overflow-hidden">
                                <div className="flex items-center justify-between mb-2">
                                    <div className="flex items-center gap-2 text-secondary-600 font-bold text-xs uppercase">
                                        <Clock size={14} /> Check-out Thực Tế
                                    </div>
                                    <span className="text-[9px] font-bold bg-secondary-200 text-secondary-700 px-1.5 py-0.5 rounded uppercase">
                                        Dự kiến: {formatTime(data.shift.endTime, false)}
                                    </span>
                                </div>
                                <p className="text-2xl font-black tabular-nums text-secondary-700">
                                    {data.checkOutAt
                                        ? formatDateTime({ date: data.checkOutAt, format: "HH:mm" })
                                        : "--:--"}
                                </p>
                                <span className="text-tiny text-secondary-500 font-medium">
                                    {data.checkOutAt
                                        ? formatDateTime({
                                              date: data.checkOutAt,
                                              format: "DD/MM/YYYY"
                                          })
                                        : "Nhân viên chưa bấm Tan ca"}
                                </span>
                            </div>
                        </div>

                        {/* 3. Bằng chứng hình ảnh (Nếu có) */}
                        {(data.checkInImageUrl || data.checkOutImageUrl) && (
                            <div className="space-y-3">
                                <span className="text-xs font-bold text-default-400 uppercase flex items-center gap-1">
                                    <Info size={14} /> Bằng chứng hình ảnh
                                </span>
                                <div className="flex gap-4">
                                    {data.checkInImageUrl && (
                                        <Image
                                            src={data.checkInImageUrl}
                                            alt="Check-in"
                                            className="w-32 h-32 object-cover rounded-xl border-2 border-white shadow-sm"
                                        />
                                    )}
                                    {data.checkOutImageUrl && (
                                        <Image
                                            src={data.checkOutImageUrl}
                                            alt="Check-out"
                                            className="w-32 h-32 object-cover rounded-xl border-2 border-white shadow-sm"
                                        />
                                    )}
                                </div>
                            </div>
                        )}

                        <Divider className="my-2" />

                        {/* 4. Form Cập Nhật */}
                        <div className="flex flex-col gap-5">
                            <EnumPicker
                                label="Trạng thái phê duyệt"
                                placeholder="Chọn trạng thái công"
                                labels={TimeLogStatusLabels}
                                value={formik.values.status}
                                onChange={(val) => formik.setFieldValue("status", val)}
                                isClearable={false}
                                isInvalid={!!formik.errors.status}
                                isReadOnly={isReadOnly}
                            />

                            <div className="relative">
                                <TextareaStyled
                                    label="Ghi chú của quản lý"
                                    placeholder={
                                        formik.values.status === TimeLogStatus.Invalid
                                            ? "Bắt buộc: Tại sao ca này không hợp lệ?"
                                            : "Nhập ghi chú (không bắt buộc)..."
                                    }
                                    name="managerNote"
                                    value={formik.values.managerNote || ""}
                                    onChange={formik.handleChange}
                                    isInvalid={!!formik.errors.managerNote}
                                    errorMessage={formik.errors.managerNote}
                                    variant="bordered"
                                    isReadOnly={isReadOnly}
                                />
                                {formik.values.status === TimeLogStatus.Invalid &&
                                    !formik.values.managerNote && (
                                        <div className="absolute top-2 right-2 flex items-center gap-1 text-danger animate-pulse">
                                            <AlertTriangle size={14} />
                                            <span className="text-[10px] font-bold">BẮT BUỘC</span>
                                        </div>
                                    )}
                            </div>
                        </div>
                    </div>
                </ModalBody>

                <ModalFooter className="border-t bg-default-50/50 rounded-b-3xl">
                    <Button variant="light" onPress={onClose} isDisabled={isSubmitting}>
                        Hủy
                    </Button>
                    {!isReadOnly && (
                        <ButtonStyled
                            color="primary"
                            isLoading={isSubmitting}
                            isDisabled={!formik.dirty}
                            onPress={() => formik.handleSubmit()}
                            className="px-8"
                        >
                            Lưu thay đổi
                        </ButtonStyled>
                    )}
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
