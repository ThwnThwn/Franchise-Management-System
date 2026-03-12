import { confirmDialog } from "@/hooks"
import {
    useAutoFillShifts,
    useBulkDeleteShift,
    useBulkOpenShift,
    useBulkPublishShift
} from "@/features/shifts/hooks"
import { useMemo } from "react"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"
import dayjs from "dayjs"

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

export const useScheduleBulkActions = ({
    currentDate,
    dateRange,
    shiftsData
}: {
    currentDate: dayjs.Dayjs
    dateRange: {
        fromDate: string
        toDate: string
    }
    shiftsData: ShiftResponse[] | undefined
}) => {
    const forOpenStartDate = currentDate.isAfter(dayjs(dateRange.fromDate))
        ? currentDate
        : dayjs(dateRange.fromDate)

    const bulkPublish = useBulkPublishShift()
    const bulkDelete = useBulkDeleteShift()
    const bulkOpen = useBulkOpenShift()
    const autoFill = useAutoFillShifts({
        onSuccess: async (data) => {
            if (data.unfilledShiftIds && data.unfilledShiftIds.length > 0) {
                await delay(500)

                const isConfirmed = await confirmDialog({
                    title: "Cảnh báo thiếu nhân sự",
                    message: `Hệ thống không thể tìm đủ người cho ${data.unfilledShiftIds.length} ca làm việc. Bạn có muốn mở các ca này ra "Chợ ca" để nhân viên tự do đăng ký không?`,
                    variant: "warning",
                    confirmText: "Mở ra Chợ Ca ngay"
                })

                if (isConfirmed) {
                    bulkOpen.mutate(data.unfilledShiftIds)
                }
            }
        }
    })

    const bulkActionIds = useMemo(() => {
        // Khởi tạo object chứa 2 mảng rỗng
        const result = {
            forDelete: [] as string[], // Chỉ dành cho Draft
            forPublish: [] as string[], // Draft + Open (Chợ ca)
            forOpen: [] as string[],
            forAutoFill: [] as string[]
        }

        if (!shiftsData) return result

        shiftsData.forEach((s) => {
            const shiftDate = dayjs(s.shiftDate)
            // 1. Logic cho Xoá nhanh: Chỉ những ca còn là Nháp
            if (s.status === ShiftStatus.Draft) {
                result.forDelete.push(s.id)

                const needsStaffAndIsFuture =
                    s.assignedStaffCount < s.requiredStaffCount &&
                    !shiftDate.isBefore(forOpenStartDate, "day")

                if (needsStaffAndIsFuture) {
                    // Hợp nhất: Thỏa mãn là đẩy vào cả 2 danh sách luôn
                    result.forOpen.push(s.id)
                    result.forAutoFill.push(s.id)
                }
            }

            // 2. Logic cho Publish nhanh: Những ca chưa ban hành (Draft hoặc đang ở Chợ Ca)
            if (
                s.assignedStaffCount === s.requiredStaffCount &&
                (s.status === ShiftStatus.Draft || s.status === ShiftStatus.Open)
            ) {
                result.forPublish.push(s.id)
            }
        })

        return result
    }, [forOpenStartDate, shiftsData])

    const { forPublish, forDelete, forOpen, forAutoFill } = bulkActionIds

    const handleBulkPublish = async () => {
        if (forPublish.length === 0) return
        const isConfirmed = await confirmDialog({
            title: "Chốt lịch",
            message: `Bạn đang thực hiện công khai ${forPublish.length} ca làm việc (bao gồm cả ca Nháp và ca đang ở Chợ).`,
            variant: "info"
        })
        if (isConfirmed) bulkPublish.mutate(forPublish)
    }

    const handleBulkDelete = async () => {
        if (forDelete.length === 0) return
        const isConfirmed = await confirmDialog({
            title: "Xóa toàn bộ nháp",
            message: `Hành động này sẽ xóa ${forDelete.length} ca chưa công khai. Bạn có chắc không?`,
            variant: "danger"
        })
        if (isConfirmed) bulkDelete.mutate(forDelete)
    }

    const handleBulkOpen = async () => {
        if (forOpen.length === 0) return
        const isConfirmed = await confirmDialog({
            title: "Mở Chợ ca (Shift Pool)",
            message: `Bạn muốn đẩy ${forOpen.length} ca làm việc đang nháp lên Chợ ca để nhân viên có thể tự do đăng ký?`,
            variant: "warning",
            confirmText: "Mở ngay"
        })
        if (isConfirmed) bulkOpen.mutate(forOpen)
    }

    const handleAutoFill = async () => {
        if (forAutoFill.length === 0) {
            // Không có ca nháp nào cần điền
            return
        }

        const isConfirmed = await confirmDialog({
            title: "Tự động xếp ca",
            message: `Hệ thống sẽ chạy tự động phân công nhân viên vào các ca làm việc từ ${forOpenStartDate.format("DD/MM")} đến ${dayjs(dateRange.toDate).format("DD/MM")}. Bạn có chắc chắn muốn tiếp tục?`,
            variant: "info",
            confirmText: "Thực hiện ngay"
        })

        if (isConfirmed) {
            autoFill.mutate({
                startDate: forOpenStartDate.format("YYYY-MM-DD"),
                endDate: dateRange.toDate
            })
        }
    }

    return {
        bulkActionIds,
        handleBulkPublish,
        handleBulkDelete,
        handleBulkOpen,
        handleAutoFill,
        isProcessing:
            bulkPublish.isPending ||
            bulkDelete.isPending ||
            bulkOpen.isPending ||
            autoFill.isPending
    }
}
