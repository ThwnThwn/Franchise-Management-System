"use client"

import React, { useState, useMemo } from "react"
import { Listbox, ListboxItem, User, Tooltip, ScrollShadow, Spinner } from "@heroui/react"
import { AlertTriangle, CheckCircle2, UserPlus, UserMinus } from "lucide-react"
import { useShiftAssignment } from "../../hooks/useShiftAssignment"
import { UnsavedChangesNotifier } from "@/components"

type Props = {
    storeId: string
    shiftId: string
    requiredStaffCount: number
    onClose?: () => void
}

export function ShiftAssignmentTab({ storeId, shiftId, requiredStaffCount, onClose }: Props) {
    const { candidates, isLoading, syncAssignments, isSyncing } = useShiftAssignment({
        shiftId: shiftId,
        storeId: storeId,
        onSuccess: onClose
    })

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
        setDraftIds((prev) => {
            const current = prev ?? originalAssignedIds
            return current.includes(userId)
                ? current.filter((id) => id !== userId)
                : [...current, userId]
        })
    }

    const handleReset = () => setDraftIds(null)

    if (isLoading) {
        return (
            <div className="flex h-125 items-center justify-center">
                <Spinner label="Đang tải danh sách nhân viên..." color="primary" />
            </div>
        )
    }

    return (
        <div className="flex flex-col h-125 relative">
            <ScrollShadow className="grow p-2">
                {/* NHÓM ĐÃ GÁN */}
                <section className="mb-6">
                    <h4 className="text-sm font-bold mb-3 flex items-center gap-2">
                        <CheckCircle2 size={16} className="text-success" />
                        Nhân viên đã gán ({assignedList.length}/{requiredStaffCount})
                    </h4>
                    {assignedList.length === 0 ? (
                        <p className="text-xs text-default-400 italic pl-6">
                            Chưa có nhân viên nào
                        </p>
                    ) : (
                        <Listbox items={assignedList} variant="flat" aria-label="Assigned list">
                            {(item) => (
                                <ListboxItem
                                    key={item.id}
                                    textValue={`${item.firstName} ${item.lastName}`}
                                    endContent={<UserMinus className="text-danger" size={18} />}
                                    onPress={() => handleToggle(item.id)}
                                >
                                    <User
                                        name={`${item.firstName} ${item.lastName}`}
                                        description={`${item.currentHourlyWage.toLocaleString()}đ/h`}
                                        avatarProps={{ src: item.avatarUrl, size: "sm" }}
                                    />
                                </ListboxItem>
                            )}
                        </Listbox>
                    )}
                </section>

                {/* NHÓM ỨNG VIÊN */}
                <section>
                    <h4 className="text-sm font-bold mb-3 flex items-center gap-2">
                        <UserPlus size={16} className="text-primary" />
                        Ứng viên khả dụng ({availableList.length})
                    </h4>
                    <Listbox items={availableList} variant="flat" aria-label="Available list">
                        {(item) => (
                            <ListboxItem
                                key={item.id}
                                textValue={`${item.firstName} ${item.lastName}`}
                                endContent={
                                    item.isConflict && (
                                        <Tooltip content="Trùng lịch" color="danger">
                                            <AlertTriangle size={18} className="text-danger" />
                                        </Tooltip>
                                    )
                                }
                                onPress={() => !item.isConflict && handleToggle(item.id)}
                                className={item.isConflict ? "opacity-50 cursor-not-allowed" : ""}
                            >
                                <div className="flex flex-col items-start gap-1">
                                    <User
                                        name={`${item.firstName} ${item.lastName}`}
                                        description={`${item.currentHourlyWage.toLocaleString()}đ/h`}
                                        avatarProps={{ src: item.avatarUrl, size: "sm" }}
                                    />
                                    {!item.isAvailable && (
                                        <span className="text-[10px] text-warning italic pl-10">
                                            Bận: {item.note || "Nghỉ"}
                                        </span>
                                    )}
                                </div>
                            </ListboxItem>
                        )}
                    </Listbox>
                </section>
            </ScrollShadow>

            <UnsavedChangesNotifier
                hasChanges={hasChanges}
                isLoading={isSyncing}
                onSave={() => syncAssignments(effectiveSelectedIds)}
                onReset={handleReset}
            />
        </div>
    )
}
