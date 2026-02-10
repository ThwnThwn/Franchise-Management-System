"use client"

import React, { useState, useMemo } from "react"
import { Card } from "@heroui/react"
import { motion, AnimatePresence } from "framer-motion"
import {
    LayoutDashboard,
    Coffee,
    Users,
    Package,
    Settings,
    Menu,
    X,
    ShieldUser,
    FileClock,
    CalendarRange
} from "lucide-react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { RoleSlugLabels } from "@/constants/labels"
import { LogoStyled } from "@/components"

// Định nghĩa RoleSlug để tránh magic string
export enum RoleSlug {
    Admin = "admin",
    Manager = "manager",
    Staff = "staff"
}

interface MenuItem {
    icon: React.ReactNode
    label: string
    href: string
    allowedRoles: string[] // Thêm trường này để phân quyền
}

export const DashboardSidebar = () => {
    const [isOpen, setIsOpen] = useState(false)
    const pathname = usePathname()

    // Lấy role từ store
    const roleSlug = useTokenStore((s) => s.roleSlug) || RoleSlug.Staff

    // Cấu hình Menu tập trung
    const menuItems: MenuItem[] = useMemo(
        () => [
            {
                icon: <LayoutDashboard className="w-5 h-5" />,
                label: "Tổng quan",
                href: "/dashboard",
                // Cả Admin và Manager đều nên thấy Dashboard
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                icon: <FileClock className="w-5 h-5" />,
                label: "Quản lý mẫu ca làm",
                href: "/dashboard/shift-templates",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                icon: <CalendarRange className="w-5 h-5" />,
                label: "Quản lý ca làm",
                href: "/dashboard/shifts",
                allowedRoles: [RoleSlug.Manager]
            },
            {
                icon: <Coffee className="w-5 h-5" />,
                label: "Quản lý sản phẩm",
                href: "/dashboard/products",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                icon: <Users className="w-5 h-5" />,
                label: "Quản lý người dùng",
                href: "/dashboard/users",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                icon: <ShieldUser className="w-5 h-5" />,
                label: "Quản lý vai trò",
                href: "/dashboard/roles",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                icon: <Package className="w-5 h-5" />,
                label: "Quản lý kho",
                href: "/dashboard/inventory",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                icon: <Settings className="w-5 h-5" />,
                label: "Cài đặt",
                href: "/dashboard/settings",
                allowedRoles: [RoleSlug.Admin]
            }
        ],
        []
    )

    // Filter menu items dựa trên role hiện tại
    const filteredMenuItems = useMemo(() => {
        return menuItems.filter(
            (item) =>
                // Nếu roleSlug chưa load xong (null/undefined) thì ẩn hoặc hiện default
                roleSlug && item.allowedRoles.includes(roleSlug)
        )
    }, [roleSlug, menuItems])

    return (
        <>
            {/* Mobile Menu Button */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="fixed top-4 left-4 z-50 lg:hidden p-2 rounded-lg bg-white/90 backdrop-blur-md border border-primary-100 shadow-lg"
            >
                {isOpen ? (
                    <X className="w-6 h-6 text-primary" />
                ) : (
                    <Menu className="w-6 h-6 text-primary" />
                )}
            </button>

            {/* Overlay for mobile */}
            <AnimatePresence>
                {isOpen && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        onClick={() => setIsOpen(false)}
                        className="fixed inset-0 bg-black/50 z-40 lg:hidden"
                    />
                )}
            </AnimatePresence>

            {/* Sidebar Container */}
            <div
                className={`
                fixed lg:static inset-y-0 left-0 z-40
                w-72 bg-background border-r border-divider
                transform transition-transform duration-300 ease-in-out
                ${isOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0"}
                flex flex-col h-full p-6
            `}
            >
                {/* Logo */}
                <div className="flex items-center gap-3 mb-8 pt-2">
                    <div className="w-10 h-10 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                        <LogoStyled className="w-10 h-10" />
                    </div>
                    <div>
                        <h1 className="text-xl font-bold text-primary">Coffee Shop</h1>
                        <p className="text-xs text-foreground/60">Management</p>
                    </div>
                </div>

                {/* Navigation Menu */}
                <nav className="flex-1 space-y-2 overflow-y-auto custom-scrollbar">
                    {filteredMenuItems.map((item, index) => {
                        const isActive =
                            item.href === "/dashboard"
                                ? pathname === item.href
                                : pathname.startsWith(item.href)

                        return (
                            <Link key={item.href} href={item.href} onClick={() => setIsOpen(false)}>
                                <motion.div
                                    initial={{ opacity: 0, x: -20 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    transition={{ duration: 0.3, delay: index * 0.05 }}
                                    className={`
                                      flex items-center gap-3 px-4 py-3 rounded-xl
                                      transition-all duration-300 group
                                      ${
                                          isActive
                                              ? "bg-linear-to-r from-primary to-primary/90 text-white shadow-lg"
                                              : "text-foreground/70 hover:bg-primary-50 hover:text-primary"
                                      }
                                    `}
                                >
                                    <span
                                        className={`
                                      transition-transform duration-300
                                      ${isActive ? "" : "group-hover:scale-110"}
                                    `}
                                    >
                                        {item.icon}
                                    </span>
                                    <span className="font-medium text-sm">{item.label}</span>
                                    {isActive && (
                                        <motion.div
                                            layoutId="activeIndicator"
                                            className="ml-auto w-2 h-2 rounded-full bg-white"
                                        />
                                    )}
                                </motion.div>
                            </Link>
                        )
                    })}
                </nav>

                {/* User Profile */}
                <Card className="mt-auto p-4 bg-linear-to-br from-primary-50 to-primary-100/50 border-none">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center text-white font-bold uppercase">
                            {roleSlug?.charAt(0) || "U"}
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold text-foreground truncate capitalize">
                                {roleSlug || "User"}
                            </p>
                            <p className="text-xs text-foreground/60">{RoleSlugLabels[roleSlug]}</p>
                        </div>
                    </div>
                </Card>
            </div>
        </>
    )
}
