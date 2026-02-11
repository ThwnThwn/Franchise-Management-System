import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import { addToast } from "@heroui/react" // Hoặc path tương ứng của bro
import { StaffAssignmentCandidate } from "../models/response"
import { BackendError } from "@/models/common/respone"
import { shiftApi } from "../services"
import { STRINGS } from "@/constants/strings"

export const useShiftAssignment = (storeId: string, shiftId: string) => {
    const queryClient = useQueryClient()
    const queryKey = ["shift-candidates", storeId, shiftId]

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
            queryClient.invalidateQueries({ queryKey: ["shifts"] })
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
