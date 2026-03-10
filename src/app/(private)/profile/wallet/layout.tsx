"use client"

import { SpinnerStyled } from "@/components"
import { useRouteAuthorization, useTokenStore } from "@/hooks"
import { usePathname } from "next/navigation"
import React from "react"
import { WALLET_MENU_ITEMS } from "@/constants/menu/walletSidebar"
import { WalletSidebar } from "@/features/wallets/components"

export default function WalletLayout({ children }: { children: React.ReactNode }) {
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || undefined
    const isLoadingRole = roleSlug === undefined

    // Giao phó toàn bộ sinh mệnh cho Hook xử lý
    const { isSafeToRender } = useRouteAuthorization({
        menuItems: WALLET_MENU_ITEMS,
        roleSlug,
        pathname,
        isLoadingRole,
        fallbackUrl: "/",
        rootPath: "/profile/wallet"
    })

    // --- GUARD RENDERING ---
    // Tuyệt đối không render children nếu:
    // - Đang load role
    // - Không có quyền (bao gồm cả Staff đang đứng ở /dashboard đợi redirect)
    if (!isSafeToRender) {
        return (
            <div className="flex h-screen w-full items-center justify-center backdrop-blur-md">
                <SpinnerStyled />
            </div>
        )
    }

    return (
        <div className="flex justify-between overflow-hidden rounded-2xl max-w-svw md:w-[85vw] mx-7 bg-linear-to-b from-white/55 to-white/50 backdrop-blur-xl border-r border-primary-100 shadow-2xl">
            <WalletSidebar />
            <div className="w-full border-l border-divider">{children}</div>
        </div>
    )
}
