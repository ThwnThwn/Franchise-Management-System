"use client"

import { BaseSidebar } from "@/components"
import { FRANCHISE_MENU_ITEMS } from "@/constants/menu/franchiseSidebar"
import { useTokenStore } from "@/hooks"
import { usePathname } from "next/navigation"

interface FranchiseSidebarProps {
    isMobile?: boolean
    onClose?: () => void
}

export const FranchiseSidebar = ({
    isMobile = false,
    onClose = () => {}
}: FranchiseSidebarProps) => {
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || undefined

    return (
        <aside className="min-w-60 bg-background h-full border-r border-divider flex-col">
            <BaseSidebar
                isMobile={isMobile}
                pathname={pathname}
                items={FRANCHISE_MENU_ITEMS}
                onClose={onClose}
                userRole={roleSlug}
                header={
                    <div className="bg-primary-50 p-4 rounded-2xl border border-primary-100 mb-2">
                        <p className="text-[10px] uppercase font-bold text-primary-600 tracking-wider">
                            Hợp đồng chuyển nhượng
                        </p>
                    </div>
                }
                // footer={
                //     <div className="text-center">
                //         <p className="text-[10px] text-default-400">Hỗ trợ: 1900 xxxx</p>
                //     </div>
                // }
            />
        </aside>
    )
}
