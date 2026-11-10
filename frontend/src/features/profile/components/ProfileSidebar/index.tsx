"use client"

import { BaseSidebar } from "@/components"
import { PROFILE_MENU_ITEMS } from "@/constants/menu/profileSidebar"
import { usePathname } from "next/navigation"

interface ProfileSidebarProps {
    isMobile?: boolean
    onClose?: () => void
}

export const ProfileSidebar = ({ isMobile = false, onClose = () => {} }: ProfileSidebarProps) => {
    const pathname = usePathname()

    return (
        <aside className="min-w-60 bg-background h-full border-r border-divider flex-col">
            <BaseSidebar
                isMobile={isMobile}
                pathname={pathname}
                items={PROFILE_MENU_ITEMS}
                onClose={onClose}
            />
        </aside>
    )
}
