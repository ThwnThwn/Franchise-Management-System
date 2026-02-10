import RolePermissionPageView from "@/features/role-permissions/pages/RolePermissionPageView"
import React from "react"

export default async function RolePermissionPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params

    const roleId = id

    return <RolePermissionPageView roleId={roleId} />
}
