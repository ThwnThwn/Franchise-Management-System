import { QUERY_KEYS } from "@/constants/queryKeys"
import { STRINGS } from "@/constants/strings"
import { addToast } from "@heroui/react"
import { StaffAssignmentCandidate } from "../models/response"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { fakeDelay, MOCK_SHIFTS_DB } from "./useShifts"

// --- MOCK CANDIDATES GENERATOR ---
const generateMockCandidates = (shiftId: string): StaffAssignmentCandidate[] => {
    return [
        {
            id: "user-1",
            firstName: "Văn A",
            lastName: "Nguyễn",
            avatarUrl: "https://i.pravatar.cc/150?u=user-1",
            currentHourlyWage: 25000,
            isAvailable: true,
            isConflict: false,
            isAssigned: shiftId === "shift-1", // Giả định user-1 đã được gán vào shift-1
            note: "Sẵn sàng tăng ca"
        },
        {
            id: "user-2",
            firstName: "Thị B",
            lastName: "Trần",
            avatarUrl: "https://i.pravatar.cc/150?u=user-2",
            currentHourlyWage: 22000,
            isAvailable: true,
            isConflict: false,
            isAssigned: shiftId === "shift-1",
            note: ""
        },
        {
            id: "user-3",
            firstName: "Minh C",
            lastName: "Lê",
            avatarUrl: "https://i.pravatar.cc/150?u=user-3",
            currentHourlyWage: 28000,
            isAvailable: false,
            isConflict: false,
            isAssigned: false,
            note: "Đi học thêm buổi sáng"
        },
        {
            id: "user-4",
            firstName: "Hoàng D",
            lastName: "Phạm",
            avatarUrl: "https://i.pravatar.cc/150?u=user-4",
            currentHourlyWage: 24000,
            isAvailable: true,
            isConflict: true,
            isAssigned: false,
            note: "Trùng lịch ca Part-time tại Store khác"
        },
        {
            id: "user-5",
            firstName: "Thanh E",
            lastName: "Vũ",
            avatarUrl: "https://i.pravatar.cc/150?u=user-5",
            currentHourlyWage: 30000,
            isAvailable: true,
            isConflict: false,
            isAssigned: false,
            note: ""
        }
    ]
}

// Giả lập DB tạm thời cho candidates
let MOCK_CANDIDATES_DB: Record<string, StaffAssignmentCandidate[]> = {}

// --- HOOK IMPLEMENTATION ---

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

    // Query lấy danh sách ứng viên
    const { data: candidates = [], isLoading } = useQuery<StaffAssignmentCandidate[]>({
        queryKey,
        queryFn: async () => {
            await fakeDelay(800) // Delay lâu hơn chút để test Skeleton/Spinner

            // Nếu chưa có data cho shift này trong mock DB, khởi tạo nó
            if (!MOCK_CANDIDATES_DB[shiftId]) {
                MOCK_CANDIDATES_DB[shiftId] = generateMockCandidates(shiftId)
            }

            return MOCK_CANDIDATES_DB[shiftId]
        },
        enabled: !!storeId && !!shiftId
    })

    // Mutation đồng bộ (Sync) mảng nhân sự
    const syncMutation = useMutation({
        mutationFn: async (userIds: string[]) => {
            await fakeDelay(1000)
            console.log(`[MOCK API] Syncing shift ${shiftId} with users:`, userIds)

            // Cập nhật trạng thái isAssigned trong Mock DB
            if (MOCK_CANDIDATES_DB[shiftId]) {
                MOCK_CANDIDATES_DB[shiftId] = MOCK_CANDIDATES_DB[shiftId].map((candidate) => ({
                    ...candidate,
                    isAssigned: userIds.includes(candidate.id)
                }))
            }

            // Đồng bộ ngược lại vào MOCK_SHIFTS_DB để Calendar cập nhật count
            const shiftIndex = MOCK_SHIFTS_DB.findIndex((s) => s.id === shiftId)
            if (shiftIndex !== -1) {
                MOCK_SHIFTS_DB[shiftIndex].assignedStaffCount = userIds.length
            }
        },
        onSuccess: () => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật nhân sự thành công (MOCK)",
                color: "success"
            })
            // Invalidate các query liên quan
            queryClient.invalidateQueries({ queryKey })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            onSuccess?.()
        },
        onError: (error: any) => {
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: error?.detail || "Lỗi Mock sync",
                color: "danger"
            })
        }
    })

    return {
        candidates,
        isLoading,
        syncAssignments: syncMutation.mutate,
        isSyncing: syncMutation.isPending
    }
}
