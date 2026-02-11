"use client"

import React, { useState, useMemo } from "react"
import { createPortal } from "react-dom" // Import Portal
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
import { RoleSlug } from "@/constants/enums"

interface MenuItem {
    icon: React.ReactNode
    label: string
    href: string
    allowedRoles: string[]
}

export const DashboardSidebar = () => {
    const [isOpen, setIsOpen] = useState(false)
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || RoleSlug.Staff

    const menuItems: MenuItem[] = useMemo(
        () => [
            {
                icon: <LayoutDashboard className="w-5 h-5" />,
                label: "Tổng quan",
                href: "/dashboard",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                icon: <FileClock className="w-5 h-5" />,
                label: "Cấu hình ca làm",
                href: "/dashboard/shift-settings",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                icon: <CalendarRange className="w-5 h-5" />,
                label: "Ca làm",
                href: "/dashboard/shifts",
                allowedRoles: [RoleSlug.Manager]
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
            }
        ],
        []
    )

    const filteredMenuItems = useMemo(
        () => menuItems.filter((item) => roleSlug && item.allowedRoles.includes(roleSlug)),
        [roleSlug, menuItems]
    )

    // Component nội dung Sidebar để dùng chung
    const renderSidebarInner = (isMobile: boolean) => (
        <div className="flex flex-col h-full relative">
            <div className="p-6 flex-1 flex flex-col min-h-0">
                <div className="flex items-center justify-between mb-8 pt-2">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                            <LogoStyled className="w-10 h-10" />
                        </div>
                        <div>
                            <h1 className="text-xl font-bold text-primary">Coffee Shop</h1>
                            <p className="text-xs text-foreground/60">Management</p>
                        </div>
                    </div>
                    {isMobile && (
                        <button onClick={() => setIsOpen(false)} className="p-1 text-primary">
                            <X size={24} />
                        </button>
                    )}
                </div>

                <nav className="flex-1 space-y-2 overflow-y-auto custom-scrollbar pr-2">
                    {filteredMenuItems.map((item, index) => {
                        const isActive =
                            item.href === "/dashboard"
                                ? pathname === item.href
                                : pathname.startsWith(item.href)
                        return (
                            <Link key={item.href} href={item.href} onClick={() => setIsOpen(false)}>
                                <motion.div
                                    initial={isMobile ? { opacity: 0, x: -20 } : false}
                                    animate={isMobile ? { opacity: 1, x: 0 } : false}
                                    transition={{ duration: 0.3, delay: index * 0.05 }}
                                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300 group ${
                                        isActive
                                            ? "bg-linear-to-r from-primary to-primary/90 text-white shadow-lg"
                                            : "text-foreground/70 hover:bg-primary-50 hover:text-primary"
                                    }`}
                                >
                                    <span className={isActive ? "" : "group-hover:scale-110"}>
                                        {item.icon}
                                    </span>
                                    <span className="font-medium text-sm">{item.label}</span>
                                    {isActive && (
                                        <motion.div
                                            layoutId={isMobile ? "activeMobile" : "activeDesktop"}
                                            className="ml-auto w-2 h-2 rounded-full bg-white"
                                        />
                                    )}
                                </motion.div>
                            </Link>
                        )
                    })}
                </nav>
            </div>

            {/* User Profile Sticky Bottom */}
            <div className="mt-auto sticky bottom-0 p-6 bg-background/80 backdrop-blur-md border-t border-divider">
                <Card className="p-4 bg-linear-to-br from-primary-50 to-primary-100/50 border-none shadow-none">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center text-white font-bold uppercase shadow-sm">
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
        </div>
    )

    return (
        <>
            {/* 1. DESKTOP VIEW: Nằm trong layout bình thường */}
            <aside className="hidden lg:flex w-72 bg-background h-full border-r border-divider flex-col">
                {renderSidebarInner(false)}
            </aside>

            {/* 2. MOBILE VIEW: Dùng Portal để bay ra khỏi max-w-7xl của cha */}
            {!isOpen && (
                <button
                    onClick={() => setIsOpen(true)}
                    className="fixed top-4 left-4 z-40 lg:hidden p-2.5 rounded-xl bg-white shadow-xl border border-primary-100"
                >
                    <Menu className="w-6 h-6 text-primary" />
                </button>
            )}

            {typeof document !== "undefined" &&
                createPortal(
                    <AnimatePresence>
                        {isOpen && (
                            <>
                                {/* Overlay che phủ toàn bộ body */}
                                <motion.div
                                    initial={{ opacity: 0 }}
                                    animate={{ opacity: 1 }}
                                    exit={{ opacity: 0 }}
                                    onClick={() => setIsOpen(false)}
                                    className="fixed inset-0 bg-black/60 backdrop-blur-sm z-9998 lg:hidden"
                                />

                                {/* Sidebar trượt ra che phủ toàn bộ body */}
                                <motion.div
                                    initial={{ x: "-100%" }}
                                    animate={{ x: 0 }}
                                    exit={{ x: "-100%" }}
                                    transition={{ type: "spring", damping: 25, stiffness: 200 }}
                                    className="fixed inset-y-0 left-0 w-72 bg-background z-9999 lg:hidden shadow-2xl flex flex-col"
                                >
                                    {renderSidebarInner(true)}
                                </motion.div>
                            </>
                        )}
                    </AnimatePresence>,
                    document.body
                )}
        </>
    )
}
