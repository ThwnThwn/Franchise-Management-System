"use client"

import { useEffect } from "react"
import { Select, SelectItem, Spinner } from "@heroui/react"
import { useFormik } from "formik"
import { ArrowRight, Clock, Store } from "lucide-react"
import dayjs from "dayjs"
import { ButtonStyled, TextareaStyled } from "@/components"
import { ShiftResponse } from "@/features/shifts/models/response"
import { useRequestSwap, useGetSwapCandidates, useReleaseShift } from "@/features/shifts/hooks"
import { createSwapSchema, CreateSwapFormValues } from "./schema"
import { useFormikHelpers, useUserHelper } from "@/hooks"
import { CandidateOptionUI } from "./CandidateOptionUI"

interface CreateSwapFormBodyProps {
    shift: ShiftResponse
    assignmentId: string
    // Truyền prop để báo cho Component cha biết lúc tạo xong (để cha tắt hoặc chuyển view)
    onSuccess?: () => void
}

export const CreateSwapFormBody = ({ shift, assignmentId, onSuccess }: CreateSwapFormBodyProps) => {
    const { toFullName } = useUserHelper()
    // 1. Fetch danh sách ứng viên (Truyền true thay vì isOpen vì Tab này render là fetch luôn)
    const { availableCandidates, isLoading: isLoadingCandidates } = useGetSwapCandidates(
        shift.id,
        true
    )

    // 2. Mutation Submit
    const { mutate: requestSwap, isPending: isRequesting } = useRequestSwap({
        onSuccess: () => {
            formik.resetForm()
            onSuccess?.() // Gọi callback cho thằng cha xử lý tiếp
        }
    })

    const { releaseWithConfirm, isPending: isReleasing } = useReleaseShift({
        onSuccess: () => {
            onSuccess?.()
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

    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    // Reset form khi unmount
    useEffect(() => {
        return () => formik.resetForm()
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    const isProcessing = isRequesting || isReleasing

    return (
        <div className="flex flex-col gap-5 py-2">
            {/* Tóm tắt ca làm */}
            <div className="bg-primary-50 p-3 rounded-lg border border-primary-100 flex items-center justify-between">
                <div>
                    <p className="text-xs text-primary-600 font-bold uppercase mb-1">
                        Gửi Yêu Cầu Đổi Ca:
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
                <p className="text-sm font-bold text-default-700 mb-2">Người đi thay</p>
                {isLoadingCandidates ? (
                    <div className="h-14 bg-default-100 rounded-xl flex items-center justify-center animate-pulse">
                        <Spinner size="sm" color="primary" />
                    </div>
                ) : availableCandidates.length === 0 ? (
                    <div className="bg-warning-50 text-warning-700 p-4 rounded-xl border border-warning-200 text-sm italic">
                        Không có đồng nghiệp nào rảnh rỗi trong khung giờ này. Bạn hãy dùng nút{" "}
                        <b>Nhả ca về Chợ</b> ở bên dưới để người khác có thể tự do đăng ký nhé!
                    </div>
                ) : (
                    <Select
                        items={availableCandidates}
                        aria-label="Chọn người thay thế"
                        placeholder="Chọn 1 đồng nghiệp..."
                        isClearable={false}
                        selectedKeys={
                            formik.values.targetUserId ? [formik.values.targetUserId] : []
                        }
                        onSelectionChange={(keys) => {
                            const selectedValue = Array.from(keys)[0] as string
                            if (selectedValue) {
                                formik.setFieldValue("targetUserId", selectedValue)
                            }
                        }}
                        isInvalid={isInvalid("targetUserId")}
                        errorMessage={getErrorMessage("targetUserId")}
                        classNames={{
                            trigger: "h-14 bg-default-50 border border-default-200 shadow-sm"
                        }}
                        renderValue={(items) => {
                            const selectedCandidate = items[0]?.data
                            if (!selectedCandidate) return null

                            return (
                                <CandidateOptionUI
                                    candidate={selectedCandidate}
                                    placement="top"
                                    className="pr-4"
                                />
                            )
                        }}
                    >
                        {(c) => (
                            <SelectItem key={c.id} textValue={toFullName(c)}>
                                <CandidateOptionUI candidate={c} placement="left" />
                            </SelectItem>
                        )}
                    </Select>
                )}
            </div>

            {/* Nhập lý do */}
            <div>
                <p className="text-sm font-bold text-default-700 mb-2">Lý do</p>
                <TextareaStyled
                    placeholder="Nhập lý do để tăng tỷ lệ đồng nghiệp nhận lời..."
                    variant="faded"
                    minRows={2}
                    name="reason"
                    value={formik.values.reason}
                    onChange={formik.handleChange}
                    onBlur={formik.handleBlur}
                    isInvalid={isInvalid("reason")}
                    errorMessage={getErrorMessage("reason")}
                    isRequired
                />
            </div>

            {/* Footer / Hành động */}
            <div className="border-t border-divider pt-4 mt-2 flex justify-end space-x-2">
                {/* NÚT BÊN TRÁI: NHẢ CA */}
                <ButtonStyled
                    variant="flat"
                    color="warning"
                    className="font-bold"
                    startContent={<Store size={16} />}
                    isLoading={isReleasing}
                    isDisabled={isProcessing}
                    onPress={() => releaseWithConfirm(assignmentId)}
                >
                    Nhả ca về Chợ
                </ButtonStyled>

                {/* NÚT BÊN PHẢI: GỬI YÊU CẦU ĐỔI CA */}
                <ButtonStyled
                    color="primary"
                    endContent={<ArrowRight size={16} />}
                    className="font-bold shadow-md px-6"
                    isLoading={isRequesting}
                    isDisabled={availableCandidates.length === 0 || isProcessing}
                    onPress={() => formik.handleSubmit()}
                >
                    Xác nhận gửi
                </ButtonStyled>
            </div>
        </div>
    )
}
