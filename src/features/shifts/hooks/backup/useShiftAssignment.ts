import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import { addToast } from "@heroui/react" // Hoặc path tương ứng của bro
import { StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { BackendError } from "@/models/common/respone"
import { shiftApi } from "@/features/shifts/services"
import { STRINGS } from "@/constants/strings"
import { QUERY_KEYS } from "@/constants/queryKeys"

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

    return {
        candidates, // Dữ liệu gốc từ server
        isLoading,
        syncAssignments: syncMutation.mutate,
        isSyncing: syncMutation.isPending
    }
}
