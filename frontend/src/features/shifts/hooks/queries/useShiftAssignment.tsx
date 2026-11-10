import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import { addToast } from "@heroui/react"
import { StaffAssignmentCandidate } from "@/features/shifts/models/response"
import { BackendError } from "@/models/common/respone"
import { shiftApi } from "@/features/shifts/services"
import { STRINGS } from "@/constants/strings"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { useCallback, useMemo, useState } from "react"
import { promptDialog } from "@/hooks"

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

    // ===============================
    // Query and Mutation
    // ===============================
    const { data: candidates = [], isLoading } = useQuery<StaffAssignmentCandidate[]>({
        queryKey,
        queryFn: () => shiftApi.getAvailableStaffs({ storeId, shiftId }),
        enabled: !!storeId && !!shiftId
    })

    const syncMutation = useMutation({
        mutationFn: async (userIds: string[]) => {
            const res = await shiftApi.updateAssignments({
                shiftId,
                req: {
                    userIds: userIds,
                    forceOverride: false,
                    overrideReason: ""
                }
            })

            if (!res.warnings || res.warnings.length == 0) return res

            // 1. Gom nhóm warnings theo code
            const groupedWarnings = res.warnings.reduce(
                (acc, warning) => {
                    if (!acc[warning.code]) {
                        acc[warning.code] = []
                    }
                    acc[warning.code].push(warning.message)
                    return acc
                },
                {} as Record<string, string[]>
            )

            // 2. Render UI đã được gom nhóm
            const warningMessage = (
                <div className="flex flex-col gap-3 text-left max-h-[60vh] overflow-y-auto pr-2">
                    <p className="text-sm">Hệ thống phát hiện các vi phạm quy tắc xếp ca sau:</p>

                    <div className="flex flex-col gap-3">
                        {Object.entries(groupedWarnings).map(([code, messages]) => (
                            <div
                                key={code}
                                className="bg-warning-50 p-3 rounded-xl border border-warning-100"
                            >
                                {/* Header của từng nhóm vi phạm */}
                                <p className="font-bold text-xs text-warning-700 mb-1.5 uppercase tracking-wider">
                                    {code}
                                </p>

                                {/* Danh sách các nhân viên/chi tiết vi phạm thuộc nhóm đó */}
                                <ul className="list-disc pl-5 text-warning-600 font-medium text-sm space-y-1">
                                    {messages.map((msg, idx) => (
                                        <li key={idx}>{msg}</li>
                                    ))}
                                </ul>
                            </div>
                        ))}
                    </div>

                    <p className="mt-2 font-bold text-default-700 text-sm">
                        Bạn có chắc chắn muốn bỏ qua và ghi đè?
                    </p>
                </div>
            )

            const reasonOrCancel = await promptDialog({
                title: "Cảnh báo vi phạm quy tắc",
                message: warningMessage,
                variant: "warning",
                confirmText: "Đồng ý ghi đè",
                inputPlaceholder: "Vui lòng nhập lý do ghi đè (Bắt buộc)..."
            })

            if (reasonOrCancel === false) {
                return Promise.reject(new Error("USER_CANCELLED")) // Ném lỗi ngầm để tắt mutation
            }

            return await shiftApi.updateAssignments({
                shiftId,
                req: {
                    userIds: userIds,
                    forceOverride: true, // Kích hoạt ghi đè
                    overrideReason: reasonOrCancel // Ném lý do xuống Backend
                }
            })
        },
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
        onError: (error: any) => {
            // Bẫy lỗi ngầm: Nếu là do User bấm Hủy ở Modal thì bỏ qua, không Toast đỏ
            if (error.message === "USER_CANCELLED") return

            // Các lỗi 400, 500 khác từ Backend thì Toast đỏ bình thường
            const beError = error as BackendError
            addToast({
                title: beError.title || STRINGS.COMMON.ERROR,
                description: beError.detail,
                color: "danger"
            })
        }
    })

    // ===============================
    // Logic xử lý danh sách
    // ===============================
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
