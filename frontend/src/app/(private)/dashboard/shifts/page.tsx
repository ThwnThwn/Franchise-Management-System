"use client"

import { RoleSlug } from "@/constants/enums"
import ShiftSchedulerPageView from "@/features/shifts/pages/ShiftSchedulerPageView"
import StaffAgendaPageView from "@/features/shifts/pages/StaffAgendaPageView"
import { useTokenStore } from "@/hooks"
import React from "react"

export default function ShiftSchedulerPage() {
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isManager = roleSlug === RoleSlug.Manager

    return isManager ? <ShiftSchedulerPageView /> : <StaffAgendaPageView />
}
