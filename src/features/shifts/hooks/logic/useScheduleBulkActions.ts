import { confirmDialog } from "@/hooks"
import { useBulkDeleteShift, useBulkPublishShift } from "@/features/shifts/hooks"

export const useScheduleBulkActions = (draftShiftIds: string[]) => {
    const bulkPublish = useBulkPublishShift()
    const bulkDelete = useBulkDeleteShift()

    const handleBulkPublish = async () => {
        if (draftShiftIds.length === 0) return
        const isConfirmed = await confirmDialog({
            title: "Chốt lịch tuần",
            message: `Bạn muốn công khai ${draftShiftIds.length} ca làm việc?`,
            variant: "info"
        })
        if (isConfirmed) bulkPublish.mutate(draftShiftIds)
    }

    const handleBulkDelete = async () => {
        if (draftShiftIds.length === 0) return
        const isConfirmed = await confirmDialog({
            title: "Xóa toàn bộ nháp",
            message: `Hành động này sẽ xóa ${draftShiftIds.length} ca chưa công khai. Bạn có chắc không?`,
            variant: "danger"
        })
        if (isConfirmed) bulkDelete.mutate(draftShiftIds)
    }

    return {
        handleBulkPublish,
        handleBulkDelete,
        isProcessing: bulkPublish.isPending || bulkDelete.isPending
    }
}
