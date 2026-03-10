"use client"

import { useMemo } from "react"
import { Card } from "@heroui/react"
import { BaseSidebar, LogoStyled } from "@/components"
import { RoleSlugLabels } from "@/constants/labels"
import { useTokenStore } from "@/hooks"
import { useGetStoreById } from "@/features/stores"
import { MenuItem } from "@/constants/menu/sidebar"
import { RoleSlug } from "@/constants/enums"

interface DashboardSidebarContentProps {
    isMobile: boolean
    items: MenuItem[]
    pathname: string
    roleSlug: RoleSlug
    onClose: () => void
}

export const DashboardSidebarContent = ({
    isMobile,
    items,
    pathname,
    roleSlug,
    onClose
}: DashboardSidebarContentProps) => {
    // Logic của riêng Dashboard
    const storeId = useTokenStore((s) => s.storeId)
    const { data: store, isLoading } = useGetStoreById({ id: storeId || "", enabled: !!storeId })

    const { title, description } = useMemo(() => {
        if (!store || isLoading) return { title: "Cửa hàng", description: "Management" }
        return { title: store.name, description: store.address }
    }, [isLoading, store])

    return (
        <BaseSidebar
            isMobile={isMobile}
            pathname={pathname}
            items={items}
            onClose={onClose}
            userRole={roleSlug}
            // Nhét UI riêng của Dashboard vào Header
            header={
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                        <LogoStyled className="w-10 h-10" />
                    </div>
                    <div>
                        <h1 className="text-xl font-bold text-primary">{title}</h1>
                        <p className="text-xs text-foreground/60">{description}</p>
                    </div>
                </div>
            }
            // Nhét Profile Card vào Footer
            footer={
                <Card className="space-y-2 p-4 bg-linear-to-br from-primary-50 to-primary-100/50 border-none shadow-none">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center text-white font-bold uppercase">
                            {roleSlug?.charAt(0) || "U"}
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold truncate capitalize">{roleSlug}</p>
                            <p className="text-xs text-foreground/60">{RoleSlugLabels[roleSlug]}</p>
                        </div>
                    </div>
                </Card>
            }
        />
    )
}
