"use client"

import { SpinnerStyled } from "@/components"
import { PROFILE_MENU_ITEMS } from "@/constants/menu/profileSidebar"
import { ProfileSidebar } from "@/features/profile/components"
import { useRouteAuthorization } from "@/hooks"
import { usePathname } from "next/navigation"
import React from "react"

export default function AccountLayout({ children }: { children: React.ReactNode }) {
    const pathname = usePathname()

    const { isSafeToRender } = useRouteAuthorization({
        menuItems: PROFILE_MENU_ITEMS,
        pathname,
        fallbackUrl: "/",
        rootPath: "/profile"
    })

    if (!isSafeToRender) {
        return (
            <div className="flex h-screen w-full items-center justify-center backdrop-blur-md">
                <SpinnerStyled />
            </div>
        )
    }

    return (
        <div className="flex justify-between overflow-hidden rounded-2xl max-w-svw md:w-[85vw] mx-7 bg-linear-to-b from-white/55 to-white/50 backdrop-blur-xl border-r border-primary-100 shadow-2xl">
            <ProfileSidebar />
            <div className="w-full border-l border-divider">{children}</div>
        </div>
    )
}
