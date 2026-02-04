"use client"

import React, { useState, useMemo } from "react"
import {
    useGetAllPermissions,
    useGetAllRoles,
    useGetPermissionsByRoleId,
    useUpdateRolePermissions
} from "../../hooks"
import { PermissionRow } from "../../components/PermissionRow"
import { ButtonIconStyled, InputStyled } from "@/components"
import { Search, FilterX, ChevronRight, ShieldUser, RotateCcw, Save } from "lucide-react"
import { Accordion, AccordionItem, Spinner, Switch } from "@heroui/react"
import { createPortal } from "react-dom"
import { motion, AnimatePresence } from "framer-motion"

export default function RolePermissionPageView({ roleId }: { roleId: string }) {
    const { data: roles } = useGetAllRoles({})
    const role = roles?.filter((r) => r.id === roleId)[0]

    const { data: allPermissions, isLoading: isLoadingAll } = useGetAllPermissions({}) //
    const { data: rolePermissions, isLoading: isLoadingRole } = useGetPermissionsByRoleId({
        id: roleId
    }) //
    const updateMutation = useUpdateRolePermissions({ id: roleId }) //

    const [searchQuery, setSearchQuery] = useState("")
    const [localPermissions, setLocalPermissions] = useState<Record<string, boolean>>({})

    // Logic Filter theo Search (giống bản cũ)
    const mergedGroups = useMemo(() => {
        if (!allPermissions) return []

        // Lưu các slug đang Active của role vào một Set để tìm kiếm nhanh O(1)
        const activeSlugs = new Set(
            rolePermissions?.flatMap((group) =>
                group.permissions.filter((p) => p.isActived).map((p) => p.slug)
            ) || []
        )

        return allPermissions.map((group) => ({
            ...group,
            permissions: group.permissions.map((p) => ({
                ...p,
                // Trạng thái gốc từ Server (dựa trên slug)
                isActived: activeSlugs.has(p.slug)
            }))
        }))
    }, [allPermissions, rolePermissions])

    const filteredGroups = useMemo(() => {
        return mergedGroups
            .map((group) => ({
                ...group,
                permissions: group.permissions.filter(
                    (p) =>
                        p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                        p.slug.toLowerCase().includes(searchQuery.toLowerCase())
                )
            }))
            .filter((group) => group.permissions.length > 0)
    }, [mergedGroups, searchQuery])

    // 2. Tính toán Grant/Revoke dựa trên SLUG
    const changes = useMemo(() => {
        const toGrant: string[] = []
        const toRevoke: string[] = []

        mergedGroups.forEach((group) => {
            group.permissions.forEach((p) => {
                // Nếu slug này có trong thay đổi tạm thời (local state)
                if (p.slug in localPermissions) {
                    const newValue = localPermissions[p.slug]
                    if (newValue !== p.isActived) {
                        newValue ? toGrant.push(p.slug) : toRevoke.push(p.slug)
                    }
                }
            })
        })
        return { toGrant, toRevoke }
    }, [localPermissions, mergedGroups])

    // Lấy danh sách keys để mặc định mở hết Accordion
    // const allGroupKeys = useMemo(() => {
    //     return filteredGroups.map((g) => g.resource)
    // }, [filteredGroups])

    // Chắc chắn tham số đầu tiên là slug
    const handleSwitch = (slug: string, checked: boolean) => {
        setLocalPermissions((prev) => ({ ...prev, [slug]: checked }))
    }

    const handleSave = async () => {
        await updateMutation.mutateAsync({
            permissionsToGrant: changes.toGrant,
            permissionsToRevoke: changes.toRevoke
        })
        setLocalPermissions({})
    }

    const hasChanges = changes.toGrant.length > 0 || changes.toRevoke.length > 0

    const handleToggleGroup = (groupPermissions: any[]) => {
        // 1. Kiểm tra trạng thái dựa trên slug
        const isAllActive = groupPermissions.every((p) =>
            p.slug in localPermissions ? localPermissions[p.slug] : p.isActived
        )

        const newState = { ...localPermissions }
        groupPermissions.forEach((p) => {
            // 2. Sửa p.id thành p.slug ở đây
            newState[p.slug] = !isAllActive
        })
        setLocalPermissions(newState)
    }

    if (isLoadingAll || isLoadingRole || !role)
        return (
            <div className="text-center">
                <Spinner />
            </div>
        )

    return (
        <div className="container mx-auto py-6 px-4 pb-32">
            <div className="mb-8">
                <div className="flex items-center gap-3 mb-2">
                    <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                        <ShieldUser className="w-6 h-6 text-white" />
                    </div>
                    <div>
                        <div>
                            <h1 className="text-2xl font-black text-slate-800">{role.name}</h1>
                            <p className="text-slate-500 text-sm">
                                {role.description || "Vai trò này chưa có mô tả"}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
            {/* SEARCH TOOLBAR */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="w-full sm:max-w-md">
                    <InputStyled
                        isClearable
                        fullWidth
                        size="lg"
                        variant="bordered"
                        placeholder="Tìm nhanh quyền hạn theo tên hoặc slug..."
                        startContent={<Search size={20} className="text-slate-400" />}
                        value={searchQuery}
                        onValueChange={setSearchQuery}
                    />
                </div>
            </div>

            {/* ACCORDION LIST */}
            <div className="mt-4">
                {filteredGroups.length > 0 ? (
                    <Accordion
                        selectionMode="multiple"
                        variant="splitted"
                        // defaultExpandedKeys={allGroupKeys}
                        itemClasses={{
                            base: "-mx-2 border-none shadow-sm",
                            title: "font-black text-slate-800 uppercase tracking-tighter text-lg",
                            trigger: "px-6 py-4",
                            content: "px-6 pb-6 pt-2"
                        }}
                        isCompact
                    >
                        {filteredGroups.map((group) => {
                            const isGroupAllActive = group.permissions.every((p) =>
                                p.slug in localPermissions ? localPermissions[p.slug] : p.isActived
                            )

                            return (
                                <AccordionItem
                                    key={group.resource}
                                    aria-label={group.resource}
                                    title={`${group.resource} (${group.permissions.length})`}
                                    // Chèn nút Toggle All vào phần Header của Accordion
                                    startContent={
                                        <div className="p-2 bg-primary-50 rounded-lg text-primary">
                                            <ChevronRight size={18} />
                                        </div>
                                    }
                                >
                                    <div className="flex gap-2 items-center mb-3">
                                        <Switch
                                            size="sm"
                                            color="primary"
                                            isSelected={isGroupAllActive}
                                            onValueChange={() =>
                                                handleToggleGroup(group.permissions)
                                            }
                                            aria-label={`Toggle all permission`}
                                        >
                                            {isGroupAllActive ? "Bỏ chọn hết" : "Chọn hết nhóm"}
                                        </Switch>
                                    </div>
                                    <div className="flex flex-col gap-1">
                                        {group.permissions.map((p) => (
                                            <PermissionRow
                                                key={p.id}
                                                permission={p}
                                                isSelected={
                                                    p.slug in localPermissions
                                                        ? localPermissions[p.slug]
                                                        : p.isActived
                                                }
                                                onChange={handleSwitch}
                                            />
                                        ))}
                                    </div>
                                </AccordionItem>
                            )
                        })}
                    </Accordion>
                ) : (
                    <div className="flex flex-col items-center justify-center py-20 text-slate-400 gap-2">
                        <FilterX size={48} strokeWidth={1} />
                        <p>Không tìm thấy quyền hạn nào.</p>
                    </div>
                )}
            </div>

            {createPortal(
                <AnimatePresence>
                    {hasChanges && (
                        <motion.div
                            initial={{ y: 100, x: "-50%", opacity: 0 }}
                            animate={{ y: 0, x: "-50%", opacity: 1 }}
                            exit={{ y: 100, x: "-50%", opacity: 0 }}
                            transition={{ type: "spring", stiffness: 300, damping: 30 }}
                            className="fixed bottom-8 left-1/2 z-9999 bg-slate-900 border border-slate-700 text-white px-8 py-4 rounded-2xl shadow-2xl flex items-center gap-8 min-w-125"
                        >
                            <div className="flex flex-col">
                                <span className="text-sm font-bold text-success">Cẩn thận!</span>
                                <span className="text-xs text-slate-400 font-medium">
                                    Bạn có thay đổi chưa lưu trên quyền hạn này.
                                </span>
                            </div>

                            <div className="ml-auto flex items-center gap-3">
                                {!updateMutation.isPending && (
                                    <ButtonIconStyled
                                        size="sm"
                                        variant="light"
                                        className="text-white hover:bg-white/10"
                                        onPress={() => setLocalPermissions({})} // Nút khôi phục
                                    >
                                        <RotateCcw size={18} />
                                    </ButtonIconStyled>
                                )}

                                <ButtonIconStyled
                                    size="sm"
                                    color="success"
                                    onPress={handleSave}
                                    isLoading={updateMutation.isPending}
                                >
                                    <Save size={18} />
                                </ButtonIconStyled>
                            </div>
                        </motion.div>
                    )}
                </AnimatePresence>,
                document.body
            )}
        </div>
    )
}
