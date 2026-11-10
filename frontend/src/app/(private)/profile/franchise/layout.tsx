"use client"

import { SpinnerStyled } from "@/components"
import { FRANCHISE_MENU_ITEMS } from "@/constants/menu/franchiseSidebar"
import { FranchiseSidebar } from "@/features/franchise/components/FranchiseSidebar"
import { useRouteAuthorization, useTokenStore } from "@/hooks"
import { usePathname } from "next/navigation"
import React from "react"

export default function FranchiseLayout({ children }: { children: React.ReactNode }) {
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || undefined
    const isLoadingRole = roleSlug === undefined

    const { isSafeToRender } = useRouteAuthorization({
        menuItems: FRANCHISE_MENU_ITEMS,
        roleSlug,
        pathname,
        isLoadingRole,
        fallbackUrl: "/",
        rootPath: "/profile/franchise"
    })

    // --- GUARD RENDERING ---
    if (!isSafeToRender) {
        return (
            <div className="flex h-screen w-full items-center justify-center backdrop-blur-md">
                <SpinnerStyled />
            </div>
        )
    }

    return (
        <div className="flex justify-between overflow-hidden rounded-2xl max-w-svw md:w-[85vw] mx-7 bg-linear-to-b from-white/55 to-white/50 backdrop-blur-xl border-r border-primary-100 shadow-2xl">
            <FranchiseSidebar />
            <div className="w-full border-l border-divider">{children}</div>
        </div>
    )
}
