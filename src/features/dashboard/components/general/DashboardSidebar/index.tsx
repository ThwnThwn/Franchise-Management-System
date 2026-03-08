"use client"

import React, { useState, useMemo } from "react"
import { createPortal } from "react-dom"
import { motion, AnimatePresence } from "framer-motion"
import { Menu } from "lucide-react"
import { usePathname } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { DASHBOARD_SIDEBAR_MENU, MenuItem } from "@/constants/menu/sidebar"
import { SidebarContent } from "./SidebarContent"

export const DashboardSidebar = () => {
    const [isOpen, setIsOpen] = useState(false)
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || RoleSlug.Staff

    // Trong file DashboardSidebar.tsx của ní
    const filteredMenuItems = useMemo(() => {
        if (!roleSlug) return []

        return DASHBOARD_SIDEBAR_MENU.reduce((acc, item) => {
            // 1. Kiểm tra quyền thằng cha
            if (item.allowedRoles.includes(roleSlug)) {
                // 2. Nếu có menu con, lọc tiếp menu con
                if (item.subItems) {
                    const filteredSubs = item.subItems.filter((sub) =>
                        sub.allowedRoles.includes(roleSlug)
                    )
                    // Nếu sau khi lọc mà menu con vẫn còn, thì push thằng cha (kèm menu con đã lọc) vào mảng
                    if (filteredSubs.length > 0) {
                        acc.push({ ...item, subItems: filteredSubs })
                    }
                } else {
                    // 3. Không có menu con (Menu đơn) thì push thẳng
                    acc.push(item)
                }
            }
            return acc
        }, [] as MenuItem[])
    }, [roleSlug])

    const commonProps = {
        items: filteredMenuItems,
        pathname,
        roleSlug,
        onClose: () => setIsOpen(false)
    }

    return (
        <>
            {/* 1. DESKTOP VIEW: Nằm trong layout bình thường */}
            <aside className="hidden lg:flex w-80 bg-background h-full border-r border-divider flex-col">
                <SidebarContent {...commonProps} isMobile={false} />
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
                                    <SidebarContent {...commonProps} isMobile={true} />
                                </motion.div>
                            </>
                        )}
                    </AnimatePresence>,
                    document.body
                )}
        </>
    )
}
