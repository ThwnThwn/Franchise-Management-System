import { QUERY_KEYS } from "@/constants/queryKeys"
import { STRINGS } from "@/constants/strings"
import { addToast } from "@heroui/react"
import { StaffAssignmentCandidate } from "../models/response"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { fakeDelay, MOCK_SHIFTS_DB } from "./useShifts"
import { useMemo, useState } from "react"

// Giả lập DB tạm thời cho candidates
let MOCK_CANDIDATES_DB: Record<string, StaffAssignmentCandidate[]> = {}

// --- MOCK CANDIDATES GENERATOR (Thêm dữ liệu validation thô) ---
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
            isAssigned: shiftId === "shift-1",
            note: "Sẵn sàng tăng ca",
            validation: {
                isRestTimeInvalid: false,
                restHours: 12,
                isOverLimit: false,
                weeklyHours: 35,
                isShuttleInvalid: false
            }
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
            note: "",
            validation: {
                isRestTimeInvalid: true,
                restHours: 6,
                isOverLimit: false,
                weeklyHours: 20,
                isShuttleInvalid: false
            }
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
            note: "Đi học thêm buổi sáng",
            validation: {
                isRestTimeInvalid: false,
                restHours: 24,
                isOverLimit: true,
                weeklyHours: 42,
                isShuttleInvalid: false
            }
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
            note: "Trùng lịch ca Part-time tại Store khác",
            validation: {
                isRestTimeInvalid: false,
                restHours: 8,
                isOverLimit: false,
                weeklyHours: 15,
                isShuttleInvalid: false
            }
        },
        // --- THÊM USER E (Cảnh báo nghỉ ít) ---
        {
            id: "user-5",
            firstName: "Thanh E",
            lastName: "Vũ",
            avatarUrl: "https://i.pravatar.cc/150?u=user-5",
            currentHourlyWage: 30000,
            isAvailable: true,
            isConflict: false,
            isAssigned: false,
            note: "Nhân viên cứng",
            validation: {
                isRestTimeInvalid: true,
                restHours: 4.5, // Nghỉ quá ít, BE trả về 4.5h
                isOverLimit: false,
                weeklyHours: 30,
                isShuttleInvalid: false
            }
        },
        // --- THÊM USER F (Cảnh báo quá giờ tuần) ---
        {
            id: "user-6",
            firstName: "Quốc F",
            lastName: "Đỗ",
            avatarUrl: "https://i.pravatar.cc/150?u=user-6",
            currentHourlyWage: 27000,
            isAvailable: true,
            isConflict: false,
            isAssigned: false,
            note: "",
            validation: {
                isRestTimeInvalid: false,
                restHours: 15,
                isOverLimit: true,
                weeklyHours: 45, // Đã làm 45h, vượt 40h quy định
                isShuttleInvalid: false
            }
        }
    ]
}

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

    // ===============================
    // Query
    // ===============================
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

    // ===============================
    // Mutation
    // ===============================
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
