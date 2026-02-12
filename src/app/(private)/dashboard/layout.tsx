"use client"

import { SpinnerStyled } from "@/components"
import { DashboardSidebar } from "@/features/dashboard/components"
import { DASHBOARD_SIDEBAR_MENU } from "@/constants/menu/sidebar"
import { useTokenStore } from "@/hooks"
import { useRouter, usePathname } from "next/navigation"
import React, { useEffect, useMemo } from "react"

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter()
    const pathname = usePathname()

    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isLoading = roleSlug === undefined

    // 1. Tự động lấy danh sách các Path mà Role hiện tại được phép vào
    const allowedPathsForCurrentRole = useMemo(() => {
        if (!roleSlug) return []
        return DASHBOARD_SIDEBAR_MENU.filter((item) => item.allowedRoles.includes(roleSlug)).map(
            (item) => item.href
        )
    }, [roleSlug])

    // 2. Kiểm tra xem URL hiện tại có hợp lệ với Role này không
    const isAuthorized = useMemo(() => {
        if (isLoading) return false
        // Nếu là root dashboard thì cho qua để useEffect xử lý redirect sau
        if (pathname === "/dashboard") return true
        // Check xem pathname có nằm trong danh sách href được phép không
        return allowedPathsForCurrentRole.some((path) => pathname.startsWith(path))
    }, [isLoading, pathname, allowedPathsForCurrentRole])

    useEffect(() => {
        if (isLoading) return

        // Nếu vào trang chủ Dashboard, tìm trang đầu tiên mà Role này được phép để redirect
        if (pathname === "/dashboard") {
            const firstAllowedPage = allowedPathsForCurrentRole[0] || "/"
            router.replace(firstAllowedPage)
            return
        }

        // Nếu cố tình vào path không có quyền, đá về Home
        if (!isAuthorized) {
            router.replace("/")
        }
    }, [isLoading, isAuthorized, pathname, allowedPathsForCurrentRole, router])

    // --- GUARD RENDERING ---
    // Chỉ render khi đã xác thực xong và đang ở đúng Path cho phép
    if (isLoading || !isAuthorized) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <SpinnerStyled />
            </div>
        )
    }

    return (
        <div className="flex justify-between overflow-hidden rounded-2xl max-w-svw md:w-7xl mx-7 bg-linear-to-b from-white/55 to-white/50 backdrop-blur-xl border-r border-primary-100 shadow-2xl">
            <DashboardSidebar />
            <div className="w-full border-l border-divider">{children}</div>
        </div>
    )
}
