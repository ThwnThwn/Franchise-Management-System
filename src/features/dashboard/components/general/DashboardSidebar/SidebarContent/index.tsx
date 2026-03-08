"use client"

import { useMemo } from "react"
import { Card } from "@heroui/react"
import { motion } from "framer-motion"
import { X } from "lucide-react"
import { LogoStyled } from "@/components"
import { RoleSlugLabels } from "@/constants/labels"
import { MenuItem } from "@/constants/menu/sidebar"
import { RoleSlug } from "@/constants/enums"
import { useTokenStore } from "@/hooks"
import { useGetStoreById } from "@/features/stores"
import { CHB } from "@/constants/constants"
import { SidebarItem } from "./SidebarItem"

interface SidebarContentProps {
    isMobile: boolean
    items: MenuItem[]
    pathname: string
    roleSlug: RoleSlug
    onClose: () => void
}
export const SidebarContent = ({
    isMobile,
    items,
    pathname,
    roleSlug,
    onClose
}: SidebarContentProps) => {
    const storeId = useTokenStore((s) => s.storeId)
    const { data: store, isLoading } = useGetStoreById({ id: storeId || "", enabled: !!storeId })

    const { title, description } = useMemo(() => {
        if (!store || isLoading) {
            return {
                title: CHB,
                description: "Management"
            }
        }

        return {
            title: store.name,
            description: store.address
        }
    }, [isLoading, store])

    return (
        <div className="flex flex-col h-full relative">
            <div className="p-6 flex-1 flex flex-col min-h-0">
                {/* Logo Section (GIỮ NGUYÊN) */}
                <div className="flex items-center justify-between mb-4 pt-2">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                            <LogoStyled className="w-10 h-10" />
                        </div>
                        <div>
                            <h1 className="text-xl font-bold text-primary">{title}</h1>
                            <p className="text-xs text-foreground/60">{description}</p>
                        </div>
                    </div>
                    {isMobile && (
                        <button
                            onClick={onClose}
                            className="p-1 text-primary hover:bg-primary-50 rounded-lg transition-colors"
                        >
                            <X size={24} />
                        </button>
                    )}
                </div>

                {/* Navigation Menu (ĐÃ ĐỔI SANG DÙNG COMPONENT SidebarItem) */}
                <nav className="flex-1 space-y-2 overflow-y-auto custom-scrollbar pr-2">
                    {items.map((item, index) => (
                        <motion.div
                            key={item.label}
                            initial={isMobile ? { opacity: 0, x: -20 } : false}
                            animate={isMobile ? { opacity: 1, x: 0 } : false}
                            transition={{ duration: 0.3, delay: index * 0.05 }}
                        >
                            <SidebarItem
                                item={item}
                                pathname={pathname}
                                isMobile={isMobile}
                                onClose={onClose}
                            />
                        </motion.div>
                    ))}
                </nav>
            </div>

            {/* User Profile Sticky Bottom (GIỮ NGUYÊN) */}
            <div className="mt-auto sticky bottom-0 p-6 bg-background/80 backdrop-blur-md border-t border-divider">
                <Card className="space-y-2 p-4 bg-linear-to-br from-primary-50 to-primary-100/50 border-none shadow-none">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center text-white font-bold uppercase shadow-sm">
                            {roleSlug?.charAt(0) || "U"}
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold text-foreground truncate capitalize">
                                {roleSlug || "User"}
                            </p>
                            <p className="text-xs text-foreground/60">{RoleSlugLabels[roleSlug]}</p>
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    )
}
