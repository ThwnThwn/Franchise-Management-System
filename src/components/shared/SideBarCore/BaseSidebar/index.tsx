"use client"

import React, { useMemo } from "react"
import { motion } from "framer-motion"
import { X } from "lucide-react"
import { SidebarItem } from "../SidebarItem"
import { RoleSlug } from "@/constants/enums"
import { MenuItem } from "@/constants/menu/types"

// Đổi tên props cho thật Generic
interface BaseSidebarProps {
    isMobile: boolean
    pathname: string
    items: MenuItem[]
    onClose: () => void

    userRole?: RoleSlug

    header?: React.ReactNode
    footer?: React.ReactNode
}

export const BaseSidebar = ({
    isMobile,
    pathname,
    items,
    onClose,
    userRole,
    header,
    footer
}: BaseSidebarProps) => {
    const filteredItems = useMemo(() => {
        // Hàm đệ quy lọc menu
        const filterMenu = (menuList: MenuItem[]): MenuItem[] => {
            return menuList.reduce((acc, item) => {
                // Rule: Mảng rỗng hoặc undefined -> Allow hết. Nếu có mảng -> Phải chứa Role
                const isAllowed =
                    !item.allowedRoles ||
                    item.allowedRoles.length === 0 ||
                    (userRole && item.allowedRoles.includes(userRole))

                if (isAllowed) {
                    if (item.subItems && item.subItems.length > 0) {
                        // Lọc đệ quy menu con
                        const filteredSubs = filterMenu(item.subItems)

                        // Nếu lọc xong menu con vẫn còn item, hoặc thằng cha có href độc lập
                        if (filteredSubs.length > 0) {
                            acc.push({ ...item, subItems: filteredSubs })
                        } else if (item.href) {
                            // Cứu vớt: Sub rỗng nhưng cha có href thì biến thành menu đơn
                            acc.push({ ...item, subItems: undefined })
                        }
                    } else {
                        // Menu đơn (không có subItems)
                        acc.push(item)
                    }
                }
                return acc
            }, [] as MenuItem[])
        }

        // Nếu không truyền userRole vào Component, tự động show hết toàn bộ menu gốc
        if (!userRole) return items

        return filterMenu(items)
    }, [items, userRole])

    // --- LOGIC TÌM ACTIVE HREF (Universal) ---
    // Cái này Generic nên để trong đây luôn, xài cho menu nào cũng đúng
    const activeHref = useMemo(() => {
        const allHrefs: string[] = []

        const extractHrefs = (menuItems: MenuItem[]) => {
            menuItems.forEach((item) => {
                if (item.href) allHrefs.push(item.href)
                if (item.subItems) extractHrefs(item.subItems)
            })
        }
        extractHrefs(filteredItems)

        const matchedHrefs = allHrefs.filter((href) => {
            // Khớp chính xác 100% HOẶC là thư mục cha (có dấu / phía sau để tránh trùng từ)
            return pathname === href || pathname.startsWith(`${href}/`)
        })

        matchedHrefs.sort((a, b) => b.length - a.length)
        return matchedHrefs[0] || ""
    }, [pathname, filteredItems])

    return (
        <div className="flex flex-col h-full relative">
            <div className="p-6 flex-1 flex flex-col min-h-0">
                {/* --- PHẦN HEADER ĐỘNG --- */}
                <div className="flex items-center justify-between mb-4 pt-2">
                    {/* Render bất cứ thứ gì cha truyền vào đây (Logo, Tên quán, Icon Ví...) */}
                    <div className="flex-1">{header}</div>

                    {/* Nút X đóng menu Mobile */}
                    {isMobile && (
                        <button
                            onClick={onClose}
                            className="p-1 ml-2 text-primary hover:bg-primary-50 rounded-lg transition-colors shrink-0"
                        >
                            <X size={24} />
                        </button>
                    )}
                </div>

                {/* --- PHẦN RENDER MENU CHÍNH --- */}
                <nav className="flex-1 space-y-2 overflow-y-auto custom-scrollbar pr-2 mt-2">
                    {filteredItems.map((item, index) => (
                        <motion.div
                            key={item.label}
                            initial={isMobile ? { opacity: 0, x: -20 } : false}
                            animate={isMobile ? { opacity: 1, x: 0 } : false}
                            transition={{ duration: 0.3, delay: index * 0.05 }}
                        >
                            <SidebarItem
                                item={item}
                                activeHref={activeHref}
                                isMobile={isMobile}
                                onClose={onClose}
                            />
                        </motion.div>
                    ))}
                </nav>
            </div>

            {/* --- PHẦN FOOTER ĐỘNG (Tuỳ chọn) --- */}
            {footer && (
                <div className="mt-auto sticky bottom-0 p-6 bg-background/80 backdrop-blur-md border-t border-divider">
                    {footer}
                </div>
            )}
        </div>
    )
}
