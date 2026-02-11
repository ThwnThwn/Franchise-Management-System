import React from "react"
import { RoleSlug } from "@/constants/enums"
import {
    LayoutDashboard,
    CalendarCog,
    CalendarRange,
    Coffee,
    Users,
    ShieldUser,
    Package,
    Settings,
    Calendar
} from "lucide-react"

export interface MenuItem {
    icon: React.ReactNode
    label: string
    href: string
    allowedRoles: RoleSlug[]
}

export const SIDEBAR_MENU: MenuItem[] = [
    {
        icon: <LayoutDashboard className="w-5 h-5" />,
        label: "Tổng quan",
        href: "/dashboard",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
    },
    {
        icon: <CalendarCog className="w-5 h-5" />,
        label: "Cấu hình ca làm",
        href: "/dashboard/shift-settings",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
    },
    {
        icon: <Coffee className="w-5 h-5" />,
        label: "Sản phẩm",
        href: "/dashboard/products",
        allowedRoles: [RoleSlug.Admin]
    },
    {
        icon: <Users className="w-5 h-5" />,
        label: "Người dùng",
        href: "/dashboard/users",
        allowedRoles: [RoleSlug.Admin]
    },
    {
        icon: <ShieldUser className="w-5 h-5" />,
        label: "Vai trò",
        href: "/dashboard/roles",
        allowedRoles: [RoleSlug.Admin]
    },
    {
        icon: <Package className="w-5 h-5" />,
        label: "Kho",
        href: "/dashboard/inventory",
        allowedRoles: [RoleSlug.Admin]
    },
    {
        icon: <Settings className="w-5 h-5" />,
        label: "Cài đặt",
        href: "/dashboard/settings",
        allowedRoles: [RoleSlug.Admin]
    },
    {
        icon: <CalendarRange className="w-5 h-5" />,
        label: "Quản lý ca làm",
        href: "/dashboard/shifts",
        allowedRoles: [RoleSlug.Manager]
    },
    {
        icon: <Calendar className="w-5 h-5" />,
        label: "Lịch cá nhân",
        href: "/dashboard/my-schedule",
        allowedRoles: [RoleSlug.Staff]
    }
]
