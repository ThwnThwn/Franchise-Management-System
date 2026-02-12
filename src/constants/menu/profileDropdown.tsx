import React from "react"
import { RoleSlug } from "@/constants/enums"
import { STRINGS } from "../strings"
import { LayoutDashboard, LogOut, ShoppingBag, UserIcon } from "lucide-react"

export type ProfileMenuItem = {
    key: string
    label: string
    href?: string
    icon?: React.ReactNode
    color?: "default" | "danger"
    allowedRoles?: RoleSlug[] // Nếu không có field này nghĩa là hiện cho all
    action?: () => void // Dành cho nút Logout
}

export const PROFILE_DROPDOWN_MENU: ProfileMenuItem[] = [
    {
        key: "profile",
        label: STRINGS.NAVBAR.PROFILE || "Hồ sơ cá nhân",
        href: "/profile",
        icon: <UserIcon size={18} />
    },
    {
        key: "dashboard",
        label: STRINGS.NAVBAR.DASHBOARD.DASHBOARD || "Trang quản trị",
        href: "/dashboard",
        icon: <LayoutDashboard size={18} />,
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager, RoleSlug.Staff]
    },
    {
        key: "orders",
        label: STRINGS.NAVBAR.ORDER_HISTORY || "Lịch sử mua hàng",
        href: "/orders",
        icon: <ShoppingBag size={18} />,
        allowedRoles: [RoleSlug.Customer]
    },
    {
        key: "logout",
        label: STRINGS.NAVBAR.LOGOUT || "Đăng xuất",
        color: "danger",
        icon: <LogOut size={18} />
    }
]
