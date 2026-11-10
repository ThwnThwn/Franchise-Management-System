import React from "react"
import { RoleSlug } from "@/constants/enums"

export interface MenuItem {
    label: string
    href?: string
    icon?: React.ReactNode
    allowedRoles: RoleSlug[]
    subItems?: MenuItem[]
}

export type ProfileMenuItem = {
    key: string
    label: string
    href?: string
    icon?: React.ReactNode
    color?: "default" | "danger"
    allowedRoles?: RoleSlug[] // Nếu không có field này nghĩa là hiện cho all
    action?: () => void // Dành cho nút Logout
}
