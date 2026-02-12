import { useState } from "react"
import { useCreateShift, useDeleteShift, useUpdateShift } from "@/features/shifts/hooks"
import { ShiftFormValues } from "@/features/shifts/components/ShiftModal/ShiftForm/schema"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"
import { CreateShiftRequest, UpdateShiftRequest } from "@/features/shifts/models/request"
import { addToast } from "@heroui/react"
import { confirmDialog, useDay } from "@/hooks"

export const useShiftActions = ({
    storeId,
    shiftsData,
    onOpenModal,
    onCloseModal
}: {
    storeId: string
    shiftsData: ShiftResponse[]
    onOpenModal: () => void
    onCloseModal: () => void
}) => {
    const { formatDate, formatDateTime } = useDay({
        defaultFormat: "HH:mm"
    })

    const [selectedShift, setSelectedShift] = useState<Partial<ShiftFormValues> | null>(null)
    const [selectedShiftId, setSelectedShiftId] = useState<string | null>(null)

    const createMutation = useCreateShift({ onSuccess: onCloseModal })
    const updateMutation = useUpdateShift({ onSuccess: onCloseModal })
    const deleteMutation = useDeleteShift({ onSuccess: onCloseModal })

    // 1. Click vào ô trống -> Mở Modal tạo mới
    const handleDateClick = (date: Date) => {
        setSelectedShiftId(null)
        setSelectedShift({
            id: undefined,
            shiftDate: date,
            startTime: "08:00", // Default
            endTime: "12:00"
        })
        onOpenModal()
    }

    // 2. Click vào Event có sẵn -> Mở Modal Edit
    const handleEventClick = (shift: ShiftResponse) => {
        setSelectedShiftId(shift.id)

        setSelectedShift({
            id: shift.id,
            shiftDate: new Date(shift.shiftDate),
            startTime: shift.startTime, // Lấy HH:mm từ ISO string
            endTime: shift.endTime,
            requiredStaffCount: shift.requiredStaffCount,
            breakDurationMinutes: shift.breakDurationMinutes,
            note: shift.note,
            status: shift.status,
            shiftTemplateId: shift.shiftTemplateId
        })
        onOpenModal()
    }

    // 3. Kéo thả (Drag & Drop) -> Update ngay lập tức
    const handleEventDrop = (
        shiftId: string,
        newStart: Date,
        newEnd: Date,
        dropInfo: any // Thêm tham số này
    ) => {
        const targetShift = shiftsData?.find((s) => s.id === shiftId)

        const isSubmitable =
            targetShift?.status === ShiftStatus.Draft ||
            targetShift?.status === ShiftStatus.Published

        if (!isSubmitable) {
            // 1. Thực hiện REVERT ngay lập tức trên UI
            if (dropInfo && typeof dropInfo.revert === "function") {
                dropInfo.revert()
            }

            // 2. Báo lỗi cho User
            addToast({
                title: "Không thể di chuyển",
                description: "Ca làm việc đã hoàn thành hoặc đã hủy không thể thay đổi thời gian.",
                color: "warning"
            })

            return
        }

        // --- Logic update API ---
        const startISO = formatDateTime({ date: newStart })
        const endISO = formatDateTime({ date: newEnd })

        if (!startISO || !endISO) {
            dropInfo?.revert()
            return
        }

        updateMutation.mutate(
            {
                id: shiftId,
                req: {
                    startTime: startISO,
                    endTime: endISO,
                    shiftDate: formatDate(newStart)
                }
            },
            {
                onSettled: (_, error) => {
                    if (error) {
                        // Chỉ revert khi có lỗi
                        dropInfo?.revert()
                    }
                }
            }
        )
    }

    // 4. Submit Form (Create or Update)
    const handleModalSubmit = (data: ShiftFormValues) => {
        // 1. Format Shift Date (Dùng hàm có sẵn của bạn)
        const dateStr = formatDate(data.shiftDate) // Ra "2026-02-10"

        // 2. Logic tính toán giờ (Vẫn cần làm ở đây vì liên quan business logic ca đêm)
        // const startDateTime = dayjs(`${dateStr}T${data.startTime}:00`)
        // let endDateTime = dayjs(`${dateStr}T${data.endTime}:00`)

        // if (endDateTime.isBefore(startDateTime) || endDateTime.isSame(startDateTime)) {
        //     endDateTime = endDateTime.add(1, "day")
        // }

        // 3. Convert sang UTC để gửi API (Dùng hàm MỚI)
        // const startISO = formatToApiUTC(startDateTime)
        // const endISO = formatToApiUTC(endDateTime)

        if (selectedShiftId) {
            // Update Mode
            const payload: UpdateShiftRequest = {
                // shiftDate vẫn giữ nguyên là ngày bắt đầu ca (để BE biết ca này thuộc ngày nào)
                shiftDate: dateStr,
                startTime: data.startTime,
                endTime: data.endTime,
                requiredStaffCount: data.requiredStaffCount,
                breakDurationMinutes: data.breakDurationMinutes,
                note: data.note || "",
                status: data.status,
                shiftTemplateId: data.shiftTemplateId || undefined
            }

            updateMutation.mutate({ id: selectedShiftId, req: payload })
        } else {
            // Create Mode
            const payload: CreateShiftRequest = {
                storeId: storeId,
                shiftDate: dateStr,
                startTime: data.startTime,
                endTime: data.endTime,
                requiredStaffCount: data.requiredStaffCount,
                breakDurationMinutes: data.breakDurationMinutes,
                note: data.note || "",
                shiftTemplateId: data.shiftTemplateId || undefined
            }
            createMutation.mutate(payload)
        }
    }

    // 5. Delete Handler
    const handleDelete = async () => {
        if (!selectedShiftId) return

        const isConfirmed = await confirmDialog({
            title: "Xóa Ca làm việc",
            message: "Hành động này không thể hoàn tác. Bạn chắc chứ?",
            variant: "danger"
        })
        if (isConfirmed) {
            deleteMutation.mutate({ id: selectedShiftId })
        }
    }

    return {
        selectedShift,
        selectedShiftId,
        handleModalSubmit,
        handleDateClick,
        handleEventClick,
        handleEventDrop,
        handleDelete,
        isSubmitting: createMutation.isPending || updateMutation.isPending
    }
}
