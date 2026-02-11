"use client"

import React from "react"
import { Listbox, ListboxItem, User, Tooltip, ScrollShadow, Spinner } from "@heroui/react"
import { AlertTriangle, CheckCircle2, UserPlus, UserMinus, Info } from "lucide-react"
import { useShiftAssignment } from "../../hooks/useShiftAssignment"
import { UnsavedChangesNotifier } from "@/components"
import { getValidationErrors } from "./helper"
import { useUserHelper } from "@/hooks"

type Props = {
    storeId: string
    shiftId: string
    requiredStaffCount: number
    isSubmitable: boolean
    onClose?: () => void
}

export function ShiftAssignmentTab({
    storeId,
    shiftId,
    requiredStaffCount,
    isSubmitable,
    onClose
}: Props) {
    const { toFullName } = useUserHelper()

    const {
        isLoading,
        syncAssignments,
        isSyncing,
        hasChanges,
        effectiveSelectedIds,
        assignedList,
        availableList,
        handleToggle,
        handleReset
    } = useShiftAssignment({
        shiftId: shiftId,
        storeId: storeId,
        onSuccess: onClose
    })

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
                                    isReadOnly={!isSubmitable}
                                >
                                    <User
                                        name={toFullName({
                                            firstName: item.firstName,
                                            lastName: item.lastName
                                        })}
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
                        {(item) => {
                            const errors = getValidationErrors(item)
                            const isHardBlocked =
                                item.isConflict || item.validation?.isShuttleInvalid

                            return (
                                <ListboxItem
                                    key={item.id}
                                    textValue={`${item.firstName} ${item.lastName}`}
                                    onPress={() => handleToggle(item.id)}
                                    className={isHardBlocked ? "opacity-50 cursor-not-allowed" : ""}
                                    isReadOnly={!isSubmitable || isHardBlocked}
                                    endContent={
                                        errors.length > 0 && (
                                            <Tooltip
                                                placement="right"
                                                closeDelay={200}
                                                content={
                                                    <div className="px-1 py-2">
                                                        <div className="text-tiny font-bold mb-1">
                                                            Cảnh báo:
                                                        </div>
                                                        {errors.map((err, i) => (
                                                            <div key={i} className="text-tiny">
                                                                • {err}
                                                            </div>
                                                        ))}
                                                    </div>
                                                }
                                                color="danger"
                                            >
                                                <AlertTriangle
                                                    size={18}
                                                    className={
                                                        isHardBlocked
                                                            ? "text-danger"
                                                            : "text-warning"
                                                    }
                                                />
                                            </Tooltip>
                                        )
                                    }
                                >
                                    <div className="flex flex-col items-start gap-1">
                                        <User
                                            name={toFullName({
                                                firstName: item.firstName,
                                                lastName: item.lastName
                                            })}
                                            description={`${item.currentHourlyWage.toLocaleString()}đ/h`}
                                            avatarProps={{ src: item.avatarUrl, size: "sm" }}
                                        />
                                        {/* Hiển thị note bận nếu có */}
                                        {!item.isAvailable && (
                                            <div className="flex items-center gap-1 text-[10px] text-warning italic pl-10">
                                                <Info size={10} />
                                                Lịch bận: {item.note || "Nghỉ"}
                                            </div>
                                        )}
                                    </div>
                                </ListboxItem>
                            )
                        }}
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
