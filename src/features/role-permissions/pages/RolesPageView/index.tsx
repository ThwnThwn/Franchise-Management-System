"use client"

import React, { useState } from "react"
import { useGetAllRoles, useGetAllPermissions } from "../../hooks"
import { Accordion, AccordionItem, Divider, useDisclosure } from "@heroui/react"
import { ChevronRight, Plus, ShieldCheck, ShieldUser } from "lucide-react"
import { RoleResponse } from "../../models/respone"
import { PermissionRow, RoleCard, RoleFormModal } from "../../components"
import { ButtonStyled } from "@/components"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"

export default function RolesPageView() {
    const { data: roles } = useGetAllRoles({})
    const { data: permissions } = useGetAllPermissions({})
    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const [selectedRole, setSelectedRole] = useState<RoleResponse | null>(null)

    // const allGroupKeys = useMemo(() => {
    //     return permissions?.map((g) => g.resource) || []
    // }, [permissions])

    const handleEditRole = (role: RoleResponse) => {
        setSelectedRole(role)
        onOpen()
    }

    const handleCreateRole = () => {
        setSelectedRole(null)
        onOpen()
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            {/* PHẦN 1: DANH SÁCH ROLE (CARDS) */}
            <section className="flex flex-col gap-2">
                <div className="flex justify-between">
                    <DashboardHeader
                        icon={<ShieldUser className="w-6 h-6 text-white" />}
                        title="Vai trò hệ thống"
                        description="Quản lý các nhóm quyền hạn chính trong hệ thống"
                    />

                    <ButtonStyled
                        color="primary"
                        startContent={<Plus size={18} />}
                        onPress={handleCreateRole}
                        className="font-bold max-w-fit"
                    >
                        Tạo vai trò mới
                    </ButtonStyled>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {roles?.map((role) => (
                        <button
                            key={role.id}
                            onClick={() => handleEditRole(role)}
                            className="min-h-fit interactive-scale"
                        >
                            <RoleCard key={role.id} role={role} />
                        </button>
                    ))}
                </div>
            </section>

            <Divider className="my-7" />

            {/* PHẦN 2: DANH SÁCH TẤT CẢ QUYỀN (VIEW ONLY) */}
            <section className="flex flex-col gap-3">
                <div className="flex items-center gap-3 text-slate-800">
                    <ShieldCheck size={24} className="text-primary" />
                    <h2 className="text-xl font-bold">Danh mục quyền hạn hệ thống</h2>
                </div>

                {(permissions?.length ?? 0) > 0 && (
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
                        {(permissions ?? []).map((group) => (
                            <AccordionItem
                                key={group.resource}
                                aria-label={group.resource}
                                title={`${group.resource} (${group.permissions.length})`}
                                startContent={
                                    <div className="p-2 bg-primary-50 rounded-lg text-primary">
                                        <ChevronRight size={18} />
                                    </div>
                                }
                            >
                                <div className="flex flex-col gap-1">
                                    {group.permissions.map((p) => (
                                        <PermissionRow key={p.id} permission={p} canEdit />
                                    ))}
                                </div>
                            </AccordionItem>
                        ))}
                    </Accordion>
                )}
            </section>

            {/* Modal dùng chung */}
            <RoleFormModal isOpen={isOpen} onOpenChange={onOpenChange} role={selectedRole} />
        </div>
    )
}
