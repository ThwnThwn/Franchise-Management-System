"use client"

import React, { useEffect } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Select,
    SelectItem,
    User,
    Textarea,
    Spinner,
    Chip
} from "@heroui/react"
import { useFormik } from "formik"
import { ArrowRight, Clock, AlertTriangle } from "lucide-react"
import dayjs from "dayjs"
import { ModalStyled, ButtonStyled } from "@/components"
import { ShiftResponse, StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { useRequestSwap, useGetSwapCandidates } from "@/features/shifts/hooks" // Path của bro
import { createSwapSchema, CreateSwapFormValues } from "./schema" // Schema mới viết
import { useFormikHelpers } from "@/hooks"
import { getValidationErrors } from "@/features/shifts/utils"

interface CreateSwapModalProps {
    isOpen: boolean
    onClose: () => void
    shift: ShiftResponse
    assignmentId: string
}

export const CreateSwapModal = ({ isOpen, onClose, shift, assignmentId }: CreateSwapModalProps) => {
    // 1. Fetch danh sách ứng viên (đã lọc Hard Block)
    const { availableCandidates, isLoading: isLoadingCandidates } = useGetSwapCandidates(
        shift.id,
        isOpen
    )

    // 2. Mutation Submit
    const { mutate: requestSwap, isPending } = useRequestSwap({
        onSuccess: () => {
            handleClose()
        }
    })

    // 3. Khởi tạo Formik
    const formik = useFormik<CreateSwapFormValues>({
        initialValues: {
            targetUserId: "",
            reason: ""
        },
        validationSchema: createSwapSchema,
        onSubmit: (values) => {
            requestSwap({
                assignmentId: assignmentId,
                req: {
                    targetUserId: values.targetUserId,
                    reason: values.reason
                }
            })
        }
    })

    // 4. Hook Helpers để bắt lỗi UI
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    // 5. Reset form khi đóng/mở
    useEffect(() => {
        if (isOpen) {
            formik.resetForm()
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen])

    const handleClose = () => {
        formik.resetForm()
        onClose()
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={handleClose} isKeyboardDismissDisabled={false}>
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1">Gửi yêu cầu đổi ca</ModalHeader>
                <ModalBody className="py-2 flex flex-col gap-5">
                    {/* Tóm tắt ca làm */}
                    <div className="bg-primary-50 p-3 rounded-lg border border-primary-100 flex items-center justify-between">
                        <div>
                            <p className="text-xs text-primary-600 font-bold uppercase mb-1">
                                Đổi ca:
                            </p>
                            <p className="text-lg font-black text-primary-900 leading-tight">
                                {dayjs(shift.shiftDate).format("DD/MM/YYYY")}
                            </p>
                        </div>
                        <div className="flex items-center gap-1.5 bg-white px-3 py-2 rounded-md shadow-sm border border-primary-100 font-bold text-primary-700">
                            <Clock size={16} />
                            {shift.startTime.substring(0, 5)} - {shift.endTime.substring(0, 5)}
                        </div>
                    </div>

                    {/* Chọn người đi thay */}
                    <div>
                        <p className="text-sm font-bold text-default-700 mb-2">
                            Người đi thay <span className="text-danger">*</span>
                        </p>
                        {isLoadingCandidates ? (
                            <div className="h-14 bg-default-100 rounded-xl flex items-center justify-center animate-pulse">
                                <Spinner size="sm" color="primary" />
                            </div>
                        ) : availableCandidates.length === 0 ? (
                            <div className="bg-danger-50 text-danger-600 p-3 rounded-xl border border-danger-100 text-sm italic">
                                Không có ai rảnh rỗi trong khung giờ này. Bạn hãy thử &quot;Nhả ca
                                về Chợ&quot; nhé!
                            </div>
                        ) : (
                            <Select
                                items={availableCandidates}
                                aria-label="Chọn người thay thế"
                                placeholder="Chọn 1 đồng nghiệp..."
                                selectedKeys={
                                    formik.values.targetUserId ? [formik.values.targetUserId] : []
                                }
                                onSelectionChange={(keys) => {
                                    formik.setFieldValue(
                                        "targetUserId",
                                        Array.from(keys)[0] as string
                                    )
                                }}
                                onClose={() => formik.setFieldTouched("targetUserId", true)}
                                isInvalid={isInvalid("targetUserId")}
                                errorMessage={getErrorMessage("targetUserId")}
                                classNames={{
                                    trigger:
                                        "h-14 bg-default-50 border border-default-200 shadow-sm"
                                }}
                                renderValue={(items) => {
                                    const selectedCandidate = items[0]
                                        ?.data as StaffAssignmentCandidate

                                    if (!selectedCandidate) return null

                                    return (
                                        <div className="flex items-center gap-2">
                                            <User
                                                name={`${selectedCandidate.firstName} ${selectedCandidate.lastName || ""}`}
                                                avatarProps={{
                                                    src: selectedCandidate.avatarUrl,
                                                    size: "sm"
                                                }}
                                            />
                                        </div>
                                    )
                                }}
                            >
                                {(c) => {
                                    const errors = getValidationErrors(c)
                                    const hasWarning = errors.length > 0

                                    return (
                                        <SelectItem
                                            key={c.id}
                                            textValue={`${c.firstName} ${c.lastName || ""}`}
                                        >
                                            <div className="flex justify-between items-center w-full">
                                                <User
                                                    name={`${c.firstName} ${c.lastName || ""}`}
                                                    description={`${c.currentHourlyWage.toLocaleString()}đ/h`}
                                                    avatarProps={{ src: c.avatarUrl, size: "sm" }}
                                                />
                                                {hasWarning && (
                                                    <Chip
                                                        size="sm"
                                                        color="warning"
                                                        variant="flat"
                                                        startContent={<AlertTriangle size={12} />}
                                                    >
                                                        Cảnh báo lịch
                                                    </Chip>
                                                )}
                                            </div>
                                        </SelectItem>
                                    )
                                }}
                            </Select>
                        )}
                    </div>

                    {/* Nhập lý do */}
                    <div>
                        <p className="text-sm font-bold text-default-700 mb-2">
                            Lý do (Không bắt buộc)
                        </p>
                        <Textarea
                            placeholder="Nhập lý do để tăng tỷ lệ đồng nghiệp nhận lời..."
                            variant="faded"
                            minRows={2}
                            name="reason"
                            value={formik.values.reason}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            isInvalid={isInvalid("reason")}
                            errorMessage={getErrorMessage("reason")}
                        />
                    </div>
                </ModalBody>

                <ModalFooter className="border-t border-divider pt-4 mt-2">
                    <ButtonStyled variant="light" onPress={handleClose} isDisabled={isPending}>
                        Hủy
                    </ButtonStyled>
                    <ButtonStyled
                        color="primary"
                        endContent={<ArrowRight size={16} />}
                        className="font-bold shadow-md px-6"
                        isLoading={isPending}
                        isDisabled={availableCandidates.length === 0}
                        onPress={() => formik.handleSubmit()} // Gọi trigger submit của Formik
                    >
                        Gửi yêu cầu
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
