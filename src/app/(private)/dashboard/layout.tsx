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

    // 1. Lấy danh sách các href mà role này được phép vào
    const allowedPaths = useMemo(() => {
        if (!roleSlug) return []
        return DASHBOARD_SIDEBAR_MENU.filter((item) => item.allowedRoles.includes(roleSlug)).map(
            (item) => item.href
        )
    }, [roleSlug])

    // 2. Kiểm tra quyền (Strict Mode)
    const isAuthorized = useMemo(() => {
        if (isLoading) return false

        // CHỈ cho phép true nếu path hiện tại khớp hoàn toàn hoặc là con của path trong menu
        return allowedPaths.some((path) => pathname === path || pathname.startsWith(path + "/"))
    }, [isLoading, pathname, allowedPaths])

    useEffect(() => {
        if (isLoading) return

        // 3. Xử lý Redirect cho root /dashboard
        if (pathname === "/dashboard") {
            // Nếu role này không có quyền vào chính trang "/dashboard" (như Staff)
            if (!allowedPaths.includes("/dashboard")) {
                const firstPage = allowedPaths[0] || "/"
                router.replace(firstPage)
            }
            return
        }

        // 4. Nếu vào path lạ/không có quyền, đá về Home
        if (!isAuthorized) {
            router.replace("/")
        }
    }, [isLoading, isAuthorized, pathname, allowedPaths, router])

    // --- GUARD RENDERING ---
    // Tuyệt đối không render children nếu:
    // - Đang load role
    // - Không có quyền (bao gồm cả Staff đang đứng ở /dashboard đợi redirect)
    if (isLoading || !isAuthorized) {
        return (
            <div className="flex h-screen w-full items-center justify-center bg-white/50 backdrop-blur-md">
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
