"use client"

import { useState, useMemo, useCallback } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { STRINGS } from "@/constants/strings"
import { addToast } from "@heroui/react"
import { StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { fakeDelay, MOCK_SHIFTS_DB } from "./useShifts" // Đảm bảo đúng path

// --- MOCK DATABASE TẠI CHỖ ---
let MOCK_CANDIDATES_DB: Record<string, StaffAssignmentCandidate[]> = {}

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
                restHours: 4.5,
                isOverLimit: false,
                weeklyHours: 30,
                isShuttleInvalid: false
            }
        },
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
                weeklyHours: 45,
                isShuttleInvalid: false
            }
        }
    ]
}

export const useShiftAssignment = ({
    storeId,
    shiftId,
    requiredStaffCount,
    onSuccess
}: {
    storeId: string
    shiftId: string
    requiredStaffCount: number
    onSuccess?: () => void
}) => {
    const queryClient = useQueryClient()
    const queryKey = [QUERY_KEYS.SHIFT_CANDIDATES, storeId, shiftId]

    // --- State cho UI ---
    const [keyword, setKeyword] = useState("")
    const [draftIds, setDraftIds] = useState<string[] | null>(null)

    // --- 1. Query fetch data mock ---
    const { data: candidates = [], isLoading } = useQuery<StaffAssignmentCandidate[]>({
        queryKey,
        queryFn: async () => {
            await fakeDelay(800)
            if (!MOCK_CANDIDATES_DB[shiftId]) {
                MOCK_CANDIDATES_DB[shiftId] = generateMockCandidates(shiftId)
            }
            return MOCK_CANDIDATES_DB[shiftId]
        },
        enabled: !!storeId && !!shiftId
    })

    // --- 2. Mutation sync data ---
    const syncMutation = useMutation({
        mutationFn: async (userIds: string[]) => {
            await fakeDelay(1000)
            if (MOCK_CANDIDATES_DB[shiftId]) {
                MOCK_CANDIDATES_DB[shiftId] = MOCK_CANDIDATES_DB[shiftId].map((c) => ({
                    ...c,
                    isAssigned: userIds.includes(c.id)
                }))
            }
            // Cập nhật ngược lại MOCK_SHIFTS_DB nếu có
            const shiftIndex = MOCK_SHIFTS_DB.findIndex((s) => s.id === shiftId)
            if (shiftIndex !== -1) {
                MOCK_SHIFTS_DB[shiftIndex].assignedStaffCount = userIds.length
            }
        },
        onSuccess: () => {
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thành công",
                color: "success"
            })
            queryClient.invalidateQueries({ queryKey })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SHIFTS] })
            setDraftIds(null)
            onSuccess?.()
        }
    })

    // --- 3. Logic xử lý danh sách ---
    const originalAssignedIds = useMemo(
        () => candidates.filter((c) => c.isAssigned).map((c) => c.id),
        [candidates]
    )
    const effectiveSelectedIds = useMemo(
        () => draftIds ?? originalAssignedIds,
        [draftIds, originalAssignedIds]
    )
    const selectedSet = useMemo(() => new Set(effectiveSelectedIds), [effectiveSelectedIds])

    const hasChanges = useMemo(() => {
        if (draftIds === null) return false
        return [...originalAssignedIds].sort().join(",") !== [...draftIds].sort().join(",")
    }, [originalAssignedIds, draftIds])

    const filteredCandidates = useMemo(() => {
        const lowKey = keyword.toLowerCase()
        return candidates.filter((c) =>
            `${c.firstName} ${c.lastName}`.toLowerCase().includes(lowKey)
        )
    }, [candidates, keyword])

    const assignedList = useMemo(
        () => candidates.filter((c) => selectedSet.has(c.id)),
        [candidates, selectedSet]
    )
    // 1. Hàm tính điểm ưu tiên (Số càng nhỏ càng ưu tiên)
    const getPriorityScore = (c: StaffAssignmentCandidate) => {
        // Tầng 3: Chặn cứng (Nặng nhất)
        if (c.isConflict || c.validation?.isShuttleInvalid) return 3

        // Tầng 2: Có cảnh báo hoặc báo bận cá nhân
        const hasWarnings =
            !c.isAvailable ||
            (c.validation && (c.validation.isRestTimeInvalid || c.validation.isOverLimit))
        if (hasWarnings) return 2

        // Tầng 1: Sạch sẽ, khả dụng hoàn toàn
        return 1
    }

    // 2. Áp dụng vào availableList
    const availableList = useMemo(() => {
        return filteredCandidates
            .filter((c) => !selectedSet.has(c.id))
            .sort((a, b) => {
                const scoreA = getPriorityScore(a)
                const scoreB = getPriorityScore(b)

                // Sort theo tầng ưu tiên trước
                if (scoreA !== scoreB) return scoreA - scoreB

                // Nếu cùng tầng, có thể sort thêm theo tên để dễ tìm
                return a.firstName.localeCompare(b.firstName)
            })
    }, [filteredCandidates, selectedSet])

    // --- 4. Event Handlers ---
    const handleToggle = useCallback(
        (userId: string) => {
            const candidate = candidates.find((c) => c.id === userId)
            if (candidate?.isConflict || candidate?.validation?.isShuttleInvalid) return

            setDraftIds((prev) => {
                const current = prev ?? originalAssignedIds
                return current.includes(userId)
                    ? current.filter((id) => id !== userId)
                    : [...current, userId]
            })
        },
        [candidates, originalAssignedIds]
    )

    const handleReset = () => {
        setDraftIds(null)
        setKeyword("")
    }

    const handleQuickSuggest = useCallback(() => {
        const currentIds = draftIds ?? originalAssignedIds
        const neededCount = requiredStaffCount - currentIds.length

        if (neededCount <= 0) {
            addToast({
                title: "Thông báo",
                description: "Ca làm việc đã đủ (hoặc thừa) nhân sự rồi bro!",
                color: "warning"
            })
            return
        }

        // Lấy những người sạch nhất (Priority 1) từ danh sách available
        // availableList đã được sort sẵn nên mình cứ thế mà "hốt" từ đầu xuống
        const bestCandidates = availableList
            .filter((c) => getPriorityScore(c) === 1) // Chỉ lấy người "sạch" hoàn toàn
            .slice(0, neededCount)
            .map((c) => c.id)

        if (bestCandidates.length === 0) {
            addToast({
                title: "Không tìm thấy ứng viên tốt",
                description: "Không còn nhân viên nào rảnh hoàn toàn để gợi ý.",
                color: "danger"
            })
            return
        }

        // Cập nhật vào bản nháp
        setDraftIds([...currentIds, ...bestCandidates])

        addToast({
            title: "Gợi ý thành công",
            description: `Đã tự động chọn thêm ${bestCandidates.length} nhân viên tốt nhất.`,
            color: "success"
        })
    }, [draftIds, originalAssignedIds, availableList, requiredStaffCount])

    return {
        candidates,
        isLoading,
        isSyncing: syncMutation.isPending,
        assignedList,
        availableList,
        hasChanges,
        keyword,
        setKeyword,
        handleToggle,
        handleReset,
        handleQuickSuggest,
        syncAssignments: () => syncMutation.mutate(effectiveSelectedIds)
    }
}
