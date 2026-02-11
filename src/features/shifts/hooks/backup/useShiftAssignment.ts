import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import { addToast } from "@heroui/react" // Hoặc path tương ứng của bro
import { StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { BackendError } from "@/models/common/respone"
import { shiftApi } from "@/features/shifts/services"
import { STRINGS } from "@/constants/strings"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMemo, useState } from "react"

export const useShiftAssignment = ({
    storeId,
    shiftId,
    onSuccess
}: {
    storeId: string
    shiftId: string
    onSuccess?: () => void
}) => {
    const queryClient = useQueryClient()
    const queryKey = [QUERY_KEYS.SHIFT_CANDIDATES, storeId, shiftId]

    // ===============================
    // Query and Mutation
    // ===============================
    const { data: candidates = [], isLoading } = useQuery<StaffAssignmentCandidate[]>({
        queryKey,
        queryFn: () => shiftApi.getAvailableStaffs({ storeId, shiftId }),
        enabled: !!storeId && !!shiftId
    })

    const syncMutation = useMutation({
        mutationFn: (userIds: string[]) => shiftApi.updateAssignments({ shiftId, userIds }),
        onSuccess: () => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật nhân sự thành công",
                color: "success"
            })
            queryClient.invalidateQueries({ queryKey })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({ title: error.title, description: error.detail, color: "danger" })
        }
    })

    // ===============================
    // Logic
    // ===============================
    const [draftIds, setDraftIds] = useState<string[] | null>(null)

    // 1. Danh sách ID gốc từ DB
    const originalAssignedIds = useMemo(
        () => candidates.filter((c) => c.isAssigned).map((c) => c.id),
        [candidates]
    )

    // 2. Mảng ID đang được chọn (nháp hoặc gốc)
    const effectiveSelectedIds = useMemo(
        () => draftIds ?? originalAssignedIds,
        [draftIds, originalAssignedIds]
    )

    // 3. Logic so sánh thay đổi
    const hasChanges = useMemo(() => {
        if (draftIds === null) return false
        return [...originalAssignedIds].sort().join(",") !== [...draftIds].sort().join(",")
    }, [originalAssignedIds, draftIds])

    // 4. PHÂN LOẠI DATA (Đây là chỗ sửa lỗi)
    // Assigned: Những người có ID nằm trong danh sách được chọn
    const assignedList = useMemo(
        () => candidates.filter((c) => effectiveSelectedIds.includes(c.id)),
        [candidates, effectiveSelectedIds]
    )

    // Available: Những người KHÔNG nằm trong danh sách được chọn
    const availableList = useMemo(
        () => candidates.filter((c) => !effectiveSelectedIds.includes(c.id)),
        [candidates, effectiveSelectedIds]
    )

    const handleToggle = (userId: string) => {
        // Tìm candidate để check hard-block
        const candidate = candidates.find((c) => c.id === userId)
        if (candidate?.isConflict || candidate?.validation?.isShuttleInvalid) return

        setDraftIds((prev) => {
            const current = prev ?? originalAssignedIds
            return current.includes(userId)
                ? current.filter((id) => id !== userId)
                : [...current, userId]
        })
    }

    const handleReset = () => setDraftIds(null)

    return {
        candidates,
        isLoading,
        syncAssignments: syncMutation.mutate,
        isSyncing: syncMutation.isPending,
        hasChanges,
        effectiveSelectedIds,
        assignedList,
        availableList,
        handleToggle,
        handleReset
    }
}
