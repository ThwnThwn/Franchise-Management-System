import { confirmDialog } from "@/hooks"
import { useBulkDeleteShift, useBulkOpenShift, useBulkPublishShift } from "@/features/shifts/hooks"
import { useMemo } from "react"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"

export const useScheduleBulkActions = (shiftsData: ShiftResponse[] | undefined) => {
    const bulkPublish = useBulkPublishShift()
    const bulkDelete = useBulkDeleteShift()
    const bulkOpen = useBulkOpenShift()

    const bulkActionIds = useMemo(() => {
        // Khởi tạo object chứa 2 mảng rỗng
        const result = {
            forDelete: [] as string[], // Chỉ dành cho Draft
            forPublish: [] as string[], // Draft + Open (Chợ ca)
            forOpen: [] as string[]
        }

        if (!shiftsData) return result

        shiftsData.forEach((s) => {
            // 1. Logic cho Xoá nhanh: Chỉ những ca còn là Nháp
            if (s.status === ShiftStatus.Draft) {
                result.forDelete.push(s.id)
                result.forOpen.push(s.id)
            }

            // 2. Logic cho Publish nhanh: Những ca chưa ban hành (Draft hoặc đang ở Chợ Ca)
            if (s.status === ShiftStatus.Draft || s.status === ShiftStatus.Open) {
                result.forPublish.push(s.id)
            }
        })

        return result
    }, [shiftsData])

    const { forPublish, forDelete, forOpen } = bulkActionIds

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

    return {
        bulkActionIds,
        handleBulkPublish,
        handleBulkDelete,
        handleBulkOpen,
        isProcessing: bulkPublish.isPending || bulkDelete.isPending || bulkOpen.isPending
    }
}
